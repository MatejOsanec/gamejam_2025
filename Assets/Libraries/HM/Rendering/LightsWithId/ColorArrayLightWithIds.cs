using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ColorArrayLightWithIds : LightWithIds {

    [SerializeField] ColorArrayLightWithId[] _colorArrayLightWithIds;

    [Space]
    [SerializeField] MaterialController _materialController;
    [SerializeField] MaterialPropertyBlockController[] _materialPropertyBlockControllers;

    [Space]
    [SerializeField] string _colorsArrayPropertyName = "_ColorsArray";
    [SerializeField] string _colorsArrayOffsetPropertyName = "_ColorsArrayOffset";

    [Serializable]
    public class ColorArrayLightWithId : LightWithId {

        [SerializeField] int _index;

        public event Action<int, Color> didSetColorEvent;

        public ColorArrayLightWithId(int index, int lightId) : base(lightId) {

            this._index = index;
        }

        public override void ColorWasSet(Color newColor) {

            base.ColorWasSet(newColor);

            didSetColorEvent?.Invoke(_index, newColor);
        }
    }

    private int _colorsArrayPropertyId;
    private int _colorsArrayOffsetPropertyId;
    private Vector4[] _colorsArray;

    protected override void OnEnable() {

        base.OnEnable();

        RegisterArrayForColorChanges();
    }

    protected void OnDestroy() {

        UnregisterArrayFromColorChanges();
    }

    protected override void ProcessNewColorData() {

        SetColorDataToMaterial();
    }

    protected override IEnumerable<LightWithId> GetLightWithIds() => _colorArrayLightWithIds;

    private void HandleColorLightWithIdDidSetColor(int index, Color color) {

        color = color.linear; // color is processed differently in shader if accessed from a vector array rather than a color property
        _colorsArray[index] = new Vector4(color.r, color.g, color.b, color.a);
    }

    private void SetColorDataToMaterial() {

        _materialController.material.SetVectorArray(_colorsArrayPropertyId, _colorsArray);
    }

    private void SetColorArrayOffsetToMaterialPropertyBlocks() {

        Assert.AreEqual(_colorsArray.Length % _materialPropertyBlockControllers.Length, 0, "ColorsArray and MaterialPropertyBlockControllers are not divisible.");

        var offset = _colorArrayLightWithIds.Length / _materialPropertyBlockControllers.Length;

        for (var i = 0; i < _materialPropertyBlockControllers.Length; i++) {
            _materialPropertyBlockControllers[i].materialPropertyBlock.SetInt(_colorsArrayOffsetPropertyId, i * offset);
            _materialPropertyBlockControllers[i].ApplyChanges();
        }
    }

    private void RegisterArrayForColorChanges() {

        _colorsArrayPropertyId = Shader.PropertyToID(_colorsArrayPropertyName);
        _colorsArrayOffsetPropertyId = Shader.PropertyToID(_colorsArrayOffsetPropertyName);

        _colorsArray = new Vector4[_colorArrayLightWithIds.Length];
        for (var i = 0; i < _colorsArray.Length; i++) {
            _colorsArray[i] = Vector4.zero;

            _colorArrayLightWithIds[i].didSetColorEvent += HandleColorLightWithIdDidSetColor;
        }

        SetColorArrayOffsetToMaterialPropertyBlocks();
        SetColorDataToMaterial();
    }

    private void UnregisterArrayFromColorChanges() {

        foreach (var colorArrayLightWithId in _colorArrayLightWithIds) {
            colorArrayLightWithId.didSetColorEvent -= HandleColorLightWithIdDidSetColor;
        }
    }

#if UNITY_EDITOR
    public void SetColorArrayLightWithIdData(ColorArrayLightWithId[] colorArrayLightWithIds) {

        UnregisterArrayFromColorChanges();
        _colorArrayLightWithIds = colorArrayLightWithIds;
        RegisterArrayForColorChanges();

        EditorUtility.SetDirty(this);

        SetNewLightsWithIds(_colorArrayLightWithIds);
    }
#endif
}
