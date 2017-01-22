using FluffyUnderware.Curvy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour {

    private GameManager gameManager;
    private Camera mainCamera;
    private Vector2 screenSize;
    private Vector3 fixedPos;
    private Vector3 fixedRot;
    private Prop currentProp;
    private bool canInteract = true;

    private bool cameraPanEnabled = true;
    public bool CameraPanEnabled
    {
        set
        {
            cameraPanEnabled = value;
            if(value == true)
            {
                //fixedPos = mainCamera.transform.position;
                //fixedRot = mainCamera.transform.rotation.eulerAngles;
                fixedPos = mainCamera.transform.localPosition;
                fixedRot = mainCamera.transform.localRotation.eulerAngles;
            }
        }
        get
        {
            return cameraPanEnabled;
        }
    }

    public Vector2 cameraPan = Vector2.zero;
    public Vector2 cameraRotation = Vector2.zero;

    public Image cursor;
    public Sprite cursorNormal;
    public Sprite cursorInteractive;
    public Sprite cursorCinematic;

    private float elapsedTime = 0.0f;
    public float interactionCooldowTime = 2.0f;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        mainCamera = FindObjectOfType<Camera>();
        screenSize = new Vector2(Screen.width, Screen.height);
        fixedPos = Vector3.zero;
        fixedRot = Vector3.zero;

        Cursor.visible = false;
    }

	// Update is called once per frame
	void Update () {

        Vector2 mousePosition = Input.mousePosition;

        if (cameraPanEnabled)
        {
            Vector3 newPos = fixedPos;
            newPos.x = fixedPos.x + cameraPan.x * (mousePosition.x - screenSize.x / 2) / screenSize.x;
            newPos.y = fixedPos.y + cameraPan.y * (mousePosition.y - screenSize.y / 2) / screenSize.y;

            // change camera pos/rotation
            mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, newPos, 0.1f);

            Vector3 newRot = fixedRot;
            newRot.y = fixedRot.y + cameraRotation.x * (mousePosition.x - screenSize.x / 2) / screenSize.x;
            newRot.x = fixedRot.x - cameraRotation.y * (mousePosition.y - screenSize.y / 2) / screenSize.y;

            mainCamera.transform.localRotation = Quaternion.Lerp(mainCamera.transform.localRotation, Quaternion.Euler(newRot), 0.1f);
        }

        bool isSelectingInteraction = false;
        if (canInteract)
        {
            RaycastHit vHit = new RaycastHit();
            Ray vRay = mainCamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(vRay, out vHit, 100))
            {
                currentProp = vHit.collider.gameObject.GetComponent<Prop>();
                if (currentProp != null && currentProp.active)
                {
                    isSelectingInteraction = true;
                }
            }
        }

        cursor.rectTransform.position = mousePosition;
        if(canInteract)
        {
            if (isSelectingInteraction)
                cursor.sprite = cursorInteractive;
            else
                cursor.sprite = cursorNormal;
        }
        else
        {
            cursor.sprite = cursorCinematic;
        }

        if(Input.GetMouseButtonUp(0) && currentProp != null)
        {
            canInteract = false;
            gameManager.PlayCinematic(currentProp.cinematic);
            currentProp = null;
            elapsedTime = 0.0f;
        }

        if (!canInteract)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > interactionCooldowTime)
            {
                canInteract = true;
            }
        }

       /* if(!canInteract && currentProp != null &&  currentProp.AnimationEnded())
        {
            canInteract = true;
        }*/
    }

    public void AnimationEnded()
    {
        RestoreInteraction();
        mainCamera.transform.transform.parent.parent = null;
    }

    public void ForceNoInteraction()
    {
        canInteract = false;
    }

    public void RestoreInteraction()
    {
        canInteract = true;
    }
}
