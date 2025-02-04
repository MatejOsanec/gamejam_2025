using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ToggleableCanvasGroup : MonoBehaviour {

    [SerializeField] CanvasGroup _canvasGroup = default;
    [SerializeField] Toggle _toggle = default;

    [Space]
    [SerializeField] bool _invertToggle = default;
    
    protected void OnEnable() {

        _toggle.onValueChanged.AddListener(HandleToggleValueChanged);

        SetCanvasGroupData(_toggle.isOn);
    }

    protected void OnDisable() {

        _toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);
    }

    private void HandleToggleValueChanged(bool isOn) => SetCanvasGroupData(isOn);

    private void SetCanvasGroupData(bool isOn) {

        var shouldActivateCanvasGroup = isOn ^ _invertToggle;

        _canvasGroup.interactable = shouldActivateCanvasGroup;
    }

#if UNITY_EDITOR
    private void OnValidate() {

        _canvasGroup = GetComponent<CanvasGroup>();
    }
#endif
}
