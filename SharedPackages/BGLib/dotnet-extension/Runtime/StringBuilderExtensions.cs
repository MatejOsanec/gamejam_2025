using System;
using System.Text;
using BGLib.DotnetExtension;

public static class StringBuilderExtension {

    private const char kCharZero = '0';

    public static void Swap(this StringBuilder sb, int startIndex, int endIndex) {

        // Swap the integers
        if (endIndex < startIndex) {
            throw new IndexOutOfRangeException($"{nameof(startIndex)}:{startIndex} must be smaller than or equal to {nameof(endIndex)}:{endIndex}");
        }
        int count = (endIndex - startIndex + 1) / 2;
        for (int i = 0; i < count; ++i) {
            (sb[startIndex + i], sb[endIndex - i]) = (sb[endIndex - i], sb[startIndex + i]);
        }
    }

    public static void AppendNumber(this StringBuilder sb, int number) {
        
        number.ToUInt(out uint uNumber, out bool isNegative);
        AppendNumber(sb, uNumber, isNegative);
    }

    public static void AppendNumber(this StringBuilder sb, uint uNumber, bool isNegative = false) {

        // Save the current length as starting index
        int startIndex = sb.Length;

        // Convert 
        do {
            sb.Append((char)(kCharZero + uNumber % 10));
            uNumber /= 10;
        } while (uNumber != 0);
        
        if (isNegative) {
            sb.Append('-');
        }

        sb.Swap(startIndex, sb.Length - 1);
    }
}