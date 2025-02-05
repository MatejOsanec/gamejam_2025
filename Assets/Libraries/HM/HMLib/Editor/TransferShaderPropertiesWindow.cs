using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class TransferShaderPropertiesWindow : EditorWindow {

    private Object _shader = (Shader)default;
    private List<Material> _materials = new List<Material>();
    private List<Shader> _ourShaders = new List<Shader>();
    private List<MaterialConsistencyChecker.KeywordInconsistency> _keywordInconsistenciesList = new List<MaterialConsistencyChecker.KeywordInconsistency>();
    private MaterialConsistencyChecker _materialChecker = new MaterialConsistencyChecker();
    private Change[] _changes = new Change[0];
    private Filter[] _filters = new Filter[0];
    private bool _showChanges = true;
    private bool _showMaterials = true;
    private bool _showNotes = false;
    private bool _showFilters = true;
    private Shader _cachedShader = default;
    private Vector2 _scrollPos;
    private Object _newShader = (Shader)default;
    private bool _createBackup = false;
    private bool _allShaders = false;
    private bool _forceReserialize = false;
    private bool _cachedAllShaders = false;
    private string _path = "Assets/";
    private bool _showUnusedShaders = false;
    private string _cachedPath = "Assets/";
    private static readonly int _stencilRefValuePropertyID = Shader.PropertyToID("_StencilRefValue");
    private static readonly int _stencilCompPropertyID = Shader.PropertyToID("_StencilComp");
    private static readonly int _stencilPassPropertyID = Shader.PropertyToID("_StencilPass");

    private struct Change {
        public ChangeType _type;
        public ValueType _valueType;
        public SpecialType _specialType;
        public string _oldName;
        public string _newName;
        public string _newToggleName;
        public int _newToggleValue;
        public bool _invert;
        public bool boolValue;
        public float floatValue;
        public int intValue;
        //public Texture texValue;
        public Vector4 vectorValue;
        public Color colorValue;
        public float _multiplier;
    }

    private struct Filter {
        public FilterType _filterType;
        public string _name;
        public bool _boolValue;
        public int _intValue;
        public Texture _texValue;
        public float _floatValue;
        public ComparisonType _comparisonType;
    }

    private enum ComparisonType {
        Less,
        LessOrEqual,
        Equal,
        NotEqual,
        Greater,
        GreaterOrEqual
    }

    private enum FilterType {
        Keyword,
        Int,
        Float,
        Texture,
        ColorAlpha,
        RenderQueue,
        StencilValue,
        CustomStencil,
        KeywordInconsistency
    }

    private enum ChangeType {
        Transfer,
        Set,
        FixKeywordsUsingToggles,
        FixTogglesUsingKeywords,
        Special
    }

    private enum ValueType {
        Keyword,
        KeywordEnum,
        Float,
        Int,
        Vector,
        Color,
        Texture
    }

    private enum SpecialType {
        WhiteboostEnum,
        FogEnum,
        RimlightAdditive,
        RimlightLerp
    }

    [MenuItem("Tools/Transfer Shader Properties")]
    private static void ShowWindow() {
        var window = GetWindow<TransferShaderPropertiesWindow>();
        window.titleContent = new GUIContent("ShaderTransfer");
        window.Show();
    }

    private void OnGUI() {

        if (_shader != null && _cachedShader != _shader) {
            _cachedShader = (Shader)_shader;
            FindMaterials();
        }

        if (_cachedAllShaders != _allShaders) {
            _cachedAllShaders = _allShaders;
            FindMaterials();
        }

        if (_cachedPath != _path) {
            _cachedPath = _path;
            FindMaterials();
        }

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);
        GUI.skin.button.wordWrap = true;

        _allShaders = EditorGUILayout.Toggle("All Shaders", _allShaders);

        if (!_allShaders) {
            EditorStyles.label.fontStyle = FontStyle.Bold;
            _shader = (Shader)EditorGUILayout.ObjectField("Shader:", _shader, typeof(Shader), false);
            EditorStyles.label.fontStyle = FontStyle.Normal;
        }

        _path = EditorGUILayout.TextField("Path", _path);
        _showUnusedShaders = EditorGUILayout.Toggle("Show Unused Shaders", _showUnusedShaders);

        _showNotes = EditorGUILayout.BeginFoldoutHeaderGroup(_showNotes, "⚠ Reminders");
        if (_showNotes) {
            EditorGUI.indentLevel++;
            EditorGUILayout.SelectableLabel("All new properties must already exist.\nKeywords must have their shader feature specified");
            EditorGUILayout.SelectableLabel("To transfer Enum keywords, edit TransferShaderPropertiesWindow.cs");

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(8);
        DisplayFilters();

        EditorGUILayout.Space(16);

        DisplayMaterials();
        EditorGUILayout.Space(8);
        DisplayChanges();

        EditorGUILayout.EndScrollView();
    }

    private void DisplayMaterials() {

        if (_materials.Count > 0) {
            EditorStyles.label.fontStyle = FontStyle.Bold;
            _showMaterials = EditorGUILayout.BeginFoldoutHeaderGroup(_showMaterials, $"Found Materials ({_materials.Count}):");
            EditorStyles.label.fontStyle = FontStyle.Normal;
            EditorGUI.indentLevel++;

            if (_showMaterials) {
                foreach (var shader in _ourShaders) {
                    var currentMaterials = _materials.Where(m => m.shader == shader).ToArray();

                    if (!_showUnusedShaders && currentMaterials.Length == 0) {
                        continue;
                    }

                    EditorStyles.label.fontStyle = FontStyle.Bold;
                    //EditorGUILayout.SelectableLabel($"{shader.name} ({currentMaterials.Length})");
                    EditorGUILayout.ObjectField($"({currentMaterials.Length} materials)", shader, typeof(Shader), false, GUILayout.Width(350));
                    EditorStyles.label.fontStyle = FontStyle.Normal;
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Space(5);

                    foreach (var mat in currentMaterials) {
                        EditorGUILayout.ObjectField(mat, typeof(Material), false);
                        DisplayInconsistencies(mat);
                    }

                    EditorGUILayout.Space(10);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndFoldoutHeaderGroup();

        }
        else {
            EditorGUI.indentLevel++;
            EditorGUILayout.SelectableLabel("No material meets the conditions");
            EditorGUI.indentLevel--;
        }
    }

    private void AddChange() {
        AddToArray(ref _changes, new Change() { _type = ChangeType.Set, _valueType = ValueType.Keyword, _newName = "", boolValue = true, _multiplier = 1.0f });
    }

    private void AddFilter() {
        AddToArray(ref _filters, new Filter() { _filterType = FilterType.Keyword, _name = "", _boolValue = true, _comparisonType = ComparisonType.Equal, _floatValue = 1.0f });
    }

    private void DisplayChanges() {

        EditorStyles.label.fontStyle = FontStyle.Bold;
        _showChanges = EditorGUILayout.BeginFoldoutHeaderGroup(_showChanges, $"Changes ({_changes.Length}):");
        EditorStyles.label.fontStyle = FontStyle.Normal;

        EditorGUI.indentLevel++;

        if (_showChanges) {

            for (int i = 0; i < _changes.Length; i++) {

                EditorGUILayout.Space(3);

                ChangeType type = (ChangeType)EditorGUILayout.EnumPopup("Type:", _changes[i]._type);
                _changes[i]._type = type;

                if (type == ChangeType.Special) {
                    _changes[i]._specialType = (SpecialType)EditorGUILayout.EnumPopup("Action:", _changes[i]._specialType);
                }
                else if (type == ChangeType.FixKeywordsUsingToggles || type == ChangeType.FixTogglesUsingKeywords) {
                    // No more fields to fill
                }
                else {

                    ValueType valueType = (ValueType)EditorGUILayout.EnumPopup("Value type:", _changes[i]._valueType);
                    _changes[i]._valueType = valueType;

                    if (type == ChangeType.Transfer) {
                        _changes[i]._oldName = EditorGUILayout.TextField("Old Name", _changes[i]._oldName);
                        _changes[i]._newName = EditorGUILayout.TextField("New Name", _changes[i]._newName);

                        if (valueType == ValueType.Keyword) {
                            _changes[i]._invert = EditorGUILayout.Toggle("Invert", _changes[i]._invert);
                            _changes[i]._newToggleName = EditorGUILayout.TextField("New Toggle Name", _changes[i]._newToggleName);
                        }

                        if (valueType == ValueType.Float) {
                            _changes[i]._multiplier = EditorGUILayout.FloatField("Multiplier", _changes[i]._multiplier);
                        }
                    }

                    if (type == ChangeType.Set) {
                        string propertyOrName = valueType == ValueType.Keyword || valueType == ValueType.KeywordEnum ? "Keyword" : "Property";
                        _changes[i]._newName = EditorGUILayout.TextField(propertyOrName, _changes[i]._newName);

                        switch (valueType) {

                            case ValueType.Keyword:
                                _changes[i].boolValue = EditorGUILayout.Toggle("Value", _changes[i].boolValue);
                                _changes[i]._newToggleName = EditorGUILayout.TextField("Toggle Name", _changes[i]._newToggleName);
                                break;

                            case ValueType.KeywordEnum:
                                _changes[i]._newToggleName = EditorGUILayout.TextField("Toggle Name", _changes[i]._newToggleName);
                                _changes[i]._newToggleValue = EditorGUILayout.IntField("Toggle Value", _changes[i]._newToggleValue);
                                break;

                            case ValueType.Float:
                                _changes[i].floatValue = EditorGUILayout.FloatField("Value", _changes[i].floatValue);
                                break;

                            case ValueType.Int:
                                _changes[i].intValue = EditorGUILayout.IntField("Value", _changes[i].intValue);
                                break;

                            case ValueType.Vector:
                                _changes[i].vectorValue = EditorGUILayout.Vector4Field("Value", _changes[i].vectorValue);
                                break;

                            case ValueType.Color:
                                _changes[i].colorValue = EditorGUILayout.ColorField("Value", _changes[i].colorValue);
                                break;

                            case ValueType.Texture:
                                EditorGUILayout.SelectableLabel("Setting textures isn't supported yet");
                                break;

                        }
                    }
                }

                if (GUILayout.Button("[x] Remove Change")) {
                    RemoveAt(ref _changes, i);
                }

                EditorGUILayout.Space(3);
                GUILine();
            }

            EditorGUILayout.Space(8);
            if (GUILayout.Button("+ Add change")) {
                AddChange();
            }

            _forceReserialize = EditorGUILayout.Toggle("Force reserialize", _forceReserialize);

            if (_forceReserialize) {
                EditorGUILayout.HelpBox(
                    @"When re-serializing, each material will be focused to force Unity to refresh the default properties.
Keep the Unity editor and  inspector window in focus and just wait it to finish.
It might take a while depending on the number of materials you are changing.", MessageType.Warning);
            }

            if (_materials != null && _materials.Count > 0 && (_changes.Length > 0 || _forceReserialize)) {
                EditorGUILayout.Space(16);
                if (GUILayout.Button("Apply Changes")) {
                    if (_forceReserialize) {
                        EditorCoroutineUtility.StartCoroutine(ApplyChangesWithForceReserializeCoroutine(), this);
                    }
                    else {
                        ApplyChanges();
                    }

                }
            }

            EditorGUILayout.Space(16);
            _newShader = (Shader)EditorGUILayout.ObjectField("New Shader:", _newShader, typeof(Shader), false);

            if (_shader != null && _materials != null && _materials.Count > 0 && _newShader != null) {

                if (GUILayout.Button("Replace Shader")) {
                    ReplaceShader();
                }

                _createBackup = EditorGUILayout.ToggleLeft("Create Backup Materials", _createBackup);
            }
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndFoldoutHeaderGroup();

    }

    private void DisplayFilters() {

        EditorStyles.label.fontStyle = FontStyle.Bold;
        _showFilters = EditorGUILayout.BeginFoldoutHeaderGroup(_showFilters, $"Filters ({_filters.Length}):");
        EditorStyles.label.fontStyle = FontStyle.Normal;

        EditorGUI.indentLevel++;

        if (_showFilters) {
            for (int i = 0; i < _filters.Length; i++) {

                EditorGUILayout.Space(3);

                FilterType type = (FilterType)EditorGUILayout.EnumPopup("Type:", _filters[i]._filterType);
                _filters[i]._filterType = type;

                switch (_filters[i]._filterType) {
                    case FilterType.Keyword:
                        _filters[i]._name = EditorGUILayout.TextField("Keyword", _filters[i]._name);
                        _filters[i]._boolValue = EditorGUILayout.Toggle("Value", _filters[i]._boolValue);
                        break;
                    case FilterType.Int:
                        _filters[i]._name = EditorGUILayout.TextField("Property", _filters[i]._name);
                        _filters[i]._intValue = EditorGUILayout.IntField("Value", _filters[i]._intValue);
                        _filters[i]._comparisonType = (ComparisonType)EditorGUILayout.EnumPopup("Comparison", _filters[i]._comparisonType);
                        break;
                    case FilterType.Texture:
                        _filters[i]._name = EditorGUILayout.TextField("Property", _filters[i]._name);
                        _filters[i]._texValue = (Texture2D)EditorGUILayout.ObjectField("value", _filters[i]._texValue, typeof(Texture), false);
                        break;
                    case FilterType.Float:
                        _filters[i]._name = EditorGUILayout.TextField("Property", _filters[i]._name);
                        _filters[i]._floatValue = EditorGUILayout.FloatField("Value", _filters[i]._floatValue);
                        _filters[i]._comparisonType = (ComparisonType)EditorGUILayout.EnumPopup("Comparison", _filters[i]._comparisonType);
                        break;
                    case FilterType.ColorAlpha:
                        _filters[i]._name = EditorGUILayout.TextField("Property", _filters[i]._name);
                        _filters[i]._floatValue = EditorGUILayout.FloatField("Value", _filters[i]._floatValue);
                        _filters[i]._comparisonType = (ComparisonType)EditorGUILayout.EnumPopup("Comparison", _filters[i]._comparisonType);
                        break;
                    case FilterType.StencilValue:
                        _filters[i]._intValue = EditorGUILayout.IntField("Value", _filters[i]._intValue);
                        _filters[i]._comparisonType = (ComparisonType)EditorGUILayout.EnumPopup("Comparison", _filters[i]._comparisonType);
                        break;
                    case FilterType.CustomStencil:
                        _filters[i]._boolValue = EditorGUILayout.Toggle("Is Customized", _filters[i]._boolValue);
                        break;
                    case FilterType.RenderQueue:
                        _filters[i]._intValue = EditorGUILayout.IntField("Value", _filters[i]._intValue);
                        break;
                }

                if (GUILayout.Button("[x] Remove Filter")) {
                    RemoveAt(ref _filters, i);
                }

                EditorGUILayout.Space(3);
                GUILine();
            }

            EditorGUILayout.Space(8);
            if (GUILayout.Button("+ Add filter")) {
                AddFilter();
            }

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Apply filters")) {
                FindMaterials();
            }
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndFoldoutHeaderGroup();

    }

    private void RemoveAt<T>(ref T[] array, int index) {
        for (int i = index; i < array.Length - 1; i++) {
            array[i] = array[i + 1];
        }
        Array.Resize(ref array, array.Length - 1);
    }

    private void AddToArray<T>(ref T[] array, T element) {
        Array.Resize(ref array, array.Length + 1);
        array[array.Length - 1] = element;
    }

    void GUILine(int i_height = 1) {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }

    private void FindMaterials() {

        _materials.Clear();
        _ourShaders.Clear();
        _keywordInconsistenciesList.Clear();

        var allMaterials = AssetDatabase.FindAssets("t: Material");

        if (_allShaders) {
            var allShaders = AssetDatabase.FindAssets("t: Shader");

            foreach (var guid in allShaders) {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (path.StartsWith("Assets/Visuals/Shaders")) {
                    var shader = (Shader)AssetDatabase.LoadAssetAtPath(path, typeof(Shader));
                    _ourShaders.Add(shader);
                }
            }
        }
        else {
            _ourShaders.Add((Shader)_shader);
        }

        foreach (var guid in allMaterials) {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.StartsWith(_path)) {
                continue;
            }

            var material = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));

            if (material.shader == _shader || _allShaders && _ourShaders.Contains(material.shader)) {

                bool filteredOut = false;
                foreach (var filter in _filters) {

                    if (filter._filterType == FilterType.Keyword) {
                        if (material.IsKeywordEnabled(filter._name) != filter._boolValue) {
                            filteredOut = true;
                            break;
                        }
                    }

                    else if (filter._filterType == FilterType.Texture) {
                        if (material.GetTexture(filter._name) != filter._texValue) {
                            filteredOut = true;
                            break;
                        }
                    }

                    else if (filter._filterType == FilterType.KeywordInconsistency) {
                        if (_materialChecker.Check(material, ref _keywordInconsistenciesList)) {
                            filteredOut = true;
                            break;
                        }
                    }

                    else if (filter._filterType == FilterType.Int) {

                        if (filter._comparisonType == ComparisonType.Equal && material.GetInt(filter._name) != filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.NotEqual && material.GetInt(filter._name) == filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.Greater && material.GetInt(filter._name) <= filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.Less && material.GetInt(filter._name) >= filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.GreaterOrEqual && material.GetInt(filter._name) < filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.LessOrEqual && material.GetInt(filter._name) > filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                    }

                    else if (filter._filterType == FilterType.Float) {

                        if (filter._comparisonType == ComparisonType.Equal && material.GetFloat(filter._name) != filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.NotEqual && material.GetFloat(filter._name) == filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.Greater && material.GetFloat(filter._name) <= filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.Less && material.GetFloat(filter._name) >= filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.GreaterOrEqual && material.GetFloat(filter._name) < filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.LessOrEqual && material.GetFloat(filter._name) > filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                    }

                    else if (filter._filterType == FilterType.ColorAlpha) {

                        if (filter._comparisonType == ComparisonType.Equal && material.GetColor(filter._name).a != filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.NotEqual && material.GetColor(filter._name).a == filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.Greater && material.GetColor(filter._name).a <= filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.Less && material.GetColor(filter._name).a >= filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.GreaterOrEqual && material.GetColor(filter._name).a < filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.LessOrEqual && material.GetColor(filter._name).a > filter._floatValue) {
                            filteredOut = true;
                            break;
                        }
                    }

                    else if (filter._filterType == FilterType.StencilValue) {

                        if (filter._comparisonType == ComparisonType.Equal && material.GetInt(_stencilRefValuePropertyID) != filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.NotEqual && material.GetInt(_stencilRefValuePropertyID) == filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.Greater && material.GetInt(_stencilRefValuePropertyID) <= filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.Less && material.GetInt(_stencilRefValuePropertyID) >= filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.GreaterOrEqual && material.GetInt(_stencilRefValuePropertyID) < filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.LessOrEqual && material.GetInt(_stencilRefValuePropertyID) > filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                    }

                    else if (filter._filterType == FilterType.RenderQueue) {

                        if (filter._comparisonType == ComparisonType.Equal && material.renderQueue != filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.NotEqual && material.renderQueue == filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.Greater && material.renderQueue <= filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.Less && material.renderQueue >= filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.GreaterOrEqual && material.renderQueue < filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                        if (filter._comparisonType == ComparisonType.LessOrEqual && material.renderQueue > filter._intValue) {
                            filteredOut = true;
                            break;
                        }
                    }

                    else if (filter._filterType == FilterType.CustomStencil) {
                        bool isCustom = material.GetInt(_stencilRefValuePropertyID) != 0 || material.GetInt(_stencilCompPropertyID) != 8 || material.GetInt(_stencilPassPropertyID) != 0;
                        if (isCustom != filter._boolValue) {
                            filteredOut = true;
                            break;
                        }
                    }

                }

                if (!filteredOut) {
                    _materials.Add(material);
                }
            }
        }
    }

    private void ApplyChanges() {

        foreach (var material in _materials) {
            if (material is null) {
                continue;
            }
            if (_changes.Length > 0) {
                ApplyAllChangesToMaterial(material);
            }
        }
    }


    private void ApplyAllChangesToMaterial(Material material) {

        foreach (var change in _changes) {
            switch (change._type) {

                case ChangeType.Special:
                    SpecialSwitch(change._specialType, material);
                    break;

                case ChangeType.FixKeywordsUsingToggles:
                    FixKeywordsUsingToggles(material);
                    break;

                case ChangeType.FixTogglesUsingKeywords:
                    FixTogglesUsingKeywords(material);
                    break;

                default:
                    ChangeProperty(material, change);
                    break;
            }
        }
    }

    private IEnumerator ApplyChangesWithForceReserializeCoroutine() {

        var materialNames = new List<string>(_materials.Count);
        EditorApplication.ExecuteMenuItem("Window/General/Inspector");
        foreach (var material in _materials) {
            if (material is null) {
                continue;
            }
            // Selecting the material in the inspector makes it refresh the keywords and the serialization
            Selection.activeObject = material;
            EditorGUIUtility.PingObject(material);
            yield return null;
            materialNames.Add(AssetDatabase.GetAssetPath(material));
            ApplyAllChangesToMaterial(material);
            yield return null;
            EditorUtility.SetDirty(material);
        }
        yield return null;
        AssetDatabase.SaveAssets();
        yield return null;
        AssetDatabase.ForceReserializeAssets(materialNames);
        yield return null;
        AssetDatabase.SaveAssets();
    }

    private void SpecialSwitch(SpecialType specialType, Material material) {
        // switch (specialType) {
        //     case SpecialType.WhiteboostEnum:
        //         WhiteboostChange(material);
        //         break;
        //
        //     case SpecialType.FogEnum:
        //         FogChange(material);
        //         break;
        //
        //     case SpecialType.RimlightAdditive:
        //         RimlightAdditive(material);
        //         break;
        //
        //     case SpecialType.RimlightLerp:
        //         RimlightLerp(material);
        //         break;
        // }
    }

    private void ReplaceShader() {

        foreach (var material in _materials) {
            if (material is null)
                continue;

            if (_newShader != null) {
                var queue = material.renderQueue;

                if (_createBackup) {
                    string path = AssetDatabase.GetAssetPath(material);
                    string newPath = path.Remove(path.Length - 4) + "_Backup.mat";
                    AssetDatabase.CopyAsset(path, newPath);
                }

                material.shader = (Shader)_newShader;
                material.renderQueue = queue;
            }
        }
    }

    private void ChangeProperty(Material material, Change change) {

        switch (change._valueType) {
            case ValueType.Float:
                var floatValue = change._type == ChangeType.Transfer ? (material.GetFloat(change._oldName) * change._multiplier) : change.floatValue;
                material.SetFloat(change._newName, floatValue);
                break;

            case ValueType.Texture:
                if (change._type == ChangeType.Transfer) {
                    var texValue = material.GetTexture(change._oldName);
                    material.SetTexture(change._newName, texValue);
                }
                break;

            case ValueType.Vector:
                var vectorValue = change._type == ChangeType.Transfer ? material.GetVector(change._oldName) : change.vectorValue;
                material.SetVector(change._newName, vectorValue);
                break;

            case ValueType.Int:
                var intValue = change._type == ChangeType.Transfer ? material.GetInt(change._oldName) : change.intValue;
                material.SetInt(change._newName, intValue);
                break;

            case ValueType.Color:
                var colorValue = change._type == ChangeType.Transfer ? material.GetColor(change._oldName) : change.colorValue;
                material.SetColor(change._newName, colorValue);
                break;

            case ValueType.Keyword:
                var keyValue = change._type == ChangeType.Transfer ? material.IsKeywordEnabled(change._oldName) : change.boolValue;
                if (change._type == ChangeType.Transfer && change._invert) keyValue = !keyValue;

                if (keyValue) {
                    material.EnableKeyword(change._newName);
                    material.SetInt(change._newToggleName, 1);
                }
                else {
                    material.DisableKeyword(change._newName);
                    material.SetInt(change._newToggleName, 0);
                }
                break;

            case ValueType.KeywordEnum:
                material.EnableKeyword(change._newName);
                material.SetInt(change._newToggleName, change._newToggleValue);
                break;

        }
    }

    private void DisplayInconsistencies(Material material) {

        if (_keywordInconsistenciesList.Count <= 0) {
            return;
        }

        string text = "";
        foreach (var inconsistency in _keywordInconsistenciesList) {
            if (inconsistency.material == material) {
                text += inconsistency.text;
            }
        }

        EditorGUILayout.HelpBox(text, MessageType.Warning, true);
        EditorGUILayout.Space(5);
    }

    private void FixKeywordsUsingToggles(Material material) {
        foreach (var inconsistency in _keywordInconsistenciesList) {
            if (inconsistency.material == material) {
                FixInconsistencyUsingToggle(inconsistency);
            }
        }
    }

    private void FixTogglesUsingKeywords(Material material) {
        foreach (var inconsistency in _keywordInconsistenciesList) {
            if (inconsistency.material == material) {
                FixInconsistencyUsingKeyword(inconsistency);
            }
        }
    }

    private void FixInconsistencyUsingToggle(MaterialConsistencyChecker.KeywordInconsistency inconsistency) {

        if (inconsistency.keywordType == MaterialConsistencyChecker.KeywordType.Toggle) {
            if (inconsistency.currentlyOn) {
                inconsistency.material.DisableKeyword(inconsistency.currentKeyword);
            }
            else {
                inconsistency.material.EnableKeyword(inconsistency.currentKeyword);
            }
        }
        else {
            inconsistency.material.EnableKeyword(inconsistency.enumKeyword);
            inconsistency.material.DisableKeyword(inconsistency.currentKeyword);
        }
    }

    private void FixInconsistencyUsingKeyword(MaterialConsistencyChecker.KeywordInconsistency inconsistency) {

        if (inconsistency.keywordType == MaterialConsistencyChecker.KeywordType.Toggle) {
            inconsistency.material.SetFloat(inconsistency.propertyNameId, inconsistency.currentlyOn ? 1.0f : 0.0f);
        }
        else {
            if (inconsistency.currentKeywordEnumId > 0) {
                inconsistency.material.SetFloat(inconsistency.propertyNameId, inconsistency.currentKeywordEnumId);
            }
        }
    }

    // SPECIFIC CHANGES

    // private void WhiteboostChange(Material material) {
    //
    //     if (material.IsKeywordEnabled("ENABLE_FORCED_WHITE_BOOST"))
    //     {
    //         material.EnableKeyword("_WHITEBOOSTTYPE_ALWAYS");
    //         material.DisableKeyword("_WHITEBOOSTTYPE_MAINEFFECT");
    //         material.DisableKeyword("_WHITEBOOSTTYPE_NONE");
    //
    //         material.SetFloat("_WhiteBoostType", 2);
    //
    //     } else if (material.IsKeywordEnabled("ENABLE_MAIN_EFFECT_WHITE_BOOST")) {
    //
    //         material.EnableKeyword("_WHITEBOOSTTYPE_MAINEFFECT");
    //         material.DisableKeyword("_WHITEBOOSTTYPE_ALWAYS");
    //         material.DisableKeyword("_WHITEBOOSTTYPE_NONE");
    //
    //         material.SetFloat("_WhiteBoostType", 1);
    //
    //     } else {
    //
    //         material.EnableKeyword("_WHITEBOOSTTYPE_NONE");
    //         material.DisableKeyword("_WHITEBOOSTTYPE_MAINEFFECT");
    //         material.DisableKeyword("_WHITEBOOSTTYPE_ALWAYS");
    //
    //         material.SetFloat("_WhiteBoostType", 0);
    //     }
    // }
}
