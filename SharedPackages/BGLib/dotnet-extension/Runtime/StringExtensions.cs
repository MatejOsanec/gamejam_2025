#nullable enable

public static class StringExtensions {

    public static string Truncate(this string s, int length, bool appendEllipsis = false) {

        if (string.IsNullOrEmpty(s)) {
            return s;
        }

        return s.Length <= length ?
            s :
            (appendEllipsis ?
                $"{s.Substring(0, length - 3)}..." :
                s.Substring(0, length));
    }

    public static bool Contains(this string? source, string substring, System.StringComparison comp) {

        return source?.IndexOf(substring, comp) >= 0;
    }

    /// <summary>
    /// Returns a value indicating whether a specified substring occurs within any string present in the array.<br/>
    /// </summary>
    public static bool AnyContains(this string[] stringArray, string value, System.StringComparison stringComparison) {

        bool result = false;
        foreach (string s in stringArray) {
            result = s.Contains(value, stringComparison);

            if (result == true) {
                break;
            }
        }

        return result;
    }
}
