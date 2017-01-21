using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public enum Actor
    {
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
        interactionManager.CameraPanEnabled = false;
        float elapsedTime = 0;
        if(cinematic.splineController != null)
        {
            float lerpTime = 2;
            Vector3 startPos = mainCamera.transform.position;
            Vector3 endPos = cinematic.splineController.Spline.ControlPoints[0].position;
            Quaternion startRot = mainCamera.transform.rotation;
            Quaternion edRot = cinematic.splineController.Spline.ControlPoints[0].GetOrientationFast(0);
            while (elapsedTime < lerpTime)
            {
                mainCamera.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / lerpTime);
                mainCamera.transform.rotation = Quaternion.Lerp(startRot, edRot, elapsedTime / lerpTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        
            //cinematic.splineController.OnAnimationEnd.AddListener(AnimationEnded);
            cinematic.StartAnimation(mainCamera);
        }
        elapsedTime = 0;
        while (elapsedTime < currentCinematic.duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // cinematic ended
    }

    public void AnimationEnded()
    {
       // currentCinematic.splineController.Refresh();
        currentCinematic = null;
        interactionManager.CameraPanEnabled = true;
    }
}
