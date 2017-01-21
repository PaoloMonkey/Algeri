using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour {

    private Camera mainCamera;
    private Vector2 screenSize;
    private Vector3 fixedPos;
    private Vector3 fixedRot;

    private bool cameraPanEnabled = true;
    public bool CameraPanEnabled
    {
        set
        {
            cameraPanEnabled = value;
            if(value == true)
            {
                fixedPos = mainCamera.transform.position;
                fixedRot = mainCamera.transform.localEulerAngles;
            }
        }
        get
        {
            return cameraPanEnabled;
        }
    }

    public Vector2 cameraPan = Vector2.zero;
    public Vector2 cameraRotation = Vector2.zero;

    public Texture2D cursorNormal;
    public Texture2D cursorInteractive;

    private void Awake()
    {
        mainCamera = FindObjectOfType<Camera>();
        screenSize = new Vector2(Screen.width, Screen.height);
        fixedPos = mainCamera.transform.position;
        fixedRot = mainCamera.transform.localEulerAngles;

        Cursor.visible = true;
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
            mainCamera.transform.position = newPos;

            Vector3 newRot = fixedRot;
            newRot.y = fixedRot.y + cameraRotation.x * (mousePosition.x - screenSize.x / 2) / screenSize.x;
            newRot.x = fixedRot.x - cameraRotation.y * (mousePosition.y - screenSize.y / 2) / screenSize.y;

            mainCamera.transform.localEulerAngles = newRot;
        }

        bool canInteract = false;
        RaycastHit vHit = new RaycastHit();
        Ray vRay = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(vRay, out vHit, 100))
        {
            Prop prop = vHit.collider.gameObject.GetComponent<Prop>();
            if (prop != null)
                canInteract = true;        
        }
        
        if(canInteract)
            Cursor.SetCursor(cursorInteractive, Vector2.zero, CursorMode.Auto);
        else
            Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
    }
}
