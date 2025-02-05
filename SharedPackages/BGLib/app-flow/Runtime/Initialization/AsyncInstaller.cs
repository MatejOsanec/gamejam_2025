using UnityEngine;

namespace BGLib.AppFlow.Initialization {

    using System.Threading.Tasks;
    

    public abstract class AsyncInstaller : MonoBehaviour {

        protected internal interface IInstallerRegistry {

            void AddMonoInstaller(MonoBehaviour newMonoInstaller);
            void AddScriptableObjectInstaller(MonoBehaviour newScriptableObjectInstaller);
        }

        protected internal abstract void LoadResourcesBeforeInstall(IInstallerRegistry registry, MonoBehaviour container);
        protected internal abstract Task LoadResourcesBeforeInstallAsync(IInstallerRegistry registry, MonoBehaviour container);
    }
}
