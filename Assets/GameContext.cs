using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Ugly one purpose class where everything is stored on runtime
 */
public class GameContext : MonoBehaviour
{
    public event Action<Hand, HandPose, HandPoseState> OnHandPoseChange;

    public void HandPoseChanged(Hand hand, HandPose pose, HandPoseState state)
    {
        OnHandPoseChange?.Invoke(hand, pose, state);
    }

}
