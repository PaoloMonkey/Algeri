using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Controllers;
using System;
using UnityEngine;

[Serializable]
public class CinematicInfo : object
{
    public enum CameraTransition
    {
        Lerp,
        Cut
    };

    // public camera movement
    public GameManager.Actor actor;
    public Transform actorTransform;
    public Transform startPosition;
    public AudioSource audioSource;
    public Animator animator;
    public string trigger;
    public CameraTransition cameraTransition;
    public SplineController splineController;
    public Transform cameraShot;
    public float cameraShotDuration;

    public void StartAnimation(Camera cam)
    {
        if(actorTransform != null)
            actorTransform.position = startPosition.position;
        if (animator != null)
        {
            animator.SetTrigger(trigger);
        }
        if (splineController != null)
        {
            Transform camParent = cam.transform.parent.transform;
            camParent.parent = splineController.transform;
            camParent.localPosition = Vector3.zero;
            camParent.localRotation = Quaternion.identity;
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;

            splineController.Position = 0;
            splineController.Play();
        }
        else if (cameraShot != null)
        {
            Transform camParent = cam.transform.parent.transform;
            camParent.parent = cameraShot;
            camParent.localPosition = Vector3.zero;
            camParent.localRotation = Quaternion.identity;
         //   cam.transform.localPosition = Vector3.zero;
         //   cam.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError("missing camera information");
        }
    }

    
}
