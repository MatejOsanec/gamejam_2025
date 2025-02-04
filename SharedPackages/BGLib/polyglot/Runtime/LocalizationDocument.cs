namespace BGLib.Polyglot {
    
    using System;
    using UnityEngine;

    [Serializable]
    public class LocalizationDocument {
        
        [SerializeField]
        private string docsId = string.Empty;
        [SerializeField]
        private string sheetId = string.Empty;
        [SerializeField]
        private GoogleDriveDownloadFormat format;
        [SerializeField]
        private TextAsset textAsset = default!;

        public TextAsset TextAsset => textAsset;

        public string DocsId => docsId;

        public string SheetId => sheetId;

        public GoogleDriveDownloadFormat Format => format;
    }
}
