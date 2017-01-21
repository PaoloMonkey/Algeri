using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class foamAnimator : MonoBehaviour {

	public float scale;
	// Update is called once per frame
	void Update () {
		float movement =  Mathf.PingPong(Time.time / 40, scale);
		Vector3 position = transform.position;
		position.x += - scale / 2 + movement;
		transform.position = position;
	}
}
