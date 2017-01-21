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
        cam.transform.parent = splineController.transform;
        cam.transform.localPosition = Vector3.zero;
        cam.transform.localRotation = Quaternion.identity;
        //splineController.OnAnimationEnd.AddListener(InteractionManager.AnimationEnded);

        splineController.Play();
    }

    
}
