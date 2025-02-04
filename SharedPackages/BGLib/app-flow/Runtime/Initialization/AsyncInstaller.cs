namespace BGLib.AppFlow.Initialization {

    using System.Threading.Tasks;
    using Zenject;

    public abstract class AsyncInstaller : MonoInstaller {

        protected internal interface IInstallerRegistry {

            void AddMonoInstaller(MonoInstaller newMonoInstaller);
            void AddScriptableObjectInstaller(ScriptableObjectInstaller newScriptableObjectInstaller);
        }

        protected internal abstract void LoadResourcesBeforeInstall(IInstallerRegistry registry, DiContainer container);
        protected internal abstract Task LoadResourcesBeforeInstallAsync(IInstallerRegistry registry, DiContainer container);
    }
}
