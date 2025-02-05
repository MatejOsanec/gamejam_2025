using UnityEngine;

namespace BGLib.AppFlow.Initialization {

    using System.Threading.Tasks;
    

    public abstract class AsyncPreloader: MonoBehaviour {
        public abstract Task PreloadAsync();

#if UNITY_EDITOR
        public abstract void PreloadSynchronously();
#endif

    }
}
