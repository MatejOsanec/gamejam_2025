namespace HMUI {

    using UnityEngine.UI;

    public class Touchable : Graphic {

#if !BS_TOURS
        [UnityEngine.SerializeField] float _skew = 0.0f;

        public float skew => _skew;
#endif

        protected override void OnPopulateMesh(VertexHelper vh) {

            vh.Clear();
        }
    }
}
