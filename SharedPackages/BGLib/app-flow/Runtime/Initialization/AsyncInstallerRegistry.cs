namespace BGLib.AppFlow.Initialization {

    using System.Collections.Generic;
    using Zenject;

    internal class AsyncInstallerRegistry : AsyncInstaller.IInstallerRegistry {

        public readonly List<MonoInstaller> monoInstallers = new();
        public readonly List<ScriptableObjectInstaller> scriptableObjectInstallers = new();

        public void AddMonoInstaller(MonoInstaller newMonoInstaller) {

            monoInstallers.Add(newMonoInstaller);
        }

        public void AddScriptableObjectInstaller(ScriptableObjectInstaller newScriptableObjectInstaller) {

            scriptableObjectInstallers.Add(newScriptableObjectInstaller);
        }
    }
}
