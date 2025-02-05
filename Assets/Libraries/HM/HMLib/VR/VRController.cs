using System;
using UnityEngine;
using UnityEngine.XR;


public class VRController : MonoBehaviour {

    [SerializeField] XRNode _node = default;
    [SerializeField] int _nodeIdx = default;
    [SerializeField] Transform _viewAnchorTransform;
    [SerializeField] VRControllerTransformOffset _transformOffset = default;

    private IVRPlatformHelper _vrPlatformHelper = default;

    public XRNode node { get => _node; set => _node = value; }
    public int nodeIdx { get => _nodeIdx; set => _nodeIdx = value; }
    public Vector3 position => transform.position;
    public Quaternion rotation => transform.rotation;
    public Vector3 forward => transform.forward;
    public float triggerValue =>
        _mouseMode ? Input.GetMouseButton(0) ? 1.0f : 0.0f : _vrPlatformHelper.GetTriggerValue(_node);
    public Vector2 thumbstick => _vrPlatformHelper.GetThumbstickValue(_node);
    public bool active => gameObject.activeInHierarchy;
    public Transform viewAnchorTransform => _viewAnchorTransform;
    public event Action<VRController, Pose> anchorUpdateEvent;
    public bool mouseMode {
        get => _mouseMode;
        set {
            if (_mouseMode == value) {
                return;
            }
            _mouseMode = value;
            if (_mouseMode) {
                _viewAnchorTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            else {
                UpdateAnchorOffsetPose();
            }
        }
    }

    private Vector3 _lastTrackedPosition = Vector3.zero;
    private Quaternion _lastTrackedRotation = Quaternion.identity;
    private bool _mouseMode;

    
    private static readonly Vector3 kLeftControllerDefaultPosition = new Vector3(-0.2f, 0.05f, 0.0f);
    
    private static readonly Vector3 kRightControllerDefaultPosition = new Vector3(0.2f, 0.05f, 0.0f);

    
    public void Init(IVRPlatformHelper vrPlatformHelper) {

        _vrPlatformHelper = vrPlatformHelper;
        _mouseMode = false;
        SetupVRPlatformHelper();
    }

    protected void OnEnable() {

        SetupVRPlatformHelper();
        // Make sure the controllers are *always* positioned correctly, even when enabled after VRController.Update would get processed
        if (_vrPlatformHelper != null) {
            Update();
        }
    }

    protected void OnDisable() {

        if (_vrPlatformHelper == null) {
            return;
        }
        _vrPlatformHelper.controllersDidChangeReferenceEvent -= UpdateAnchorOffsetPose;
    }

    private void SetupVRPlatformHelper() {

        if (_vrPlatformHelper == null || !isActiveAndEnabled) {
            return;
        }
        _vrPlatformHelper.controllersDidChangeReferenceEvent -= UpdateAnchorOffsetPose;
        _vrPlatformHelper.controllersDidChangeReferenceEvent += UpdateAnchorOffsetPose;
        UpdateAnchorOffsetPose();
    }

    public bool TryGetControllerOffset(out Pose poseOffset) {

        return TryGetControllerOffset(_vrPlatformHelper, _transformOffset, _node, out poseOffset);
    }

    private static bool TryGetControllerOffset(
        IVRPlatformHelper vrPlatformHelper,
        VRControllerTransformOffset transformOffset,
        in XRNode node,
        out Pose poseOffset
    ) {

        bool result;
        var customPositionOffset = Vector3.zero;
        var customRotationOffset = Vector3.zero;
        if (transformOffset != null) {
            if (node == XRNode.LeftHand) {
                customPositionOffset = transformOffset.leftPositionOffset;
                customRotationOffset = transformOffset.leftRotationOffset;
            }
            else {
                customPositionOffset = transformOffset.rightPositionOffset;
                customRotationOffset = transformOffset.rightRotationOffset;
            }
        }
        if (transformOffset.alternativeHandling) {
            Pose legacyRoot = vrPlatformHelper.GetRootPositionOffsetForLegacyNodePose(node);
            if (node == XRNode.LeftHand) {
                legacyRoot = InvertControllerPose(legacyRoot);
            }
            result = vrPlatformHelper.TryGetLegacyPoseOffsetForNode(node, out Vector3 defaultPositionOffset, out Vector3 defaultRotationOffset);
            Vector3 positionOffset = defaultPositionOffset + customPositionOffset;
            Vector3 rotationOffset = defaultRotationOffset + customRotationOffset;
            if (node == XRNode.LeftHand) {
                positionOffset = positionOffset.MirrorOnYZPlane();
                rotationOffset = rotationOffset.MirrorEulerAnglesOnYZPlane();
            }
            poseOffset = AdjustPose(legacyRoot, new Pose(Vector3.zero, Quaternion.Euler(rotationOffset)));
            poseOffset = AdjustPose(poseOffset, new Pose(positionOffset, Quaternion.identity));
        }
        else {
            result = vrPlatformHelper.TryGetPoseOffsetForNode(node, out poseOffset);
            UpdatePoseOffset(node, customPositionOffset, customRotationOffset, ref poseOffset);
        }
        return result;
    }

    private static void UpdatePoseOffset(in XRNode node, in Vector3 customPositionOffset, in Vector3 customRotationOffset, ref Pose poseOffset) {

        poseOffset = AdjustPose(poseOffset, new Pose(customPositionOffset, Quaternion.Euler(customRotationOffset)));
        if (node == XRNode.LeftHand) {
            poseOffset = InvertControllerPose(poseOffset);
        }
    }

    public void UpdateAnchorOffsetPose(Pose poseOffset) {

        if (_viewAnchorTransform.position == poseOffset.position &&
            _viewAnchorTransform.rotation == poseOffset.rotation) {
            return;
        }
        _viewAnchorTransform.SetLocalPositionAndRotation(poseOffset.position, poseOffset.rotation);
        anchorUpdateEvent?.Invoke(this, poseOffset);
    }

    public static Pose AdjustPose(Pose originalPose, Pose adjustment) {

        return adjustment.GetTransformedBy(originalPose);
    }

    public static Pose InvertControllerPose(Pose finalPose) {

        return new Pose(
            finalPose.position.MirrorOnYZPlane(),
            new Quaternion(finalPose.rotation.x, -finalPose.rotation.y, -finalPose.rotation.z, finalPose.rotation.w)
        );
    }

    private void UpdateAnchorOffsetPose() {

        if (TryGetControllerOffset(out var offset)) {
            UpdateAnchorOffsetPose(offset);
        }
    }

    protected void Update() {

        bool poseValid = _vrPlatformHelper.GetNodePose(_node, _nodeIdx, out var pos, out var rot);
        if (poseValid) {
            _lastTrackedPosition = pos;
            _lastTrackedRotation = rot;
        }
        else {
            pos = _lastTrackedPosition != Vector3.zero ? _lastTrackedPosition :
                _node == XRNode.LeftHand ? kLeftControllerDefaultPosition : kRightControllerDefaultPosition;
            rot = _lastTrackedRotation != Quaternion.identity ? _lastTrackedRotation : Quaternion.identity;
        }
        transform.SetLocalPositionAndRotation(pos, rot);
    }
}
