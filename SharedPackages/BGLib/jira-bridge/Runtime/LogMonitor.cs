using System;
using System.Text;
using UnityEngine;

namespace BGLib.JiraBridge {

    public class LogMonitor {

        private StringBuilder _log;

        public LogMonitor() {

            _log = new StringBuilder();
            Application.logMessageReceivedThreaded += HandleLog;
        }

        ~LogMonitor() {

            Application.logMessageReceivedThreaded -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type) {

            _log.AppendLine($"{GetFormattedDateTime()} {GetLogTypePrefix(type)} {logString}");
            if (type != LogType.Log) {
                _log.AppendLine($"{GetFormattedDateTime()} {GetLogTypePrefix(type)} {stackTrace}");
            }
        }

        private string GetLogTypePrefix(LogType type)
            => type switch {
                LogType.Error => "[E]",
                LogType.Assert => "[A]",
                LogType.Warning => "[W]",
                LogType.Log => "[L]",
                LogType.Exception => "[E]",
                _ => throw new NotImplementedException(),
            };

        private string GetFormattedDateTime()
            => $"[{DateTime.UtcNow:yy-MM-dd HH:mm:ss}]";

        public string GetLog() {

            return _log.ToString();
        }
    }
}
