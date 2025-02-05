using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HMUI {

    public class UIItemsList<T> : MonoBehaviour where T : UnityEngine.MonoBehaviour {

        [SerializeField] T _prefab = default;
        [SerializeField] Transform _itemsContainer = default;
        [SerializeField] bool _insertInTheBeginning = default;
        [SerializeField] List<T> _items = new List<T>();

       MonoBehaviour _container = default;

        public delegate void DataCallback(int idx, T item);

        public IEnumerable<T> items => _items;


        public void SetData(int numberOfElements, DataCallback dataCallback) {


        }
    }
}
