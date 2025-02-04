namespace BGLib.UnityExtension {

    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class InfoBoxAttribute : PropertyAttribute {

        public enum Type {

            /// <summary>
            ///   <para>Neutral message.</para>
            /// </summary>
            None,
            /// <summary>
            ///   <para>Info message.</para>
            /// </summary>
            Info,
            /// <summary>
            ///   <para>Warning message.</para>
            /// </summary>
            Warning,
            /// <summary>
            ///   <para>Error message.</para>
            /// </summary>
            Error,
        }

        public readonly string info;
        public readonly Type messageType;

        public InfoBoxAttribute(string info, Type messageType) {

            this.info = info;
            this.messageType = messageType;
        }
    }
}
