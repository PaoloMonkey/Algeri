using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Controllers;
using System;
using UnityEngine;

[Serializable]
public class CinematicInfo : object
{
    // public camera movement
    public GameManager.Actor actor;
    public Transform actorTransform;
    public Transform startPosition;
    public AudioSource audioSource;
    public Animator animator;
    public string trigger;
    public float duration;
    public SplineController splineController;

    public void StartAnimation(Camera cam)
    {
        actorTransform.position = startPosition.position;
        if (animator != null)
        {
            animator.SetTrigger(trigger);
        }
        Transform camParent = cam.transform.parent.transform;
        camParent.parent = splineController.transform;
        camParent.localPosition = Vector3.zero;
        camParent.localRotation = Quaternion.identity;

        splineController.Position = 0;
        splineController.Play();
    }

    
}
