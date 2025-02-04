using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimpleTextWithIconTableCell : HMUI.TableCell {

    [SerializeField] TextMeshProUGUI _text = default;
    [SerializeField] Image _icon = default;

    public Image icon { set => _icon = value; get => _icon; }
    public string text { set => _text.text = value; get => _text.text; }

}
