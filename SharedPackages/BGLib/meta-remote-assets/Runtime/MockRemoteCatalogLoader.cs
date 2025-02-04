namespace BGLib.MetaRemoteAssets {

    using System.Threading;
    using System.Threading.Tasks;
    public class MockRemoteCatalogLoader : IRemoteCatalogLoader {

        public Task<bool> LoadRemoteCatalogAsync(CancellationToken cancellationToken) {

            return Task.FromResult(true);
        }
    }
}
