using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace BGLib.JiraBridge {

    public class JiraClient {

        private Settings _settings = null;

        public JiraClient() {

            ReloadSettings();
            Debug.Log("Successfully initialized Jira Client");
        }

        public void ReloadSettings() {

            string filePath = Path.Combine(Application.persistentDataPath, Settings.kFileName);
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir) || !File.Exists(filePath)) {
                throw new Exception($"{filePath} did not exist");
            }

            var text = File.ReadAllText(filePath);
            _settings = JsonConvert.DeserializeObject<Settings>(text);
            if (string.IsNullOrEmpty(_settings.apiUrl)) {
                _settings = null;
                throw new Exception($"'apiUrl' was not defined in {filePath}");
            }

            if (string.IsNullOrEmpty(_settings.apiKeyUser)) {
                _settings = null;
                throw new Exception($"'apiKeyUser' was not defined in {filePath}");
            }

            if (string.IsNullOrEmpty(_settings.apiKey)) {
                _settings = null;
                throw new Exception($"'apiKey' was not defined in {filePath}");
            }

            if (string.IsNullOrEmpty(_settings.projectKey)) {
                _settings = null;
                throw new Exception($"'projectKey' was not defined in {filePath}");
            }
        }

        public async Task<CreateIssueSuccessResponse> CreateTicket(IssueFields issueFields) {

            if (_settings == null) {
                Debug.LogException(new Exception("No settings file was loaded"));
                return null;
            }

            try {
                Issue issue = new();
                issue.fields = issueFields;
                if (string.IsNullOrEmpty(issue.fields.project.key)) {
                    issue.fields.project.key = _settings.projectKey;
                }

                var serialized = JsonConvert.SerializeObject(issue, new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore
                });
                var request = UnityWebRequest.Post($"{_settings.apiUrl}/issue", serialized, "application/json");
                request.SetRequestHeader("Authorization", _settings.GetAuthHeader());

                var result = await request.SendWebRequestAsync();
                if (result != UnityWebRequest.Result.Success) {
                    Debug.LogError($"{request.error}: {request.downloadHandler.text}");
                    return null;
                }

                return JsonConvert.DeserializeObject<CreateIssueSuccessResponse>(request.downloadHandler.text);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }

            return null;
        }

        /// <summary>
        /// Uploads attachments to an existing ticket
        /// </summary>
        /// <param name="ticketKey">ticket to attach data to</param>
        /// <param name="attachments">keyValuePair of filename (use .extension or name.extension) and binary data</param>
        /// <returns>null if failed</returns>
        public async Task<List<UploadAttachmentSuccessResponse>> UploadAttachmentsToTicket(string ticketKey, Dictionary<string, byte[]> attachments) {

            if (_settings == null) {
                Debug.LogException(new Exception("No settings file was loaded"));
                return null;
            }

            try {
                WWWForm form = new();
                string prefix = $"{Application.platform}_{GetIdentifier()}_{DateTime.UtcNow:yy-MM-dd-HH-mm-ss}";
                foreach (var attachment in attachments) {
                    form.AddBinaryData("file", attachment.Value, $"{prefix}_{attachment.Key}");
                    // string could use: new UTF8Encoding().GetBytes(playerDataString)
                    // Texture2d could use: screenshot.EncodeToPNG()
                }

                var request = UnityWebRequest.Post($"{_settings.apiUrl}/issue/{ticketKey}/attachments", form);
                request.SetRequestHeader("Authorization", _settings.GetAuthHeader());
                request.SetRequestHeader("X-Atlassian-Token", "no-check");

                var result = await request.SendWebRequestAsync();
                if (result != UnityWebRequest.Result.Success) {
                    Debug.LogError($"{request.error}: " + request.downloadHandler.text);
                    return null;
                }

                var response = JsonConvert.DeserializeObject<List<UploadAttachmentSuccessResponse>>(request.downloadHandler.text);
                return response;
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }

            return null;
        }

        public async Task<List<UserInfo>> GetUsers() {

            if (_settings == null) {
                Debug.LogException(new Exception("No settings file was loaded"));
                return null;
            }

            try {
                var request = UnityWebRequest.Get($"{_settings.apiUrl}/user/assignable/search?project={_settings.projectKey}&maxResults={Int32.MaxValue}");
                request.SetRequestHeader("Authorization", _settings.GetAuthHeader());
                request.SetRequestHeader("X-Atlassian-Token", "no-check");

                var result = await request.SendWebRequestAsync();
                if (result != UnityWebRequest.Result.Success) {
                    Debug.LogError($"{request.error}: " + request.downloadHandler.text);
                    return null;
                }

                var response = JsonConvert.DeserializeObject<List<UserInfo>>(request.downloadHandler.text);
                return response;
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }

            return null;
        }

        public bool IsLoggedInUser(string emailAddress)
            => string.Equals(_settings.apiKeyUser, emailAddress);

        private string GetIdentifier()
            => _settings.customDeviceIdentifier ?? SystemInfo.deviceName;
    }
}
