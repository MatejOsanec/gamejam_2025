namespace AddressablesInternalBridge.Runtime {

    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.ResourceLocations;
    using System.Collections.Generic;
    using System.Linq;

    public static class Extensions {

        public static CatalogLocationData GetCatalogLocationData (string locatorId) {

            var locatorInfo = Addressables.Instance.GetLocatorInfo(locatorId);
            return locatorId == null ? null : new CatalogLocationData(locatorId, locatorInfo.LocalHash, locatorInfo.CatalogLocation);
        }

        public static IEnumerable<CatalogLocationData> GetUpdateableCatalogLocationDatas() {

            return Addressables.Instance.m_ResourceLocators
                .Where(locatorInfo => locatorInfo.CanUpdateContent)
                .Select(locatorInfo => new CatalogLocationData(locatorInfo.Locator.LocatorId, locatorInfo.LocalHash,
                    locatorInfo.HashLocation));
        }
    }

    public class CatalogLocationData {

        public string LocatorId { get; }
        public string LocalHash { get; }
        public IResourceLocation CatalogLocation { get; }

        public CatalogLocationData(string locatorId, string localHash, IResourceLocation resourceLocation) {

            LocatorId = locatorId;
            LocalHash = localHash;
            CatalogLocation = resourceLocation;
        }
    }
}
