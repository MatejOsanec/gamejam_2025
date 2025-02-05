using UnityEngine;


public class ResizablePanel: MonoBehaviour {
    
    [SerializeField] RectTransform _rectTransform = default;
    
   protected void OnDestroy() {


    }

    public void Resize(Vector2 size, float duration) {
        

    }

    private void SetSize(Vector2 size) {
        
        _rectTransform.sizeDelta = size;
    }
}
