using System.Collections.Generic;
using UnityEngine;
using ShatterToolkit;

// FastHull.cs, stripped down to only get the cap portion of the slicing, plus injects information for the foam shader into the vertex colors
public class CapOnlyHull : IHull
{
    protected static float smallestValidLength = 0.01f;
    protected static float smallestValidRatio = 0.05f;
    
    protected bool isValid = true;
    
    protected List<Vector3> vertices;
    protected List<Vector3> normals;
    protected List<Color32> colors;
    protected List<Vector4> tangents;
    protected List<Vector2> uvs;
    protected List<int> indices;
    protected string name;

    public static List<List<Ray>> DEBUG_NORMALS = new List<List<Ray>>();
    
    public CapOnlyHull(Mesh mesh)
    {
        vertices = new List<Vector3>(mesh.vertices);
        indices = new List<int>(mesh.triangles);
        
        if (mesh.normals.Length > 0)
        {
            normals = new List<Vector3>(mesh.normals);
        }
        
        if (mesh.colors32.Length > 0)
        {
            colors = new List<Color32>(mesh.colors32);
        }
        
        if (mesh.tangents.Length > 0)
        {
            tangents = new List<Vector4>(mesh.tangents);
        }
        
        if (mesh.uv.Length > 0)
        {
            uvs = new List<Vector2>(mesh.uv);
        }

        name = mesh.name;
    }
    
    public CapOnlyHull(CapOnlyHull reference)
    {
        vertices = new List<Vector3>(reference.vertices.Count);
        indices = new List<int>(reference.indices.Count);
        
        if (reference.normals != null)
        {
            normals = new List<Vector3>(reference.normals.Count);
        }
        
        if (reference.colors != null)
        {
            colors = new List<Color32>(reference.colors.Count);
        }
        
        if (reference.tangents != null)
        {
            tangents = new List<Vector4>(reference.tangents.Count);
        }
        
        if (reference.uvs != null)
        {
            uvs = new List<Vector2>(reference.uvs.Count);
        }
    }
    
    public bool IsEmpty
    {
        get { return !isValid || vertices.Count < 3 || indices.Count < 3; }
    }
    
    public Mesh GetMesh()
    {
        if (isValid)
        {
            Mesh mesh = new Mesh();
            
            // Required properties
            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();
            
            // Optional properties
            if (normals != null)
            {
                mesh.normals = normals.ToArray();
            }
            
            if (colors != null)
            {
                mesh.colors32 = colors.ToArray();
            }
            
            if (tangents != null)
            {
                mesh.tangents = tangents.ToArray();
            }
            
            if (uvs != null)
            {
                mesh.uv = uvs.ToArray();
            }
            
            return mesh;
        }
        
        return null;
    }
    
    public void Split(Vector3 localPointOnPlane, Vector3 localPlaneNormal, bool fillCut, UvMapper uvMapper, ColorMapper colorMapper, out IHull resultA, out IHull resultB)
    {
        resultA = null;
        resultB = null;
    }

    // copy of FastHull.Split() 
    public void GetCap(Vector3 localPointOnPlane, Vector3 localPlaneNormal, UvMapper uvMapper, ColorMapper colorMapper, out CapOnlyHull resultCap)
    {
        if (localPlaneNormal == Vector3.zero)
        {
            localPlaneNormal = Vector3.up;
        }

        CapOnlyHull a = new CapOnlyHull(this);
        CapOnlyHull b = new CapOnlyHull(this);
        CapOnlyHull cap = new CapOnlyHull(this);

        bool[] vertexAbovePlane;
        int[] oldToNewVertexMap;

        AssignVertices(a, b, localPointOnPlane, localPlaneNormal, out vertexAbovePlane, out oldToNewVertexMap);

        IList<Vector3> cutEdges;

        AssignTriangles(a, b, vertexAbovePlane, oldToNewVertexMap, localPointOnPlane, localPlaneNormal, out cutEdges);

        if (colors != null && colorMapper == null)
        {
            Debug.LogWarning("Fill cut failed: A ColorMapper was not provided even though the mesh has a color channel");
        }
        else if ((tangents != null || uvs != null) && uvMapper == null)
        {
            Debug.LogWarning("Fill cut failed: A UvMapper was not provided even though the mesh has a tangent/uv channel");
        }
        else
        {
            FillCutEdges(a, b, cutEdges, localPlaneNormal, uvMapper, colorMapper, cap);
        }

        //ValidateOutput(cap, localPlaneNormal);

        for(int i = 0; i < cap.vertices.Count; i++) {
            var p = cap.vertices[i];
            p.y = 0;
            cap.vertices[i] = p;
        }

        // Set output
        resultCap = cap;
    }
    
    protected void AssignVertices(CapOnlyHull a, CapOnlyHull b, Vector3 pointOnPlane, Vector3 planeNormal, out bool[] vertexAbovePlane, out int[] oldToNewVertexMap)
    {
        vertexAbovePlane = new bool[vertices.Count];
        oldToNewVertexMap = new int[vertices.Count];
        
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vertex = vertices[i];
            
            bool abovePlane = Vector3.Dot(vertex - pointOnPlane, planeNormal) >= 0.0f;
            
            vertexAbovePlane[i] = abovePlane;
            
            if (abovePlane)
            {
                // Assign vertex to hull A
                oldToNewVertexMap[i] = a.vertices.Count;
                
                a.vertices.Add(vertex);
                
                if (normals != null)
                {
                    a.normals.Add(normals[i]);
                }
                
                if (colors != null)
                {
                    a.colors.Add(colors[i]);
                }
                
                if (tangents != null)
                {
                    a.tangents.Add(tangents[i]);
                }
                
                if (uvs != null)
                {
                    a.uvs.Add(uvs[i]);
                }
            }
            else
            {
                // Assign vertex to hull B
                oldToNewVertexMap[i] = b.vertices.Count;
                
                b.vertices.Add(vertex);
                
                if (normals != null)
                {
                    b.normals.Add(normals[i]);
                }
                
                if (colors != null)
                {
                    b.colors.Add(colors[i]);
                }
                
                if (tangents != null)
                {
                    b.tangents.Add(tangents[i]);
                }
                
                if (uvs != null)
                {
                    b.uvs.Add(uvs[i]);
                }
            }
        }
    }
    
    protected void AssignTriangles(CapOnlyHull a, CapOnlyHull b, bool[] vertexAbovePlane, int[] oldToNewVertexMap, Vector3 pointOnPlane, Vector3 planeNormal, out IList<Vector3> cutEdges)
    {
        cutEdges = new List<Vector3>();
        
        int triangleCount = indices.Count / 3;
        
        for (int i = 0; i < triangleCount; i++)
        {
            int index0 = indices[i * 3 + 0];
            int index1 = indices[i * 3 + 1];
            int index2 = indices[i * 3 + 2];
            
            bool above0 = vertexAbovePlane[index0];
            bool above1 = vertexAbovePlane[index1];
            bool above2 = vertexAbovePlane[index2];
            
            if (above0 && above1 && above2)
            {
                // Assign triangle to hull A
                a.indices.Add(oldToNewVertexMap[index0]);
                a.indices.Add(oldToNewVertexMap[index1]);
                a.indices.Add(oldToNewVertexMap[index2]);
            }
            else if (!above0 && !above1 && !above2)
            {
                // Assign triangle to hull B
                b.indices.Add(oldToNewVertexMap[index0]);
                b.indices.Add(oldToNewVertexMap[index1]);
                b.indices.Add(oldToNewVertexMap[index2]);
            }
            else
            {
                // Split triangle
                int top, cw, ccw;
                
                if (above1 == above2 && above0 != above1)
                {
                    top = index0;
                    cw = index1;
                    ccw = index2;
                }
                else if (above2 == above0 && above1 != above2)
                {
                    top = index1;
                    cw = index2;
                    ccw = index0;
                }
                else
                {
                    top = index2;
                    cw = index0;
                    ccw = index1;
                }
                
                Vector3 cutVertex0, cutVertex1;
                
                if (vertexAbovePlane[top])
                {
                    SplitTriangle(a, b, oldToNewVertexMap, pointOnPlane, planeNormal, top, cw, ccw, out cutVertex0, out cutVertex1);
                }
                else
                {
                    SplitTriangle(b, a, oldToNewVertexMap, pointOnPlane, planeNormal, top, cw, ccw, out cutVertex1, out cutVertex0);
                }
                
                // Add cut edge
                if (cutVertex0 != cutVertex1)
                {
                    cutEdges.Add(cutVertex0);
                    cutEdges.Add(cutVertex1);
                }
            }
        }
    }
    
    protected void SplitTriangle(CapOnlyHull topHull, CapOnlyHull bottomHull, int[] oldToNewVertexMap, Vector3 pointOnPlane, Vector3 planeNormal, int top, int cw, int ccw, out Vector3 cwIntersection, out Vector3 ccwIntersection)
    {
        Vector3 v0 = vertices[top];
        Vector3 v1 = vertices[cw];
        Vector3 v2 = vertices[ccw];
        
        // Intersect the top-cw edge with the plane
        float cwDenominator = Vector3.Dot(v1 - v0, planeNormal);
        float cwScalar = Mathf.Clamp01(Vector3.Dot(pointOnPlane - v0, planeNormal) / cwDenominator);
        
        // Intersect the top-ccw edge with the plane
        float ccwDenominator = Vector3.Dot(v2 - v0, planeNormal);
        float ccwScalar = Mathf.Clamp01(Vector3.Dot(pointOnPlane - v0, planeNormal) / ccwDenominator);
        
        // Interpolate vertex positions
        Vector3 cwVertex = new Vector3();
        
        cwVertex.x = v0.x + (v1.x - v0.x) * cwScalar;
        cwVertex.y = v0.y + (v1.y - v0.y) * cwScalar;
        cwVertex.z = v0.z + (v1.z - v0.z) * cwScalar;
        
        Vector3 ccwVertex = new Vector3();
        
        ccwVertex.x = v0.x + (v2.x - v0.x) * ccwScalar;
        ccwVertex.y = v0.y + (v2.y - v0.y) * ccwScalar;
        ccwVertex.z = v0.z + (v2.z - v0.z) * ccwScalar;
        
        // Create top triangle
        int cwA = topHull.vertices.Count;
        topHull.vertices.Add(cwVertex);
        
        int ccwA = topHull.vertices.Count;
        topHull.vertices.Add(ccwVertex);
        
        topHull.indices.Add(oldToNewVertexMap[top]);
        topHull.indices.Add(cwA);
        topHull.indices.Add(ccwA);
        
        // Create bottom triangles
        int cwB = bottomHull.vertices.Count;
        bottomHull.vertices.Add(cwVertex);
        
        int ccwB = bottomHull.vertices.Count;
        bottomHull.vertices.Add(ccwVertex);
        
        bottomHull.indices.Add(oldToNewVertexMap[cw]);
        bottomHull.indices.Add(oldToNewVertexMap[ccw]);
        bottomHull.indices.Add(ccwB);
        
        bottomHull.indices.Add(oldToNewVertexMap[cw]);
        bottomHull.indices.Add(ccwB);
        bottomHull.indices.Add(cwB);
        
        // Interpolate normals
        if (normals != null)
        {
            Vector3 n0 = normals[top];
            Vector3 n1 = normals[cw];
            Vector3 n2 = normals[ccw];
            
            Vector3 cwNormal = new Vector3();
            
            cwNormal.x = n0.x + (n1.x - n0.x) * cwScalar;
            cwNormal.y = n0.y + (n1.y - n0.y) * cwScalar;
            cwNormal.z = n0.z + (n1.z - n0.z) * cwScalar;
            
            cwNormal.Normalize();
            
            Vector3 ccwNormal = new Vector3();
            
            ccwNormal.x = n0.x + (n2.x - n0.x) * ccwScalar;
            ccwNormal.y = n0.y + (n2.y - n0.y) * ccwScalar;
            ccwNormal.z = n0.z + (n2.z - n0.z) * ccwScalar;
            
            ccwNormal.Normalize();
            
            // Add vertex property
            topHull.normals.Add(cwNormal);
            topHull.normals.Add(ccwNormal);
            
            bottomHull.normals.Add(cwNormal);
            bottomHull.normals.Add(ccwNormal);
        }
        
        // Interpolate colors
        if (colors != null)
        {
            Color32 c0 = colors[top];
            Color32 c1 = colors[cw];
            Color32 c2 = colors[ccw];
            
            Color32 cwColor = Color32.Lerp(c0, c1, cwScalar);
            Color32 ccwColor = Color32.Lerp(c0, c2, ccwScalar);
            
            // Add vertex property
            topHull.colors.Add(cwColor);
            topHull.colors.Add(ccwColor);
            
            bottomHull.colors.Add(cwColor);
            bottomHull.colors.Add(ccwColor);
        }
        
        // Interpolate tangents
        if (tangents != null)
        {
            Vector4 t0 = tangents[top];
            Vector4 t1 = tangents[cw];
            Vector4 t2 = tangents[ccw];
            
            Vector4 cwTangent = new Vector4();
            
            cwTangent.x = t0.x + (t1.x - t0.x) * cwScalar;
            cwTangent.y = t0.y + (t1.y - t0.y) * cwScalar;
            cwTangent.z = t0.z + (t1.z - t0.z) * cwScalar;
            
            cwTangent.Normalize();
            cwTangent.w = t1.w;
            
            Vector4 ccwTangent = new Vector4();
            
            ccwTangent.x = t0.x + (t2.x - t0.x) * ccwScalar;
            ccwTangent.y = t0.y + (t2.y - t0.y) * ccwScalar;
            ccwTangent.z = t0.z + (t2.z - t0.z) * ccwScalar;
            
            ccwTangent.Normalize();
            ccwTangent.w = t2.w;
            
            // Add vertex property
            topHull.tangents.Add(cwTangent);
            topHull.tangents.Add(ccwTangent);
            
            bottomHull.tangents.Add(cwTangent);
            bottomHull.tangents.Add(ccwTangent);
        }
        
        // Interpolate uvs
        if (uvs != null)
        {
            Vector2 u0 = uvs[top];
            Vector2 u1 = uvs[cw];
            Vector2 u2 = uvs[ccw];
            
            Vector2 cwUv = new Vector2();
            
            cwUv.x = u0.x + (u1.x - u0.x) * cwScalar;
            cwUv.y = u0.y + (u1.y - u0.y) * cwScalar;
            
            Vector2 ccwUv = new Vector2();
            
            ccwUv.x = u0.x + (u2.x - u0.x) * ccwScalar;
            ccwUv.y = u0.y + (u2.y - u0.y) * ccwScalar;
            
            // Add vertex property
            topHull.uvs.Add(cwUv);
            topHull.uvs.Add(ccwUv);
            
            bottomHull.uvs.Add(cwUv);
            bottomHull.uvs.Add(ccwUv);
        }
        
        // Set output
        cwIntersection = cwVertex;
        ccwIntersection = ccwVertex;
    }
    
    protected void FillCutEdges(CapOnlyHull a, CapOnlyHull b, IList<Vector3> edges, Vector3 planeNormal, UvMapper uvMapper, ColorMapper colorMapper, CapOnlyHull capResult)
    {
        int edgeCount = edges.Count / 2;
        
        List<Vector3> points = new List<Vector3>(edgeCount);
        List<int> outline = new List<int>(edgeCount * 2);
        List<List<int>> separateOutlines = new List<List<int>>();
        int start = 0;
        //int outlineIndex = 0;
        for (int current = 0; current < edgeCount; current++)
        {
            int next = current + 1;
            
            // Find the next edge
            int nearest = start;
            float nearestDistance = (edges[current * 2 + 1] - edges[start * 2 + 0]).sqrMagnitude;
            
            for (int other = next; other < edgeCount; other++)
            {
                float distance = (edges[current * 2 + 1] - edges[other * 2 + 0]).sqrMagnitude;
                
                if (distance < nearestDistance)
                {
                    nearest = other;
                    nearestDistance = distance;
                }
            }
            
            // Is the current edge the last edge in this edge loop?
            if (nearest == start && current > start)
            {
                int pointStart = points.Count;
                int pointCounter = pointStart;

                var currentOutline = new List<int>();

                // Add this edge loop to the triangulation lists
                for (int edge = start; edge < current; edge++)
                {
                    points.Add(edges[edge * 2 + 0]);
                    outline.Add(pointCounter++);
                    outline.Add(pointCounter);
                    currentOutline.Add(points.Count-1);

                }
                
                points.Add(edges[current * 2 + 0]);
                outline.Add(pointCounter);
                outline.Add(pointStart);
                currentOutline.Add(points.Count-1);

                separateOutlines.Add(currentOutline);
                
                // Start a new edge loop
                start = next;
            }
            else if (next < edgeCount)
            {
                // Move the nearest edge so that it follows the current edge
                Vector3 n0 = edges[next * 2 + 0];
                Vector3 n1 = edges[next * 2 + 1];
                
                edges[next * 2 + 0] = edges[nearest * 2 + 0];
                edges[next * 2 + 1] = edges[nearest * 2 + 1];
                
                edges[nearest * 2 + 0] = n0;
                edges[nearest * 2 + 1] = n1;
            }
        }

        if (points.Count > 0)
        {
            // Triangulate the outline
            int[] newEdges, newTriangles, newTriangleEdges;

            // flatten
            for(int i = 0; i < points.Count; i++) {
                var p = points[i];
                p.y = 0;
                points[i] = p;
            }
            
            ShatterToolkit.Triangulator triangulator = new ShatterToolkit.Triangulator(points, outline, planeNormal);
            
            triangulator.Fill(out newEdges, out newTriangles, out newTriangleEdges);

            // inject 2D normals into color for vetex shader to extrude outwards
            List<Color32> newColors = new List<Color32>();

            for(int i = 0; i < points.Count; i++) {
                // fill up the colors list
                newColors.Add(Color.black);

            } 


            // for each outline, calculate the vector of each point
            foreach(var loop in separateOutlines) {
                // find the average point
                /*
                Vector2 center = Vector2.zero;
                for(int i = 0; i < loop.Count; i++) {
                    center += loop[i];
                }
                center /= loop.Count;
*/
                var debugNormalsInfo = new List<Ray>();
                for(int i = 0; i < loop.Count; i++) {
                    int vert0 = loop[ i ];
                    int vert1 = loop[ (i + 1) % loop.Count ];
                    int vert2 = loop[ (i + 2) % loop.Count ];

                    Vector2 p0 = new Vector2(points[vert0].x,points[vert0].z);
                    Vector2 p1 = new Vector2(points[vert1].x,points[vert1].z);
                    Vector2 p2 = new Vector2(points[vert2].x,points[vert2].z);

                    /*

                    var p0_n = p1 + (p0 - p1);
                    var p2_n = p1 + (p2 - p0);

                    var v = p2_n - p0_n;
                    var pwat = new Vector2(-v.y, v.x) / Mathf.Sqrt(v.x*v.x + v.y*v.y);
                    var dir = (pwat - p1).normalized;
                    */
                    Vector2 v0 = (p1-p0).normalized;
                    Vector2 v1 = (p2-p1).normalized;

                    //Vector2 normal = new Vector2(-v0.y, v0.x).normalized;

                    // calculate the miter
                    Vector2 tangent = ( v1 + v0 ).normalized;
                    // find the miter, which is the normal of the tangent
                    Vector2 dir = new Vector2( -tangent.y, tangent.x ).normalized;

                    Vector3 result = new Vector3(dir.x,0,dir.y);

                    result = result * 0.5f + Vector3.one * 0.5f; // -1>1 to 0>1
                    newColors[vert1] = new Color(result.x,result.y,result.z,1.0f); // convert it to a color

                    debugNormalsInfo.Add(new Ray(new Vector3(p1.x,0,p1.y), -Vector3.one + result * 2));

                }

                for(int i = 0; i < loop.Count; i++) {
                    
                }
                DEBUG_NORMALS.Add(debugNormalsInfo);
            }
            

            capResult.vertices = points;
            capResult.indices = new List<int>(newTriangles);
            capResult.uvs = null;
            capResult.tangents = null;
            capResult.colors = newColors;
        }
    }
    
    protected void ValidateOutput(CapOnlyHull result, Vector3 planeNormal)
    {
        float length = result.LengthAlongAxis(planeNormal);
        
        if (length < smallestValidLength)
        {
            result.isValid = false;
        }
    }
    
    protected float LengthAlongAxis(Vector3 axis)
    {
        if (vertices.Count > 0)
        {
            float min = Vector3.Dot(vertices[0], axis);
            float max = min;
            
            foreach (Vector3 vertex in vertices)
            {
                float distance = Vector3.Dot(vertex, axis);
                
                min = Mathf.Min(distance, min);
                max = Mathf.Max(distance, max);
            }
            
            return max - min;
        }
        
        return 0.0f;
    }
}