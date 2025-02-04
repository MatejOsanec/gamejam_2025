using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using Debug = UnityEngine.Debug;

public interface IVerboseLogger {

    public string loggerPrefix { get; }
}

public static class GenericLogger {

    public const string kVerboseLogDefineSymbol = "BS_VERBOSE_LOGGING";

    private static string Format(this IVerboseLogger logger, string message) {

        return $"[{logger.loggerPrefix}] {message}";
    }

    [Conditional(kVerboseLogDefineSymbol)]
    public static void Log(this IVerboseLogger logger, string message) {

        Debug.Log(logger.Format(message));
    }

    [Conditional(kVerboseLogDefineSymbol)]
    public static void Log<T>(this T logger, string message) where T : MonoBehaviour, IVerboseLogger {

        Debug.Log(logger.Format(message), logger.gameObject);
    }

    public static void LogWithTimestamp(string message) {

        Debug.Log($"[{DateTime.Now.ToString("s", CultureInfo.InvariantCulture)}] {message}");
    }

    /// <summary>
    /// Stopwatch for estimation an execution time within the scope
    /// Usage in C#8: <c>using new ScopedStopwatch('Rebuilding assets');</c>
    /// Prints the process elapsed time at the end of the scope execution
    /// In a batch mode, prints a start message additionally
    /// </summary>
    public class ScopedStopwatch : IDisposable {

        private readonly string _processName;
        private readonly Stopwatch _stopwatch;

        public ScopedStopwatch(string processName) {

            _processName = processName;
            _stopwatch = Stopwatch.StartNew();

            if (Application.isBatchMode) {
                LogWithTimestamp($"{processName} started");
            }
        }

        public void Dispose() {

            _stopwatch.Stop();
            LogWithTimestamp($"{_processName} finished, it took {_stopwatch.Elapsed.TotalSeconds}s");
        }
    }
}
