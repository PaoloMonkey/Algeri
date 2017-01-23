using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public enum Actor
    {
        Meursault,
        Transition,
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
        None = -1,
        Prigione,
        Spiaggia,
        CameraArdente,
        Tribunale,
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

    public Transform[] locations;
    private Location currentLocation = Location.None;
    private float locationTimer = 0;

    private Camera mainCamera;
    private InteractionManager interactionManager;
    private CinematicInfo currentCinematic = null;
    private Queue<CinematicInfo> cinematicQueue = new Queue<CinematicInfo>();
    public float automaticSceneTimer = 60;

    public AudioSource wavesSound;

    private void Start()
    {
        interactionManager = FindObjectOfType<InteractionManager>();
        mainCamera = FindObjectOfType<Camera>();
        currentLocation = startLocation;

        for (int i = 0; i < locations.Length; i++)
        {
            locations[i].gameObject.SetActive(i == (int)startLocation);
        }
    }

    public void Update()
    {
        if(currentCinematic == null && cinematicQueue.Count > 0)
        {
            StartCoroutine(StartCinematic(cinematicQueue.Dequeue()));
        }

        locationTimer += Time.deltaTime;
        if(locationTimer > automaticSceneTimer)
        {
            locationTimer = -10000;

            for (int i = 0; i < sceneInfoArray.Length; i++)
            {
                if (sceneInfoArray[i].location == currentLocation)
                    //&& sceneInfoArray[i].scene == startScene)
                {
                    PlayCinematic(sceneInfoArray[i].cinematic);
                    break;
                }
            }
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

        wavesSound.Play();

        if (cinematic.audioSource != null)
            cinematic.audioSource.Play();

        if (cinematic.actorTransform != null)
        {
            if(cinematic.teleportActor)
            {
                if (!cinematic.animator.GetCurrentAnimatorStateInfo(0).IsName("S_IDLE"))
                {
                    cinematic.animator.SetTrigger("Idle");
                    yield return null;
                }
                cinematic.animator.SetTrigger(cinematic.trigger);
                cinematic.actorTransform.position = cinematic.startPosition.position;
            }
            else
            {
                StartCoroutine(MoveActorAndDo(cinematic));
            }
        }

        Location prevLocation = Location.None;
        if (cinematic.actor == Actor.Transition)
        {
            prevLocation = currentLocation;
            currentLocation = cinematic.toLocation;
            locationTimer = 0;
        }
            

        if (cinematic.cameraFov == 0)
            cinematic.cameraFov = mainCamera.fieldOfView;
        
        if (cinematic.cameraShot != null && cinematic.actor == Actor.Transition)
        {
            for (int i = 0; i < locations.Length; i++)
            {
                var animator = locations[i].parent.gameObject.GetComponent<Animator>();
                if (i == (int)currentCinematic.toLocation)
                {
                    //locations[i].gameObject.SetActive(true);
                    if (i == (int)Location.Prigione)
                        animator.SetTrigger("prisonIn");
                    else if (i == (int)Location.CameraArdente)
                        animator.SetTrigger("morgueIn");
                    else if (i == (int)Location.Tribunale)
                        animator.SetTrigger("tribunalIn");
                    else
                        locations[i].gameObject.SetActive(true);
                }
                else if (i == (int)prevLocation)
                {
                    if (i == (int)Location.Prigione)
                        animator.SetTrigger("prisonOut");
                    else if (i == (int)Location.CameraArdente)
                        animator.SetTrigger("morgueOut");
                    else if (i == (int)Location.Tribunale)
                        animator.SetTrigger("tribunalOut");
                    else
                        locations[i].gameObject.SetActive(false);
                }
            }
        }


        if (cinematic.cameraTransition == CinematicInfo.CameraTransition.Lerp)
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
            float startFov = mainCamera.fieldOfView;
            float endFov = cinematic.cameraFov; 

            while ((endPos-startPos).sqrMagnitude > 0.001f &&  elapsedTime < lerpTime)
            {
                camParent.position = Vector3.Lerp(startPos, endPos, elapsedTime / lerpTime);
                camParent.rotation = Quaternion.Lerp(startRot, endRot, elapsedTime / lerpTime);
                mainCamera.fieldOfView = Mathf.Lerp(startFov, endFov, elapsedTime / lerpTime);
                elapsedTime += Time.deltaTime;
                //interactionManager.CameraPanEnabled = true;
                yield return null;
            }
        }
        else if(cinematic.cameraTransition == CinematicInfo.CameraTransition.Cut)
        {
            mainCamera.fieldOfView = cinematic.cameraFov;
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
                    currentCinematic = null;
                    break;
                }
                yield return null;
            }
        } 
        else if (cinematic.splineController != null && cinematic.actor == Actor.Transition)
        {
            while (currentCinematic != null)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > currentCinematic.cameraShotDuration)
                {
                    for(int i = 0; i < locations.Length; i++)
                    {
                        var animator = locations[i].parent.gameObject.GetComponent<Animator>();
                        if (i == (int)currentCinematic.toLocation)
                        {
                            locations[i].gameObject.SetActive(true);

                            if (i == (int)Location.Prigione)
                                animator.SetTrigger("prisonIn");
                            else if (i == (int)Location.CameraArdente)
                                animator.SetTrigger("morgueIn");
                            else if (i == (int)Location.Tribunale)
                                animator.SetTrigger("tribunalIn");
                            

                        }
                        else if (i == (int)prevLocation)
                        {


                            if (i == (int)Location.Prigione)
                                animator.SetTrigger("prisonOut");
                            else if (i == (int)Location.CameraArdente)
                                animator.SetTrigger("morgueOut");
                            else if (i == (int)Location.Tribunale)
                                animator.SetTrigger("tribunalOut");
                            else
                                locations[i].gameObject.SetActive(false);

                        }
                    }
                    break;
                }
                yield return null;
            }
            currentCinematic = null;
        }
        // cinematic ended
    }

    public void AnimationEnded()
    {
        if (currentCinematic != null && (currentCinematic.cameraShot == null || (currentCinematic.splineController == null || currentCinematic.actor != Actor.Transition)))
        {
            currentCinematic = null;
        }
            
     //   interactionManager.CameraPanEnabled = true;
    }

    IEnumerator MoveActorAndDo(CinematicInfo cinematic)
    {
        float elapsedTime = 0;
        Transform actorTransform = cinematic.actorTransform;
        Transform target = cinematic.startPosition;
        Vector3 startPosition = actorTransform.position;
        float duration = (actorTransform.position - target.position).magnitude;
        actorTransform.LookAt(target);
        if (cinematic.animator != null)
        {
            if (!cinematic.animator.GetCurrentAnimatorStateInfo(0).IsName("S_IDLE"))
            {
                cinematic.animator.SetTrigger("Idle");
                yield return null;
            }
            cinematic.animator.SetTrigger("Walk");
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                actorTransform.position = Vector3.Lerp(startPosition, target.position, elapsedTime / duration);
                yield return null;
            }
        }
        actorTransform.position = target.position;

        elapsedTime = 0;
        duration = 1;
        float startRotation = cinematic.actorTransform.localEulerAngles.y;
        float endRotation = target.localEulerAngles.y;
        if (cinematic.animator != null)
            cinematic.animator.SetTrigger("Idle");
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var localRotation = actorTransform.localEulerAngles;
            localRotation.y = Mathf.LerpAngle(startRotation, endRotation, elapsedTime / duration);
            actorTransform.localEulerAngles = localRotation;
            yield return null;
        }

        if (cinematic.animator != null && cinematic.trigger != "")
        {
            cinematic.animator.SetTrigger(cinematic.trigger);
        }
    }
}
