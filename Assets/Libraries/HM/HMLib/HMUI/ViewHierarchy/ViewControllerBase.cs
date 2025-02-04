namespace HMUI {

    using UnityEngine;

    public class ViewControllerBase : MonoBehaviour {

        public event DidActivateDelegate didActivateEvent;
        public event DidDeactivateDelegate didDeactivateEvent;

        public delegate void DidActivateDelegate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling);
        public delegate void DidDeactivateDelegate(bool removedFromHierarchy, bool screenSystemDisabling);

        protected void CallDidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {

            didActivateEvent?.Invoke(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        protected void CallDidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {

            didDeactivateEvent?.Invoke(removedFromHierarchy, screenSystemDisabling);
        }
    }
}
