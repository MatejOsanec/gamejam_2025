namespace HMUI {

    using UnityEngine;

    /// <summary>
    /// A definition for an overlay over a view controller. This could be a pop-up dialog box, or a list view for a dropdown.
    /// Different definitions exist within different build configurations, therefore we use this base class to generically refer to them.
    /// </summary>
    public abstract class ModalViewBase : MonoBehaviour {

        public abstract event System.Action blockerClickedEvent;

        public abstract void Show(bool animated, bool moveToCenter = false, System.Action finishedCallback = null);
        public abstract void Hide(bool animated, System.Action finishedCallback = null);
    }
}
