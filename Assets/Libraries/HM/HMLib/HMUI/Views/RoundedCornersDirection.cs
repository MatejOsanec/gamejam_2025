namespace HMUI {

    using UnityEngine;

    public enum RoundedCornersDirection {
        All,
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft
    }

    public static class RoundedCornersDirectionExtensions {

        public static Vector4 GetFlipAndSymmetry(this RoundedCornersDirection direction) {

            switch (direction) {
                case RoundedCornersDirection.All: return new Vector4(1.0f, 1.0f, -1.0f, -1.0f);
                case RoundedCornersDirection.Up: return new Vector4(1.0f, -1.0f, -1.0f, 1.0f);
                case RoundedCornersDirection.UpRight: return new Vector4(1.0f, -1.0f, 1.0f, 1.0f);
                case RoundedCornersDirection.Right: return new Vector4(1.0f, 1.0f, 1.0f, -1.0f);
                case RoundedCornersDirection.DownRight: return new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                case RoundedCornersDirection.Down: return new Vector4(1.0f, 1.0f, -1.0f, 1.0f);
                case RoundedCornersDirection.DownLeft: return new Vector4(-1.0f, 1.0f, 1.0f, 1.0f);
                case RoundedCornersDirection.Left: return new Vector4(-1.0f, 1.0f, 1.0f, -1.0f);
                case RoundedCornersDirection.UpLeft: return new Vector4(-1.0f, -1.0f, 1.0f, 1.0f);
            }

            return Vector4.one;
        }
    }
}
