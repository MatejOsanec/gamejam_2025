namespace HMUI {

    using UnityEngine;

    public abstract class ImageViewBase : UnityEngine.UI.Image {

        public abstract bool gradient { get; set; }
        public abstract Color color0 { get; set; }
        public abstract Color color1 { get; set; }
    }
}
