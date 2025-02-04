namespace BGLib.Polyglot {

    using System;
    using UnityEngine;

    [Serializable]
    public class LocalizationAsset {

        [SerializeField]
        private TextAsset textAsset;

        [SerializeField]
        private GoogleDriveDownloadFormat format;

        public TextAsset TextAsset => textAsset;

        public GoogleDriveDownloadFormat Format => format;

        internal LocalizationAsset(TextAsset textAsset, GoogleDriveDownloadFormat format) {

            this.textAsset = textAsset;
            this.format = format;
        }
    }
}
