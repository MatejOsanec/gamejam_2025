namespace BGLib.PackagesCore.Editor {

    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class CodeFileHandler {

        private readonly Regex _ifLineRegex;
        private readonly Regex _conditionalLineRegex;
        private readonly Regex _defineLineRegex;
        private readonly IFileSystem _fileSystem;

        private const string kSymbolRegexId = "symbol";

        public CodeFileHandler(IFileSystem fileSystem) {

            _ifLineRegex = new Regex(
                @$"^\s*#\s*(el)?if((\s|\|\||!|&&|\(|\))*(?<{kSymbolRegexId}>\w+))+",
                RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.Compiled
            );
            _conditionalLineRegex = new Regex(
                @$"(?<=\[[^\]]*)Conditional\s*\(""\s*(?<{kSymbolRegexId}>\w+)""",
                RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.Compiled
            );
            _defineLineRegex = new Regex(
                @$"#\s*(define|undef)\s+(?<{kSymbolRegexId}>\w+)",
                RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.Compiled
            );
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> ExtractDefineSymbols(string filePath) {

            var result = new HashSet<string>();
            var localDefines = new HashSet<string>();
            using var fileStream = _fileSystem.File.OpenRead(filePath);
            using var streamReader = new StreamReader(fileStream);
            var line = streamReader.ReadLine();
            while (line != null) {
                bool foundSymbol = false;
                foreach (var usedSymbol in ExtractDefineSymbolUsage(line)) {
                    result.Add(usedSymbol);
                    foundSymbol = true;
                }
                if (!foundSymbol) {
                    foreach (var localSymbol in GetGroupCaptures(_defineLineRegex.Match(line))) {
                        localDefines.Add(localSymbol);
                    }
                }
                line = streamReader.ReadLine();
            }
            result.ExceptWith(localDefines);
            return result;
        }

        private IEnumerable<string> ExtractDefineSymbolUsage(string line) {

            var ifLineCaptures = GetGroupCaptures(_ifLineRegex.Match(line));
            bool hasIf = false;
            foreach (var capture in ifLineCaptures) {
                hasIf = true;
                yield return capture;
            }
            if (hasIf) {
                yield break;
            }
            var conditionalCapture = GetGroupCaptures(_conditionalLineRegex.Match(line));
            foreach (var capture in conditionalCapture) {
                yield return capture;
            }
        }

        private static IEnumerable<string> GetGroupCaptures(Match match) {

            while (match.Success) {
                if (match.Groups[kSymbolRegexId].Success) {
                    foreach (Capture? capture in match.Groups[kSymbolRegexId].Captures) {
                        if (capture != null) {
                            yield return capture.Value;
                        }
                    }
                }
                match = match.NextMatch();
            }
        }
    }
}
