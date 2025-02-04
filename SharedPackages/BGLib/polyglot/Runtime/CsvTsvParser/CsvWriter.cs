namespace BGLib.Polyglot {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public static class CsvWriter {

        public static void AppendRow(string filePath, List<string> row) {

            using (StreamWriter sw = File.AppendText(filePath)) {
                StringBuilder buffer = new StringBuilder(Environment.NewLine);

                AppendRowInternal(buffer, row);

                sw.Write(buffer.ToString());
            }
        }

        private static void AppendRowInternal(StringBuilder buffer, IEnumerable<string> row) {

            foreach (var element in row) {
                if (HasEscapeChars(element)) {
                    buffer.Append('"');
                    AppendElement(buffer, element);
                    buffer.Append('"');
                }
                else {
                    AppendElement(buffer, element);
                }
                buffer.Append(',');
            }
            buffer.Remove(buffer.Length - 1, 1);
        }

        private static void AppendElement(StringBuilder buffer, string element) {

            buffer.Append(element.Replace("\"", "\"\""));
        }

        private static bool HasEscapeChars(string element) {

            return element.Contains(",") || element.Contains("\n") || element.Contains("\r");
        }

        public static void AppendCSVLine(this StringBuilder buffer, IEnumerable<string> values) {

            AppendRowInternal(buffer, values);
            buffer.AppendLine();
        }

        public static void AppendCSVLine(this StringBuilder buffer, params string[] values) {

            AppendRowInternal(buffer, values);
            buffer.AppendLine();
        }
    }
}
