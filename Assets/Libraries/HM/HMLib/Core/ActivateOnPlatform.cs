using UnityEngine;


public class ActivateOnPlatform : MonoBehaviour {

    [SerializeField] VRPlatformSDK _vrPlatformSdk = default;

   readonly IVRPlatformHelper _vrPlatformHelper = default;

    private void Awake() {

        gameObject.SetActive(_vrPlatformHelper.vrPlatformSDK == _vrPlatformSdk);
    }
}
