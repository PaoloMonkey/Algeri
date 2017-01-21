using UnityEngine;
using System.Collections;

public class DebugFoam : MonoBehaviour {

    public Transform objectParent;
    public FoamGenerator foamGen;
    public Transform water;

    public bool hideObjects = false;

	// Use this for initialization
	void Awake () {
        foamGen.waterHeight = water.position.y;

        foreach(Transform tr in objectParent) {
            Renderer[] rs = tr.GetComponentsInChildren<MeshRenderer>();
            foamGen.AddFoam(rs);
            if (hideObjects) {
                foreach(Renderer r in rs) { r.enabled = false; }
            }
        }
	}

    void Update() {
        
    }
    #if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        foreach(var loop in CapOnlyHull.DEBUG_NORMALS) {
            for(int i = 0; i < loop.Count; i++) {
                Ray current = loop[i];
                Ray next = loop[(i + 1) % loop.Count];

                Debug.DrawLine(current.origin,next.origin,Color.red);
                Debug.DrawLine(current.origin,current.origin + current.direction * 0.05f,Color.blue);

                //var c = UnityEditor.SceneView.lastActiveSceneView.camera;
                UnityEditor.Handles.Label(current.origin, "#"+i);

                /*Debug.DrawLine(current.origin,current.origin + 
                    new Vector3(-1 + current.direction.x * 2,
                        0,
                        -1 + current.direction.z * 2) * 0.05f,Color.green);*/
            }
        }
    }

    void OnDisable() {
        CapOnlyHull.DEBUG_NORMALS.Clear();
    }
    #endif
}
