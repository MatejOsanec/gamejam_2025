using System;
using UnityEngine;

public enum HandPose
{
    THUMBS_UP,
    SHOOT,
    FORCE_PULL,
    FORCE_PUSH,
}

public enum Hand
{
    LEFT,
    RIGHT
}

public enum HandPoseState
{
    ON,
    OFF
}

public class HandPoseController : MonoBehaviour
{
    [SerializeField] GameContext gameContext;
    [SerializeField] HandPose handPose;
    [SerializeField] Hand hand;
    HandPoseState state = HandPoseState.OFF;

    public void OnPoseSelected()
    {
        state = HandPoseState.ON;
        NotifyPoseChanged();
    }

    public void OnPoseUnselected()
    {
        state = HandPoseState.OFF;
        NotifyPoseChanged();
    }

    private void NotifyPoseChanged()
    {
        Debug.Log($"{Enum.GetName(typeof(Hand), hand)}({Enum.GetName(typeof(HandPose), handPose)}): {Enum.GetName(typeof(HandPoseState), state)}.");
        gameContext.HandPoseChanged(hand, handPose, state);
    }
}
