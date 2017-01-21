using UnityEngine;
using System.Collections;
using ShatterToolkit;

public class FoamGeneratorShatter : MonoBehaviour {

    public GameObject target;

    public Vector3 waterPosition = Vector3.zero;
    public Material foamMaterial;

	// Use this for initialization
	void Start () {
        GetFoam(target);

        //target.SendMessage("Split", new Plane[] { new Plane(Vector3.up,waterPosition) });
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void GetFoam(GameObject obj) {
        MeshFilter mf = obj.GetComponent<MeshFilter>();

        var plane = new Plane(Vector3.up, waterPosition);

        UvMapper uvMapper = GetComponent<UvMapper>();
        ColorMapper colorMapper = GetComponent<ColorMapper>();

        Vector3 localPlanePoint, localPlaneNormal;
        ConvertPlaneToLocalSpace(plane, out localPlanePoint, out localPlaneNormal);

        var hull = new CapOnlyHull(mf.sharedMesh);
        hull.GetCap(localPlanePoint, localPlaneNormal, uvMapper, colorMapper, out hull);

        var go = new GameObject(obj.name+"_foam");
        go.transform.position = obj.transform.position;
        go.transform.rotation = obj.transform.rotation;
        go.transform.localScale = obj.transform.localScale;
        go.AddComponent<MeshFilter>().sharedMesh = hull.GetMesh();
        go.AddComponent<MeshRenderer>().material = foamMaterial;

        obj.SetActive(false);

    }

    protected void ConvertPlaneToLocalSpace(Plane plane, out Vector3 planePoint, out Vector3 planeNormal)
    {
        Vector3 localPoint = target.transform.InverseTransformPoint(plane.normal * -plane.distance);
        Vector3 localNormal = target.transform.InverseTransformDirection(plane.normal);

        localNormal.Scale(transform.localScale);
        localNormal.Normalize();

        planePoint = localPoint;
        planeNormal = localNormal;
    }

    protected void ConvertPlanesToLocalspace(Plane[] planes, out Vector3[] points, out Vector3[] normals)
    {
        points = new Vector3[planes.Length];
        normals = new Vector3[planes.Length];

        for (int i = 0; i < planes.Length; i++)
        {
            Plane plane = planes[i];

            Vector3 localPoint = target.transform.InverseTransformPoint(plane.normal * -plane.distance);
            Vector3 localNormal = target.transform.InverseTransformDirection(plane.normal);

            localNormal.Scale(transform.localScale);
            localNormal.Normalize();

            points[i] = localPoint;
            normals[i] = localNormal;
        }
    }
}
