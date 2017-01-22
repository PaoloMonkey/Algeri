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
    public GameManager.Location toLocation;
    public Transform actorTransform;
    public Transform startPosition;
    public AudioSource audioSource;
    public Animator animator;
    public string trigger;
    public CameraTransition cameraTransition;
    public SplineController splineController;
    public bool invertSpline = false;
    public Transform cameraShot;
    public float cameraShotDuration;
    public float cameraFov = 60;

    public void StartAnimation(Camera cam)
    {
        
        if (splineController != null)
        {
            Transform camParent = cam.transform.parent.transform;
            camParent.parent = splineController.transform;
            camParent.localPosition = Vector3.zero;
            camParent.localRotation = Quaternion.identity;
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;

            if(invertSpline)
            {
                splineController.Position = 1;
                splineController.Clamping = CurvyClamping.PingPong;
            }
            else
            {
                splineController.Position = 0;
                splineController.Clamping = CurvyClamping.Clamp;
            }
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
