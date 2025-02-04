namespace BGLib.MetaRemoteAssets {

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRemoteCatalogLoader {

        public Task<bool> LoadRemoteCatalogAsync(CancellationToken cancellationToken);
    }
}
