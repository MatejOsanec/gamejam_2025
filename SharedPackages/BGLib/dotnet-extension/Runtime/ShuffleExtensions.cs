using System;
using System.Collections.Generic;
using System.Linq;

public static class ShuffleExtensions  {

    // Fisher-Yates shuffle based on either random or set random generator
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random random) {

        if (source is null) {
            throw new ArgumentNullException(nameof(source), "Cannot compute shuffle on null collection.");
        }

        // Unable to do shuffle in place as we are passing IEnumerable
        var result = new List<T>();
        foreach (var item in source) {
            var index = random.Next(result.Count + 1);
            if (index == result.Count) {
                result.Add(item);
            }
            else {
                result.Add(result[index]);
                result[index] = item;
            }
        }

        return result;
    }

    // Reservoir sampling into source enumerable
    public static IEnumerable<T> PickRandomElementsWithTombstone<T>(this IEnumerable<T> source, int limit, int count, Random random, T tombstone) {

        if (source is null) {
            throw new ArgumentNullException(nameof(source), "Cannot pick random elements on null collection");
        }
        int sourceCount = source.Count();
        if (sourceCount != count) {
            throw new ArgumentException($"Count property needs to equal enumerable count. Source has count: {sourceCount}, but argument is {count}", nameof(count));
        }
        var index = 0;
        var picked = 0;
        foreach (var item in source) {
            var shouldPick = random.Next(count - index) < limit - picked;
            if (shouldPick) {
                picked++;
                yield return item;
            }
            else {
                yield return tombstone;
            }
            index++;
        }
    }

    public static IEnumerable<T> TakeWithTombstone<T>(this IEnumerable<T> source, int limit, T tombstone) {

        if (source is null) {
            throw new ArgumentNullException(nameof(source), "Cannot take elements on null collection");
        }

        using (IEnumerator<T> enumerator = source.GetEnumerator()) {

            var index = 0;
            while (enumerator.MoveNext()) {
                if (index < limit) {
                    yield return enumerator.Current;
                }
                else {
                    yield return tombstone;
                }
                index++;
            }
        }
    }

    public static IEnumerable<(int, int)> ZipSkipTombstone(this IEnumerable<int> collection1, IEnumerable<int> collection2, int collection2Tombstone) {

        if (collection1 is null) {
            throw new ArgumentNullException(nameof(collection1), "Cannot perform Zip with null collection 1.");
        }
        if (collection2 is null) {
            throw new ArgumentNullException(nameof(collection2), "Cannot perform Zip with null collection 2.");
        }

        using IEnumerator<int> enum1 = collection1.GetEnumerator();
        using IEnumerator<int> enum2 = collection2.GetEnumerator();
        while (enum1.MoveNext() && enum2.MoveNext()) {
            if (collection2Tombstone == enum2.Current) {
                continue;
            }

            yield return (enum1.Current, enum2.Current);
        }
    }

    // Taken from https://stackoverflow.com/questions/273313/randomize-a-listt
    public static void ShuffleInPlace<T>(this IList<T> list, Random random) {

        int n = list.Count;
        while (n > 1) {
            n--;
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
