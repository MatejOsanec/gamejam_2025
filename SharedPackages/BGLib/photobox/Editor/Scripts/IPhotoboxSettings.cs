using UnityEngine;

namespace BGLib.Photobox.Editor {

    public interface IPhotoboxSettings {

        public Vector2Int thumbnailResolution { get; set; }
        public string depthTextureShaderGUID { get; set; }
    }
}
