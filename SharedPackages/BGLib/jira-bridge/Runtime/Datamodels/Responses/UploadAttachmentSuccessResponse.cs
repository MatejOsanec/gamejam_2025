using System;

namespace BGLib.JiraBridge {

    [Serializable]
    public class UploadAttachmentSuccessResponse {

        public string self;
        public string id;
        public string filename;
        public string created;
        public long size;
        public string mimetype;
        public string content;
        public string thumbnail;
    }
}
