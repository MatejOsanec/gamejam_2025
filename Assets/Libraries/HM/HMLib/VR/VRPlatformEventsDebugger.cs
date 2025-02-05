using System;
using UnityEngine;


public class VRPlatformEventsDebugger : IInitializable, IDisposable, IVerboseLogger  {

   IVRPlatformHelper _vrPlatformHelper;

    public string loggerPrefix => "VRPlatformEventsDebugger";

    public void Initialize() {
        
        _vrPlatformHelper.vrFocusWasReleasedEvent += HandleVRFocusWasReleased;
        _vrPlatformHelper.vrFocusWasCapturedEvent += HandleVRFocusWasCaptured;
        _vrPlatformHelper.hmdMountedEvent += HandleHMDMounted;
        _vrPlatformHelper.hmdUnmountedEvent += HandleHMDUnmounted;
        _vrPlatformHelper.inputFocusWasReleasedEvent += HandleInputFocusWasReleased;
        _vrPlatformHelper.inputFocusWasCapturedEvent += HandleInputFocusWasCaptured;
    }
    
    public void Dispose() {
        
        _vrPlatformHelper.vrFocusWasReleasedEvent -= HandleVRFocusWasReleased;
        _vrPlatformHelper.vrFocusWasCapturedEvent -= HandleVRFocusWasCaptured;
        _vrPlatformHelper.hmdMountedEvent -= HandleHMDMounted;
        _vrPlatformHelper.hmdUnmountedEvent -= HandleHMDUnmounted;
        _vrPlatformHelper.inputFocusWasReleasedEvent -= HandleInputFocusWasReleased;
        _vrPlatformHelper.inputFocusWasCapturedEvent -= HandleInputFocusWasCaptured;
    }

    private void HandleInputFocusWasCaptured() {
        
        this.Log("Input Focus was captured");
    }

    private void HandleInputFocusWasReleased() {
        
        this.Log("Input Focus was released");
    }

    private void HandleHMDUnmounted() {
        
        this.Log("HMD was unmounted");
    }

    private void HandleHMDMounted() {
        
        this.Log("HMD was mounted");
    }

    private void HandleVRFocusWasCaptured() {
        
        this.Log("VR Focus was captured.");
    }

    private void HandleVRFocusWasReleased() {
        
        this.Log("VR Focus was released.");
    }
}
