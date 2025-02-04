using UnityEngine;
using UnityEditor;

internal class MaterialSpaceShowIfAnyDecorator : MaterialPropertyDrawer {

    private readonly float _height;
    private readonly string[] _conditions;
    private bool _show;
    private readonly int _requiredCount = 0;


    public MaterialSpaceShowIfAnyDecorator(float height, params string[] conditions) {

        _height = height;
        _conditions = conditions;
    }

    public MaterialSpaceShowIfAnyDecorator(float height, float requiredCount, params string[] conditions) {

        _height = height;
        _conditions = conditions;
        _requiredCount = (int) requiredCount;
    }

    public override float GetPropertyHeight(MaterialProperty prop,  string label,  MaterialEditor editor)  {

        _show = MaterialDrawerConditionsTester.TestConditions(editor.targets, _conditions, _requiredCount);
        return _show ? _height : 0.0f;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) { }

}
