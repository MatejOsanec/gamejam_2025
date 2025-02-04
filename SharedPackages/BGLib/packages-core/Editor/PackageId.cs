namespace BGLib.PackagesCore.Editor {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public readonly struct QualifiedIdentifier {

        public const string kSectionSeparatorChar = ".";

        private readonly QualifiedIdentifierSection[] _sections;

        public IEnumerable<QualifiedIdentifierSection> sections => _sections;

        private QualifiedIdentifier(QualifiedIdentifierSection[] sections) {

            _sections = sections;
        }

        public static QualifiedIdentifier Parse(string identifier) {

            var sections = identifier.Split(kSectionSeparatorChar).Select(QualifiedIdentifierSection.Parse).ToArray();
            return new QualifiedIdentifier(sections);
        }

        public QualifiedIdentifier GetSubIdentifier(int offset, int length) {

            var newIdentifier = new QualifiedIdentifierSection[length];
            Array.Copy(_sections, offset, newIdentifier, 0, length);
            return new QualifiedIdentifier(newIdentifier);
        }

        public int Length => _sections.Length;
    }

    public readonly struct QualifiedIdentifierSection {

        public const string kWordSeparatorChar = "-";

        private readonly string[] _words;

        public IEnumerable<string> words => _words;

        private QualifiedIdentifierSection(string[] words) {

            _words = words;
        }

        public static QualifiedIdentifierSection Parse(string section) {

            string[] words = section.Split(kWordSeparatorChar);
            return new QualifiedIdentifierSection(words);
        }
    }

    public readonly struct PackageId {

        private readonly QualifiedIdentifier identifier;
        public readonly PackageType type;
        private readonly string id;

        public PackageId(string name, PackageType type) : this(QualifiedIdentifier.Parse(name), type) {

            id = name;
        }

        private PackageId(QualifiedIdentifier identifier, PackageType type) {

            this.identifier = identifier;
            this.type = type;
            id = FormatName(
                identifier,
                s => s,
                QualifiedIdentifierSection.kWordSeparatorChar,
                QualifiedIdentifier.kSectionSeparatorChar
            ).ToString();
        }

        private static StringBuilder FormatName(
            QualifiedIdentifier identifier,
            Func<string, string> wordTransform,
            string wordSeparator,
            string sectionSeparator
        ) {

            var sb = new StringBuilder();
            foreach (var section in identifier.sections) {
                foreach (string word in section.words) {
                    sb.Append(wordTransform(word));
                    sb.Append(wordSeparator);
                }
                if (wordSeparator.Length > 0) {
                    sb.Remove(sb.Length - wordSeparator.Length, wordSeparator.Length);
                }
                sb.Append(sectionSeparator);
            }
            if (sectionSeparator.Length > 0) {
                sb.Remove(sb.Length - sectionSeparator.Length, sectionSeparator.Length);
            }
            return sb;
        }

        public string GetDisplayName() {

            var targetIdentifier = type == PackageType.ThirdParty ? GetLastIdentifierSection() : identifier;
            return FormatName(
                targetIdentifier,
                System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase,
                wordSeparator: "",
                sectionSeparator: "/"
            ).ToString();
        }

        public string GetRuntimeAssemblyName() => GetAssemblyName(AssemblyDefinitionFile.Type.Runtime);

        internal string GetAssemblyName(AssemblyDefinitionFile.Type assemblyFileType) {

            var assemblyName = FormatName(
                identifier,
                System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase,
                wordSeparator: "",
                sectionSeparator: QualifiedIdentifier.kSectionSeparatorChar
            );
            if (type != PackageType.ThirdParty) {
                assemblyName.Insert(0, QualifiedIdentifier.kSectionSeparatorChar);
                assemblyName.Insert(0, type.ToString());
            }
            if (assemblyFileType == AssemblyDefinitionFile.Type.Runtime) {
                return assemblyName.ToString();
            }
            assemblyName.Append(QualifiedIdentifier.kSectionSeparatorChar);
            assemblyName.Append(AssemblyDefinitionFile.GetSuffixName(assemblyFileType));
            return assemblyName.ToString();
        }

        public string GetTitle() {

            var targetIdentifier = type == PackageType.ThirdParty ? GetLastIdentifierSection() : identifier;
            return FormatName(
                targetIdentifier,
                System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase,
                wordSeparator: " ",
                sectionSeparator: " | "
            ).ToString();
        }

        private QualifiedIdentifier GetLastIdentifierSection() {

            return identifier.GetSubIdentifier(identifier.Length - 1, 1);
        }

        public override string ToString() {

            return id;
        }

        public string GetFullName() {

            return type switch {
                PackageType.BeatSaber =>
                    $"{Constants.kCommercialPrefix}.{Constants.kCompanyName}.{Constants.kBeatSaberName}.{id}",
                PackageType.BGLib =>
                    $"{Constants.kCommercialPrefix}.{Constants.kCompanyName}.{Constants.kBGLibName}.{id}",
                _ => id
            };
        }
    }
}
