using UnityEngine;

public class GroundShadow : MonoBehaviour  {

    protected void OnEnable() {

        Setup();
    }

    protected void OnValidate() {

        Setup();
    }

    private void Setup() {

        var position = transform.position;
        position.y = 0.01f;
        transform.SetPositionAndRotation(position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
    }
}
