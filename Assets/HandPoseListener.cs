using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPoseListener : MonoBehaviour
{
    [SerializeField] GameContext gameContext;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material forcePullMaterial;
    [SerializeField] Material defaultMaterial;
    void Start()
    {
        gameContext.OnHandPoseChange += GameContextOnHandPoseChange;
    }

    private void OnDestroy()
    {
        gameContext.OnHandPoseChange -= GameContextOnHandPoseChange;
    }

    private void GameContextOnHandPoseChange(Hand hand, HandPose pose, HandPoseState state)
    {
        Debug.Log($"Listener: {Enum.GetName(typeof(Hand), hand)}({Enum.GetName(typeof(HandPose), pose)}): {Enum.GetName(typeof(HandPoseState), state)}.");
        meshRenderer.material = defaultMaterial;
        if (state == HandPoseState.ON) {
            if (pose == HandPose.FORCE_PULL)
            {
                meshRenderer.material = forcePullMaterial;
            }
        }
    }

    void Update()
    {
        
    }
}
