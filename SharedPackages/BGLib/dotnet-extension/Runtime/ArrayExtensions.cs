using System;

public static class ArrayExtension {

    public static bool IsValidIndex(this Array array, int index) {

        return index < array.Length && index > -1;
    }
}