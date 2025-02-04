using System;

namespace BGLib.JiraBridge {

    [Serializable]
    public class UserInfo {

        public string self;
        public string accountId;
        public string accountType;
        public string emailAddress;
        public string displayName;
        public bool active;
        public string timeZone;
        public string locale;
    }
}
