using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class StringListSO : PersistentScriptableObject {

    [SerializeField] string[] _strings = default!;

    public IReadOnlyList<string> strings => _strings;
}
