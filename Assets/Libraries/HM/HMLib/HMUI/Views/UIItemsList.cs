using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HMUI {

    public class UIItemsList<T> : MonoBehaviour where T : UnityEngine.MonoBehaviour {

        [SerializeField] T _prefab = default;
        [SerializeField] Transform _itemsContainer = default;
        [SerializeField] bool _insertInTheBeginning = default;
        [SerializeField] List<T> _items = new List<T>();

       DiContainer _container = default;

        public delegate void DataCallback(int idx, T item);

        public IEnumerable<T> items => _items;


        public void SetData(int numberOfElements, DataCallback dataCallback) {

            for (int i = 0; i < numberOfElements; i++) {

                T item = null;

                // We need new element.
                if (i > _items.Count - 1) {
                    var go = _container.InstantiatePrefab(_prefab);
                    go.SetActive(true);
                    item = go.GetComponent<T>();
                    item.transform.SetParent(_itemsContainer, worldPositionStays: false);
                    _items.Add(item);
                }
                else {
                    item = _items[i];
                    item.gameObject.SetActive(true);
                }

                if (_insertInTheBeginning) {
                    item.transform.SetAsFirstSibling();
                }
                dataCallback(i, item);
            }

            for (int i = numberOfElements; i < _items.Count; i++) {
                _items[i].gameObject.SetActive(false);
            }
        }
    }
}
