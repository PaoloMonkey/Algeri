using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ShatterToolkit;
//using LibTessDotNet;

public class FoamGenerator : MonoBehaviour {
    
    public Material objectFoamMaterial;
    public Material rockFoamMaterial;

    public float waterPlaneOffset = 0.5f; // offset from the water position to render the foam
    public float slicingOffset = 0.5f; // offset from the water position to get the intersection (for models that don't have geometry inside the water)

    float _waterHeight = 0;
    public float waterHeight { 
        set {
            _waterHeight = value;
            waterBounds = new Bounds(Vector3.up * _waterHeight, new Vector3(Mathf.Infinity,0.005f,Mathf.Infinity));
        }
        get { return _waterHeight; }
    }
    Bounds waterBounds;

    UvMapper _uvMapper;
    UvMapper uvMapper { 
        get {
            if (_uvMapper == null) _uvMapper = GetComponent<UvMapper>();
            if (_uvMapper == null) _uvMapper = gameObject.AddComponent<WorldUvMapper>(); 
            return _uvMapper; 
        } 
    }
    ColorMapper _colorMapper;
    ColorMapper colorMapper { 
        get { 
            if (_colorMapper == null) _colorMapper = gameObject.GetComponent<SolidColorMapper>(); 
            if (_colorMapper == null) _colorMapper = gameObject.AddComponent<SolidColorMapper>(); 
            return _colorMapper; 
        } 
    }

    List<GameObject> foamObjects = new List<GameObject>();

    public class CachedFoam {
        public Mesh mesh;
        public float height;
        public Mesh foamMesh;
        public CachedFoam(Mesh m, float py, Mesh f) {
            mesh = m;
            height = py;
            foamMesh = f;
        }
    }

    public static Dictionary<Mesh, List<CachedFoam>> CACHED_FOAMS = new Dictionary<Mesh, List<CachedFoam>>();

    void Awake () {
        CACHED_FOAMS.Clear();
    }

    public void AddFoam(Renderer[] Meshes) {

        var plane = new Plane(Vector3.up, Vector3.up * (waterHeight + slicingOffset));

        foreach(Renderer r in Meshes) {
            if (!r.enabled || !r.gameObject.activeInHierarchy)
                continue;
            
            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (mf == null) continue;

            // intersects with water?
            if (!r.bounds.Intersects(waterBounds))
                continue;

            Mesh mesh = mf.sharedMesh;
            CachedFoam foamInfo = null;

            if (CACHED_FOAMS.ContainsKey(mesh)) {
                // search for cached foam with same height

                foreach(CachedFoam cf in CACHED_FOAMS[mesh]) {
                    if (cf.height == r.transform.position.y) {
                        foamInfo = cf;
                        break;
                    }
                }
            }
            else
            {
                // generate foam

                Vector3 localPlanePoint, localPlaneNormal;
                ConvertPlaneToLocalSpace(plane, r.transform, out localPlanePoint, out localPlaneNormal);

                var hull = new CapOnlyHull(mesh);
                hull.GetCap(localPlanePoint, localPlaneNormal, uvMapper, colorMapper, out hull);

                Mesh foam = hull.GetMesh();

                if (foam != null) {
                    foamInfo = new CachedFoam(mesh, r.transform.position.y, foam);

                    // cache
                    if (!CACHED_FOAMS.ContainsKey(mesh))
                        CACHED_FOAMS.Add(mesh, new List<CachedFoam>());
                    CACHED_FOAMS[mesh].Add(foamInfo);
                } else {
                    // foam could not be created
                    continue;
                }
            }

            if (foamInfo != null) {
                var go = new GameObject();
				var mr = go.AddComponent<MeshRenderer>();
				mr.sharedMaterial = objectFoamMaterial;
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                go.AddComponent<MeshFilter>().sharedMesh = foamInfo.foamMesh;
                go.name = "foam_"+foamInfo.mesh.name;
                go.transform.SetParent(r.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localEulerAngles = Vector3.zero;
                go.transform.localScale = Vector3.one;
                var p = go.transform.position;
                p.y = waterHeight + waterPlaneOffset;
                go.transform.position = p;
                foamObjects.Add(go);
            }

            
        }
        //roadGen.waterHeight;
    }
    public void AddFoam(GameObject Obj) {
        AddFoam(Obj.GetComponentsInChildren<Renderer>());
    }

    public void AddRockFoam(GameObject Parent, Mesh FoamMesh, float WaterHeight) {
        if (FoamMesh == null) return;
        var foamgo = new GameObject(FoamMesh.name);
        foamgo.AddComponent<MeshFilter>().sharedMesh = FoamMesh;
        var mr = foamgo.AddComponent<MeshRenderer>();
        mr.sharedMaterial = rockFoamMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        foamgo.transform.SetParent(Parent.transform);
        foamgo.layer = Parent.layer;
        foamgo.transform.localPosition = Vector3.up * (WaterHeight + waterPlaneOffset);
        foamgo.transform.localEulerAngles = new Vector3(270,0,0);
        foamgo.transform.localScale = Vector3.one;
    }


    protected void ConvertPlaneToLocalSpace(Plane plane, Transform target, out Vector3 planePoint, out Vector3 planeNormal)
    {
        Vector3 localPoint = target.InverseTransformPoint(plane.normal * -plane.distance);
        Vector3 localNormal = target.InverseTransformDirection(plane.normal);

        localNormal.Scale(target.localScale);
        localNormal.Normalize();

        planePoint = localPoint;
        planeNormal = localNormal;
    }
}