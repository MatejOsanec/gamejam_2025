namespace BGLib.DotnetExtension.Collections {

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Least Recently Used Cache.
    /// Whenever you add a new element, if you exceed the maximum number of element, it will remove the least recently
    /// used element.
    /// Whenever you try to get an element, this element will be the most recently used element
    /// </summary>
    /// <typeparam name="TKey">Key to value type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public class LRUCache<TKey, TValue> {

        private class CacheElement {

            private readonly TKey _key;
            public TKey key => _key;
            public TValue value;

            public CacheElement(TKey key, TValue value) {

                _key = key;
                this.value = value;
            }
        }

        public event Action<TKey, TValue>? itemWillBeRemovedFromCacheEvent;

        private readonly LinkedList<CacheElement> _cacheContent;
        private readonly Dictionary<TKey, LinkedListNode<CacheElement>> _index;
        private readonly int _maxNumberElements;

        public LRUCache(int maxNumberElements) {

            _maxNumberElements = maxNumberElements;
            _cacheContent = new LinkedList<CacheElement>();
            _index = new Dictionary<TKey, LinkedListNode<CacheElement>>(_maxNumberElements);
        }

        public bool IsInCache(TKey key) {

            return _index.ContainsKey(key);
        }

        public int Count => _index.Count;

        private void MakeNodeMostRecentlyUsed(LinkedListNode<CacheElement> node) {

            if (node == _cacheContent.Last) {
                return;
            }
            _cacheContent.Remove(node);
            _cacheContent.AddLast(node);
        }

        public bool TryGetFromCache(TKey key, [NotNullWhen(true)] out TValue? value) {

            var result = _index.TryGetValue(key, out var node);
            value = default;
            if (result && node != default) {
                MakeNodeMostRecentlyUsed(node);
                value = node.Value.value;
            }
            return result;
        }

        public void Add(TKey key, TValue value) {

            if (_index.TryGetValue(key, out var existingNode)) {
                existingNode.Value.value = value;
                return;
            }
            if (_index.Count >= _maxNumberElements) {
                RemoveLeastUsedElement();
            }
            var cacheElement = new CacheElement(key, value);
            var node = _cacheContent.AddLast(cacheElement);
            _index.Add(key, node);
        }

        private void RemoveLeastUsedElement() {

            var node = _cacheContent.First;
            var key = node.Value.key;
            var value = node.Value.value;
            itemWillBeRemovedFromCacheEvent?.Invoke(key, value);
            _cacheContent.RemoveFirst();
            _index.Remove(key);
        }

        public void Clear() {

            while (_index.Count > 0) {
                RemoveLeastUsedElement();
            }
        }
    }
}
