using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public enum Actor
    {
        Transition,
        Meursault,
        Amante,
        Amico,
        Vittima,
        Prete,
        Avvocato,
        Infermiera,
        Guardiano,
        Prigioniero,
        Giudice,
        Uccello
    }

    public enum Location
    {
        Prigione,
        Spiaggia,
        Tribunale,
        CameraArdente
    }

    public enum Scene
    {
        Prete,
        Amante,
        Avvocato,
        Amico,
        Vittima,
        Veglia,
        Processo
    }

    [System.Serializable]
    public class SceneInfo : object
    {
        public string name;
        public Location location;
        public Scene scene;
        public CinematicInfo cinematic;
    }

    public SceneInfo[] sceneInfoArray;

    public Location startLocation;
    public Scene startScene;

    private Camera mainCamera;
    private InteractionManager interactionManager;
    private CinematicInfo currentCinematic = null;
    private Queue<CinematicInfo> cinematicQueue = new Queue<CinematicInfo>();


    private void Start()
    {
        interactionManager = FindObjectOfType<InteractionManager>();
        mainCamera = FindObjectOfType<Camera>();
        for (int i = 0; i < sceneInfoArray.Length; i++)
        {
            if(sceneInfoArray[i].location == startLocation 
                && sceneInfoArray[i].scene == startScene)
            {
                PlayCinematic(sceneInfoArray[i].cinematic);
                break;
            }
        }
    }

    public void Update()
    {
        if(currentCinematic == null && cinematicQueue.Count > 0)
        {
            StartCoroutine(StartCinematic(cinematicQueue.Dequeue()));
        }
        
    }

    public void PlayCinematic(CinematicInfo cinematic)
    {
        cinematicQueue.Enqueue(cinematic);
    }

    IEnumerator StartCinematic(CinematicInfo cinematic)
    {
        currentCinematic = cinematic;
    //    interactionManager.CameraPanEnabled = false;
        float elapsedTime = 0;
        
        if(cinematic.cameraTransition == CinematicInfo.CameraTransition.Lerp)
        {
            float lerpTime = 2;
            Transform camParent = mainCamera.transform.parent.transform;

            Vector3 endPos = Vector3.zero;
            Quaternion endRot = Quaternion.identity;
            if (cinematic.splineController != null)
            {
                endPos = cinematic.splineController.Spline.ControlPoints[0].position;
                endRot = cinematic.splineController.Spline.ControlPoints[0].GetOrientationFast(0);
            }
            else if (cinematic.cameraShot != null)
            {
                endPos = cinematic.cameraShot.position;
                endRot = cinematic.cameraShot.rotation;
            }
            Vector3 startPos = camParent.position;
            Quaternion startRot = camParent.rotation;
             
            while ((endPos-startPos).sqrMagnitude > 0.001f &&  elapsedTime < lerpTime)
            {
                camParent.position = Vector3.Lerp(startPos, endPos, elapsedTime / lerpTime);
                camParent.rotation = Quaternion.Lerp(startRot, endRot, elapsedTime / lerpTime);
                elapsedTime += Time.deltaTime;
                //interactionManager.CameraPanEnabled = true;
                yield return null;
            }
        }
        else if(cinematic.cameraTransition == CinematicInfo.CameraTransition.Cut)
        {
            // camParent.position = cinematic.splineController.Spline.ControlPoints[0].position;
            //  camParent.rotation = cinematic.splineController.Spline.ControlPoints[0].GetOrientationFast(0);
        }

        //cinematic.splineController.OnAnimationEnd.AddListener(AnimationEnded);
        
        cinematic.StartAnimation(mainCamera);

        elapsedTime = 0;
        if (cinematic.cameraShot != null)
        {
            while (currentCinematic != null)
            {
                //interactionManager.CameraPanEnabled = true;
                elapsedTime += Time.deltaTime;
                if (elapsedTime > currentCinematic.cameraShotDuration)
                {
                    AnimationEnded();
                    break;
                }
                yield return null;
            }
        }
        // cinematic ended
    }

    public void AnimationEnded()
    {
       // currentCinematic.splineController.Refresh();
        currentCinematic = null;
     //   interactionManager.CameraPanEnabled = true;
    }
}
