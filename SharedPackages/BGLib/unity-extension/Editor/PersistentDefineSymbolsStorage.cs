using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class PersistentDefineSymbolsStorage {

    private const string kStorageFileName = ".persistentSymbols.json";

    [DoesNotRequireDomainReloadInit]
    private static Dictionary<string, bool> _values = null;
    [DoesNotRequireDomainReloadInit]
    private static string _filePath = string.Empty;
    [DoesNotRequireDomainReloadInit]
    private static bool _isDirty = false;

    public static void Load() {

        _isDirty = false;
        _filePath = Path.Combine(Path.GetDirectoryName(Application.dataPath)!, kStorageFileName);
        _values = File.Exists(_filePath)
            ? JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(_filePath))
            : new ();
    }

    public static void Save() {

        if (!_isDirty) {
            return;
        }

        File.WriteAllText(_filePath, JsonConvert.SerializeObject(_values, Formatting.Indented));

        _isDirty = false;
    }

    public static bool Get(string symbol) {

        if (_values == null) {
            Load();
        }

        return _values.GetValueOrDefault(symbol);
    }

    public static void Set(string symbol, bool value) {

        if (value == _values.GetValueOrDefault(symbol)) {
            return;
        }

        _values[symbol] = value;
        _isDirty = true;
    }
}
