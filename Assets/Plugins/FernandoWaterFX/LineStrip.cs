using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineStrip {


    public static Mesh GetMesh(Vector3[] Points, float Thickness = 1, float MiterLimit = 0.5f, bool CollapseToCenter = false, bool ClosedLoop = true, Vector3? Origin = null) {
        
        List<Vector2[]> quads = new List<Vector2[]>();

        for(int i = 0; i < Points.Length; i++) {
            Vector3 p03d = Points[i];
            Vector3 p13d = Points[Mathf.Min(i+1, Points.Length-1)];
            Vector3 p23d = Points[Mathf.Min(i+2, Points.Length-1)];
            if (ClosedLoop) {
                p13d = Points[i == Points.Length - 1 ? 0 : i+1];
                p23d = Points[ClosedLoop && i == Points.Length - 2 ? 0 : (i == Points.Length - 1 ? 1 : i+2)];
            }

            Vector2 p0 = new Vector2(p03d.x,p03d.z);
            Vector2 p1 = new Vector2(p13d.x,p13d.z);
            Vector2 p2 = new Vector2(p23d.x,p23d.z);

            Vector2 v0 = (p1-p0).normalized;
            Vector2 v1 = (p2-p1).normalized;

            Vector2 normal = new Vector2(-v0.y, v0.x).normalized;

            // calculate the miter
            Vector2 tangent = ( v1 + v0 ).normalized;
            // find the miter, which is the normal of the tangent
            Vector2 miterLine = new Vector2( -tangent.y, tangent.x );
            //The correct length (d) of the miter can then be found by projecting it on one of the normals using the dotproduct.
            //This gives us a value that is the inverse of the desired length
            float miterLength = Thickness / Vector2.Dot(miterLine, normal);
            Vector2 miterNormal = miterLine.normalized;

            //prevent excessively long miters at sharp corners
            if (Vector2.Dot(v0, v1) < -MiterLimit) {
                miterNormal = new Vector2(-v1.y, v1.x);
                miterLength = Thickness;
            }

            // get quad points
            Vector2[] quad = new Vector2[4];
            quad[0] = p0 - normal * Thickness; //bl
            quad[1] = p0 + normal * Thickness; //tl
            quad[2] = p1 - miterNormal * miterLength; //br
            quad[3] = p1 + miterNormal * miterLength; //tr
            if (CollapseToCenter) {
                quad[1] = quad[2] = new Vector2(Origin.GetValueOrDefault().x, Origin.GetValueOrDefault().z);
            }
            //Debug.Log("point " + p0 + " q0 " + quad[0]+", q1 "+quad[1] + " q2 " + quad[2] + " q3 " + quad[3]);
            if (i > 0) {
                quad[0] = quads[i-1][2];
                quad[1] = quads[i-1][3];
            }
            quads.Add(quad);
        }
        if (ClosedLoop) {
            // adjust first quad to last mitered angle for a perfect loop
            quads[0][0] = quads[quads.Count-1][2];
            quads[0][1] = quads[quads.Count-1][3];
        }
        Mesh m = new Mesh();

        // convert to vector3s (TODO: set orientation?)
        //Vector3[] quads3d = new Vector3[quads.Count];
        for(int i = 0; i < quads.Count; i++) {
            Vector3[] quad3d = new Vector3[quads[i].Length];
            for(int j = 0; j < quads[i].Length; j++) {
                quad3d[j] = new Vector3(quads[i][j].x,0,quads[i][j].y);
            }
            AddQuad(m, quad3d, i == 0, Color.white, Color.white);
        }

        m.RecalculateBounds();
        m.RecalculateNormals();

        return m;
    }

    public static Mesh GetMesh(ArrayList Points, float Thickness = 1, float MiterLimit = 0.5f, bool CollapseToCenter = false, bool ClosedLoop = true, Vector3? Origin = null) {
        Vector3[] vs = new Vector3[Points.Count];
        for(int i = 0; i < Points.Count; i++) {
            vs[i] = (Vector3)Points[i];
        }
        return GetMesh(vs, Thickness, MiterLimit, CollapseToCenter, ClosedLoop, Origin);
    }

    private static void AddQuad(Mesh m, Vector3[] quad, bool firstLine, Color ColorFrom, Color ColorTo) {

        int vl = m.vertices.Length;

        Vector3[] vs = m.vertices;
        Vector2[] uvs = m.uv;
        Color[] cs = m.colors;
        if(!firstLine || vl == 0) {
            vs = resizeArray(vs, 4);
            uvs = resizeArray(uvs, 4);
            cs = resizeArray(cs, 4);
        } else {
            vl -= 4;
        }

        vs[vl] = quad[0];
        vs[vl+1] = quad[1];
        vs[vl+2] = quad[2];
        vs[vl+3] = quad[3];

        uvs[vl] = new Vector2(1,1);
        uvs[vl+1] = new Vector2(0,1);
        uvs[vl+2] = new Vector2(1,0);
        uvs[vl+3] = new Vector2(0,0);

        cs[vl] = ColorFrom;
        cs[vl+1] = ColorFrom;
        cs[vl+2] = ColorTo;
        cs[vl+3] = ColorTo;

        int tl = m.triangles.Length;

        int[] ts = m.triangles;
        if(!firstLine || tl == 0) ts = resizeArray(ts, 6);
        else tl -= 6;
        ts[tl] = vl;
        ts[tl+1] = vl+1;
        ts[tl+2] = vl+2;
        ts[tl+3] = vl+1;
        ts[tl+4] = vl+3;
        ts[tl+5] = vl+2;

        m.vertices = vs;
        m.uv = uvs;
        m.triangles = ts;
        m.colors = cs;

        //m.RecalculateBounds();
        //m.RecalculateNormals();
    }



    static public Vector3[] resizeArray(Vector3[] ovs, int ns) {
        Vector3[] nvs = new Vector3[ovs.Length + ns];
        for(int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
    }

    static public Vector2[] resizeArray(Vector2[] ovs, int ns) {
        Vector2[] nvs = new Vector2[ovs.Length + ns];
        for(int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
    }

    static public int[] resizeArray(int[] ovs, int ns) {
        int[] nvs = new int[ovs.Length + ns];
        for(int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
    }

    static public Color[] resizeArray(Color[] ovs, int ns) {
        Color[] nvs = new Color[ovs.Length + ns];
        for(int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
    }
}
