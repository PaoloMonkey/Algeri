using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameaRotate : MonoBehaviour {

	public Transform pivot;

	public float smooth = 2.0F;
	public float tiltAngle = 30.0F;


	public Quaternion target;
	void Update() {
		
		if (Random.value > 0.99) {
			target = Quaternion.Euler(Random.Range(0,30), Random.Range(-180,180), Random.Range(-10,10));
		}
		transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);
	}
}
