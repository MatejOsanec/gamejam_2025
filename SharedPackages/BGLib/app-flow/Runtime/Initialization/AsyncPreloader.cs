namespace BGLib.AppFlow.Initialization {

    using System.Threading.Tasks;
    using Zenject;

    public abstract class AsyncPreloader: MonoInstaller {
        public abstract Task PreloadAsync();

#if UNITY_EDITOR
        public abstract void PreloadSynchronously();
#endif

    }
}
