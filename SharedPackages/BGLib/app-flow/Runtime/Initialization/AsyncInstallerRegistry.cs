using UnityEngine;

namespace BGLib.AppFlow.Initialization {

    using System.Collections.Generic;
    

    internal class AsyncInstallerRegistry : AsyncInstaller.IInstallerRegistry {

        public readonly List<MonoBehaviour> monoInstallers = new();
        public readonly List<MonoBehaviour> scriptableObjectInstallers = new();

        public void AddMonoInstaller(MonoBehaviour newMonoInstaller) {

            monoInstallers.Add(newMonoInstaller);
        }

        public void AddScriptableObjectInstaller(MonoBehaviour newScriptableObjectInstaller) {

            scriptableObjectInstallers.Add(newScriptableObjectInstaller);
        }
    }
}
