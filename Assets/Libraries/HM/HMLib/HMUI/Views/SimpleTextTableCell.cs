using UnityEngine;
using TMPro;

public class SimpleTextTableCell : HMUI.TableCell {
    
    [SerializeField] TextMeshProUGUI _text = default;

    public string text {
        set => _text.text = value;
        get => _text.text;
    }
}
