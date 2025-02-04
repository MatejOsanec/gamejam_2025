namespace BGLib.AppFlow {

    using System.Collections.Generic;
    using Initialization;
    using Zenject;

    public class FeatureAsyncInstaller : AddressablesAsyncInstaller<ScriptableObjectInstaller> {

        private const string kFeatureAsyncInstallerLabel = "FeatureInstaller";

        protected override string assetLabelRuntimeKey => kFeatureAsyncInstallerLabel;

        protected override void LoadResourcesBeforeInstall(IList<ScriptableObjectInstaller> scriptableObjectInstallers, AsyncInstaller.IInstallerRegistry registry) {

            foreach (var installer in scriptableObjectInstallers) {
                registry.AddScriptableObjectInstaller(installer);
            }
        }

        public override void InstallBindings() { }
    }
}
