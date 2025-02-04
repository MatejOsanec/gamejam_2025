using System;
using System.Collections.Generic;

public static class ListExtensions {


    public static int IndexOf<T>(this IReadOnlyList<T> self, T item) {

        var i = 0;
        foreach (T element in self) {
            if (Equals(element, item)) {
                return i;
            }
            i++;
        }
        return -1;
    }

    // Taken from C# Core Lib and modified for IReadOnlyList
    public static int FindIndex<T>(this IReadOnlyList<T> list, Predicate<T> match) {

        for (int index = 0; index < list.Count; index++) {
            if (match(list[index])) {
                return index;
            }
        }
        return -1;
    }

    public static void InsertIntoSortedListFromEnd<T>(this List<T> sortedList, T newItem) where T : IComparable<T> {

        if (sortedList.Count == 0) {
            sortedList.Add(newItem);
            return;
        }
        else if (sortedList[sortedList.Count - 1].CompareTo(newItem) < 0) {
            sortedList.Add(newItem);
            return;
        }
        else {
            for (int i = sortedList.Count - 2; i >= 0; i--) {

                if (sortedList[i].CompareTo(newItem) < 0) {
                    sortedList.Insert(i + 1, newItem);
                    return;
                }
            }
        }

        sortedList.Insert(0, newItem);
    }
}
