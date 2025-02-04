namespace BGLib.MetaRemoteAssets {

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.AddressableAssets.ResourceLocators;
    using UnityEngine.Networking;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceLocations;
    using Zenject;
    using System.Linq;
    using AddressablesInternalBridge.Runtime;

    public class MetaRemoteAssetsManager: IInitializable, IDisposable {

        // WARNING! This variable name is baked into addressable remote catalog assets' load path, which is resolved in runtime, so renaming it
        // will cause any asset that is served by the remote catalog to fail its download. You can change its value for testing
        // against an OD.
        public const string MetaServerHost = "https://oculus.com";

        public const string kPlatformInjectId = "MetaRemoteAssetsManager_platform_injectId";
        public const string kMetaServerCatalogPath = "beat-saber/remote-assets/download/catalog.json";

        public static string RemoteCatalogPath =>
            $"{MetaServerHost}/{kMetaServerCatalogPath}";

        private string _accessToken;
        private readonly string _platform;
        private readonly CancellationTokenSource _initializationCancellationTokenSource;
        private Task<bool>? _initializationTask;
        private Task? _updateCatalogTask;
        private readonly string _appId;
        private readonly IPlatformUserModel _platformUserModel;
        private readonly IRemoteCatalogLoader _remoteCatalogLoader;
        private PlatformAuthenticationTokenProvider? _platformAuthenticationTokenProvider;

        private readonly string? _inBuildGameVersion;

        public event Action didCatalogLoadOrUpdateEvent = delegate{ };

        public MetaRemoteAssetsManager(
            INetworkConfig networkConfig,
            IPlatformUserModel platformUserModel,
            IRemoteCatalogLoader remoteCatalogLoader,
            [Inject(Id = kPlatformInjectId)] string platform,
            // Using string since it's injected from different assembly (BeatSaber.Init), constant lives in BSAppInit
            [Inject(Id = "InBuildGameVersion")] string inBuildGameVersion
        ) {

            _platformUserModel = platformUserModel;
            _platform = platform;
            _remoteCatalogLoader = remoteCatalogLoader;

            // Default App Access Token that can be valid for cases where we are unable to obtain the User Access Token.
            // This will result in default behaviour which is not scoped in a specific user context.
            _accessToken = networkConfig.graphAccessToken;

            _appId = networkConfig.appId;
            _initializationCancellationTokenSource = new CancellationTokenSource();

            _inBuildGameVersion = inBuildGameVersion;

            ApplyAddressablesOverrides();
        }

        private void ApplyAddressablesOverrides() {

            Addressables.WebRequestOverride = WebRequestOverride;
            Addressables.InternalIdTransformFunc = InternalIdTransformFunc;
        }

        public async void Initialize() {

            try {
                await WaitInitAsync();
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        public async Task<bool> WaitInitAsync() {

            return await (_initializationTask ??= InitializeInternalAsync(_initializationCancellationTokenSource.Token));
        }

        private async Task<bool> InitializeInternalAsync(CancellationToken cancellationToken) {

            try {
                var catalogWasLoaded = await _remoteCatalogLoader.LoadRemoteCatalogAsync(cancellationToken);

                if (catalogWasLoaded) {
                    didCatalogLoadOrUpdateEvent.Invoke();
                }

                return catalogWasLoaded;
            }
            catch (OperationCanceledException) {
                return false;
            }
            catch (Exception e) {
                Debug.LogError($"Unable to initialize MetaRemoteAssetsManager due to exception: {e.Message}");
                return false;
            }
        }

        private string InternalIdTransformFunc(IResourceLocation resourceLocation) {

            var internalId = resourceLocation.InternalId;
            return internalId.StartsWith(MetaServerHost) ? internalId.Replace("\\", "/") : internalId;
        }

        private async Task FetchTokenAsync(CancellationToken cancellationToken) {

            try {
                if (_platformAuthenticationTokenProvider == null) {
                    var userInfo = await _platformUserModel.GetUserInfo(cancellationToken);
                    if (userInfo == null) {
                        return;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    _platformAuthenticationTokenProvider = new PlatformAuthenticationTokenProvider(_platformUserModel, userInfo);
                }

                var xPlatformAccessTokenData = await _platformAuthenticationTokenProvider.GetXPlatformAccessToken(cancellationToken);

                if (xPlatformAccessTokenData.IsValid()) {
                    _accessToken = xPlatformAccessTokenData.token;
                }
            }
            catch (OperationCanceledException) {
                throw;
            }
            catch (Exception e) {
                Debug.LogError($"Unable to fetch the xPlatformAccessToken due to exception: {e.Message}");
                Debug.LogException(e);
            }
        }

        public async Task UpdateCatalogsAsync(CancellationToken cancellationToken) {

            await (_updateCatalogTask ??= UpdateCatalogsInternalAsync(cancellationToken));
        }


        private async Task UpdateCatalogsInternalAsync(CancellationToken cancellationToken) {

            try {
                await FetchTokenAsync(_initializationCancellationTokenSource.Token);

                var updateableCatalogsData = Extensions.GetUpdateableCatalogLocationDatas().ToList();
                if (updateableCatalogsData.Count == 0) {
                    return;
                }

                // The main reason we ad-hoc check if hashes before/after update are different is because CheckForContentUpdates
                // does not use WebRequestOverride, so it fails because it doesn't request for the catalog with the proper
                // http request parameters.
                var hashesBeforeUpdate = new Dictionary<string, string>();
                foreach (var catalogData in updateableCatalogsData) {
                    hashesBeforeUpdate.Add(catalogData.LocatorId, catalogData.LocalHash);
                }

                var updateableCatalogLocatorIds = updateableCatalogsData.Select(catalogData => catalogData.LocatorId).ToList();
                AsyncOperationHandle<List<IResourceLocator>> updateHandle
                    = Addressables.UpdateCatalogs(updateableCatalogLocatorIds);

                cancellationToken.ThrowIfCancellationRequested();
                await updateHandle.Task.WaitAsync(cancellationToken);

                var anyCatalogGotUpdated = hashesBeforeUpdate.Any((entry) => {
                    (string locatorId, string hashBeforeUpdate) = entry;

                    var hashAfterUpdate = Extensions.GetCatalogLocationData(locatorId).LocalHash;
                    return hashAfterUpdate != hashBeforeUpdate;
                });

                if (anyCatalogGotUpdated) {
                    didCatalogLoadOrUpdateEvent.Invoke();
                }

            }
            catch (OperationCanceledException) {
                Debug.Log("[RemoteAssets] Cancelling catalog update");
            }
            finally {
                _updateCatalogTask = null;
            }
        }


        public void Dispose() {

            if (_initializationTask == null) {
                return;
            }
            if (_initializationTask.IsCompleted) {
                _initializationTask.Dispose();
            }
            else {
                _initializationCancellationTokenSource.Cancel();
            }
        }

        private void WebRequestOverride(UnityWebRequest request) {

            if (request.url.StartsWith(MetaServerHost)) {
                request.url = request.url.Replace("\\", "/") +
                              $"/?access_token={_accessToken}&platform={_platform}&app_id={_appId}&client_version={_inBuildGameVersion}";
            }
        }


        private class AddResourceLocatorInput {

            public IResourceLocator ResourceLocator;
            public string LocalHash;
            public IResourceLocation CatalogLocation;

            public AddResourceLocatorInput(IResourceLocator resourceLocator, string localHash, IResourceLocation catalogLocation) {

                ResourceLocator = resourceLocator;
                LocalHash = localHash;
                CatalogLocation = catalogLocation;
            }
        }

        private static AddResourceLocatorInput CreateAddResourceLocatorInput(IResourceLocator resourceLocator) {

            var catalogLocationData = Extensions.GetCatalogLocationData(resourceLocator.LocatorId);
            return new(resourceLocator, catalogLocationData.LocalHash, catalogLocationData.CatalogLocation);
        }

        public static void MakeRemoteCatalogTopPriority() {

            var resourceLocators = Addressables.ResourceLocators.ToList();
            var allAddResourceLocatorInputs = new List<AddResourceLocatorInput>();
            var otherAddResourceLocatorInputs = new List<AddResourceLocatorInput>();

            for (int i = 0; i < resourceLocators.Count; i++) {
                var resourceLocator = resourceLocators[i];

                if (resourceLocator.LocatorId == RemoteCatalogPath) {
                    allAddResourceLocatorInputs.Add(CreateAddResourceLocatorInput(resourceLocator));

                    // Addressables always adds a dynamic resource locator for atlas sprites after a new catalog load.
                    // This is so we ensure both resource locators are added to the locators in correct order.
                    resourceLocator = resourceLocators[i+1];
                    allAddResourceLocatorInputs.Add(CreateAddResourceLocatorInput(resourceLocator));

                    i++;
                    continue;
                }

                otherAddResourceLocatorInputs.Add(CreateAddResourceLocatorInput(resourceLocator));
            }

            allAddResourceLocatorInputs.AddRange(otherAddResourceLocatorInputs);

            Addressables.ClearResourceLocators();
            foreach (var input in allAddResourceLocatorInputs) {
                Addressables.AddResourceLocator(input.ResourceLocator, input.LocalHash, input.CatalogLocation);
            }
        }
    }
}
