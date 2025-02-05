using System.Threading.Tasks;
using UnityEngine;

namespace BGLib.AppFlow {

    using System.Collections.Generic;
    using Initialization;
    

    public class FeatureAsyncInstaller : AddressablesAsyncInstaller<MonoBehaviour> {

        private const string kFeatureAsyncInstallerLabel = "FeatureInstaller";

        protected override string assetLabelRuntimeKey => kFeatureAsyncInstallerLabel;

        protected override void LoadResourcesBeforeInstall(IList<MonoBehaviour> scriptableObjectInstallers, AsyncInstaller.IInstallerRegistry registry) {

            foreach (var installer in scriptableObjectInstallers) {
                registry.AddScriptableObjectInstaller(installer);
            }
        }


        protected internal override void LoadResourcesBeforeInstall(IInstallerRegistry registry, MonoBehaviour container)
        {
            throw new System.NotImplementedException();
        }

        protected internal override Task LoadResourcesBeforeInstallAsync(IInstallerRegistry registry, MonoBehaviour container)
        {
            throw new System.NotImplementedException();
        }
    }
}
