using System.Collections.Generic;

public static class LinkedListExtension {

    public static int Index<T>(this LinkedListNode<T> searchNode) {

        var list = searchNode.List;
        var item = searchNode.Value;
        if (item == null) {
            return -1;
        }
        var count = 0;
        for (var node = list.First; node != null; node = node.Next, count++) {
            if (item.Equals(node.Value)) {
                return count;
            }
        }
        return -1;
    }
}
