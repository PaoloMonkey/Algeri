using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class loadGame : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	public bool loading = false;
	// Update is called once per frame
	void Update () {

		if (loading == false && (Input.GetMouseButtonDown (0) || Input.GetKey (KeyCode.Return))) {
			SceneManager.LoadSceneAsync (1);
			loading = true;
		}

		if (Input.GetKey (KeyCode.Escape)) {
			Application.Quit();
		}
		
	}
}
