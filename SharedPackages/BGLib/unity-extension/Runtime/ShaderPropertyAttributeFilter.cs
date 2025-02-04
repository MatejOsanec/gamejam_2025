namespace BGLib.UnityExtension {

    using System;

    public class ShaderPropertyAttributeFilter {

        public readonly PropType propType;
        public readonly string nameFilter;

        public enum PropType {
            /// <summary>
            /// <para>Any Property</para>>
            /// </summary>
            Any,
            /// <summary>
            ///   <para>Color property.</para>
            /// </summary>
            Color,
            /// <summary>
            ///   <para>Vector property.</para>
            /// </summary>
            Vector,
            /// <summary>
            ///   <para>Float property.</para>
            /// </summary>
            Float,
            /// <summary>
            ///   <para>Ranged float (with min/max values) property.</para>
            /// </summary>
            Range,
            /// <summary>
            ///   <para>Texture property.</para>
            /// </summary>
            Texture,
            /// <summary>
            ///   <para>Int property.</para>
            /// </summary>
            Int,
        }

        public ShaderPropertyAttributeFilter(string nameFilter, PropType propType) {

            this.propType = propType;
            this.nameFilter = nameFilter;
        }
    }
}
