using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class exitGame : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	public bool loading = false;
	// Update is called once per frame
	void Update () {

		if (loading == false && Input.GetKey (KeyCode.Escape)) {
			SceneManager.LoadSceneAsync (0);
			loading = true;
		}
		
	}
}
