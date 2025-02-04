using System;
using System.Text;

namespace BGLib.JiraBridge {

    [Serializable]
    internal class Settings {

        public static string kFileName = "jira-settings.json";

        /// <summary>
        /// Full API URL, eg https://beatgames.atlassian.net/rest/api/2
        /// </summary>
        public string apiUrl = "https://beatgames.atlassian.net/rest/api/2";

        /// <summary>
        /// Usually, the email address of the user. EG ddebruijne@meta.com
        /// </summary>
        public string apiKeyUser;

        /// <summary>
        /// Generated from https://id.atlassian.com/manage-profile/security/api-tokens
        /// </summary>
        public string apiKey;

        /// <summary>
        /// Key of the project you want to to perform your modifications on
        /// </summary>
        public string projectKey;

        /// <summary>
        /// Normally an identifier is generated, but if you want a more human-readable version you can re-define it here.
        /// </summary>
        public string customDeviceIdentifier;

        public string GetAuthHeader() {
            string toEncode = $"{apiKeyUser}:{apiKey}";
            return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(toEncode))}";
        }
    }
}
