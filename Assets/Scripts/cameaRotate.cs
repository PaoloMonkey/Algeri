using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameaRotate : MonoBehaviour {

	public Transform pivot;

	public float smooth = 2.0F;
	public float tiltAngle = 30.0F;


	public Quaternion target;
	/*void Update() {
		
		if (Random.value > 0.99) {
			target = Quaternion.Euler(Random.Range(0,30), Random.Range(-180,180), Random.Range(-10,10));
		}

		target = Quaternion.Euler(Random.Range(0,30), Random.Range(-180,180), Random.Range(-10,10));
		transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);
	}*/

	public Vector3 delta = Vector3.zero;
	private Vector3 lastPos = Vector3.zero;
	public bool thisFrame = true;

	void Update()
	{
		
		if (thisFrame)
		{
			lastPos = Input.mousePosition;
			thisFrame = false;
		}
		else 
		{
			delta += Input.mousePosition - lastPos;

			// Do Stuff here

			Debug.Log( "delta X : " + delta.x );
			Debug.Log( "delta Y : " + delta.y );

			// End do stuff

			lastPos = Input.mousePosition;
			thisFrame = true;
		}

		float tiltAroundZ = delta.x * tiltAngle;
		float tiltAroundX = delta.y * tiltAngle;
		Quaternion target = Quaternion.Euler(tiltAroundX,tiltAroundZ,0 );
		transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);
	}
}
