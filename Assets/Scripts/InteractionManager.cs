using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour {

    private Camera mainCamera;
    private Vector2 screenSize;
    private Vector2 fixedPos;

    public Vector2 cameraPan = Vector2.zero;

    private void Awake()
    {
        mainCamera = FindObjectOfType<Camera>();
        screenSize = new Vector2(Screen.width, Screen.height);
        fixedPos = mainCamera.transform.position;
    }

	// Update is called once per frame
	void Update () {
        Vector2 mousePosition = Input.mousePosition;

        // change camera pos/rotation
        mainCamera.transform.position
	}
}
