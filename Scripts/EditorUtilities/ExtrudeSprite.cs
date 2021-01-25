using UnityEngine;
using System.Collections;
using System.Collections.Generic;


#if UNITY_EDITOR
/// <summary>
/// 
/// </summary>
/// <remarks>Source: https://forum.unity3d.com/threads/trying-extrude-a-2d-polygon-to-create-a-mesh.102629/ </remarks>
//[RequireComponent(typeof(MeshFilter)/*, typeof(MeshCollider),*/ /*typeof(PolygonCollider2D)*/)]
[ExecuteInEditMode]
public class ExtrudeSprite : MonoBehaviour
{
    //public Color extrudeColor = Color.white;
    public float frontDistance = -0.249f;
    public float backDistance = 0.249f;
    public bool keepMeshColliderOnZ0 = true;

    [HideInInspector]
    public bool working = false;

    [HideInInspector]
    public float progress = 0f;

    public PolygonCollider2D myPolyCol;

    private float oldZ = 0f;

    private void Update()
    {
        MeshCollider col = GetComponent<MeshCollider>();
        if(keepMeshColliderOnZ0 && transform.position.z!=oldZ && col!=null)
        {
            oldZ = transform.position.z;
            Mesh m = moveOnZ0(col.sharedMesh, transform.position);

            col.sharedMesh = m;
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
    }

    public Mesh moveOnZ0(Mesh mesh, Vector3 objectPos)
    {

        Mesh m = new Mesh();
        Vector3[] vertices = new Vector3[mesh.vertexCount];
        int[] tris = new int[mesh.triangles.Length];

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 1f);
        float maxZ = 0f;
        float minZ = 0f;
        float firstZ = mesh.vertices[0].z;
        for (int i = 1; i < mesh.vertices.Length; i++)
        {
            if (mesh.vertices[i].z != firstZ)
            {
                if (mesh.vertices[i].z > firstZ)
                {
                    maxZ = mesh.vertices[i].z;
                    minZ = firstZ;
                    break;
                }
                else
                {
                    minZ = mesh.vertices[i].z;
                    maxZ = firstZ;
                    break;
                }
            }
        }
        float avgZ = (maxZ + minZ) / 2;

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            
            if (mesh.vertices[i].z > avgZ)
            {
                vertices[i] = new Vector3(mesh.vertices[i].x, mesh.vertices[i].y, (-objectPos.z) + backDistance);
            }
            else
            {
                vertices[i] = new Vector3(mesh.vertices[i].x, mesh.vertices[i].y, (-objectPos.z) + frontDistance);
            }
        }

        m.vertices = vertices;
        m.triangles = mesh.triangles;
        m.RecalculateNormals();
        m.RecalculateBounds();

        return m;
    }

    public IEnumerator Generate()
    {
        working = true;
        progress = 0f;
        yield return null;
        PolygonCollider2D pol = GetComponent<PolygonCollider2D>();
        Mesh m = CreateMesh(pol.points, frontDistance, backDistance);
        if(keepMeshColliderOnZ0)
        {
            Debug.Log("pos=" + transform.position);
            m = moveOnZ0(m, transform.position);
        }

        //GetComponent<MeshFilter>().sharedMesh = m;
        DestroyImmediate(GetComponent<Collider2DOptimization.PolygonColliderOptimizer>());
        DestroyImmediate(pol);
        UnityEditor.EditorUtility.SetDirty(gameObject);
        yield return null;
        MeshCollider meshcol= gameObject.AddComponent<MeshCollider>();
        meshcol.sharedMesh = m;
        UnityEditor.EditorUtility.SetDirty(gameObject);
        working = false;
        //GetComponent<MeshRenderer>().material.color = extrudeColor;

        //pol.isTrigger = true;
        //pol.enabled = false;
    }

    public IEnumerator DeGenerate()
    {
        working = true;
        progress = 0f;
        yield return null;
        MeshCollider m = GetComponent<MeshCollider>();
        Mesh mesh = m.sharedMesh;

        //GetComponent<MeshFilter>().sharedMesh = m;
        DestroyImmediate(m);
        UnityEditor.EditorUtility.SetDirty(gameObject);
        yield return null;

        myPolyCol = gameObject.AddComponent<PolygonCollider2D>();


        yield return null;

        StartCoroutine(CreatePolygonCollider(mesh));
        //myPolyCol = p;
        //GetComponent<MeshRenderer>().material.color = extrudeColor;

        //pol.isTrigger = true;
        //pol.enabled = false;
    }

    private Mesh CreateMesh(Vector2[] poly, float frontDistance = -10, float backDistance = 10)
    {
        frontDistance = Mathf.Min(frontDistance, 0);
        backDistance = Mathf.Max(backDistance, 0);

        // convert polygon to triangles
        Triangulator triangulator = new Triangulator(poly);
        int[] tris = triangulator.Triangulate();
        Mesh m = new Mesh();
        Vector3[] vertices = new Vector3[poly.Length * 2];

        for (int i = 0; i < poly.Length; i++)
        {
            vertices[i].x = poly[i].x;
            vertices[i].y = poly[i].y;
            vertices[i].z = frontDistance;// + (keepMeshColliderOnZ0?-transform.position.z:0); // front vertex
            vertices[i + poly.Length].x = poly[i].x;
            vertices[i + poly.Length].y = poly[i].y;
            vertices[i + poly.Length].z = backDistance;// + (keepMeshColliderOnZ0 ? -transform.position.z : 0);  // back vertex    
        }
        progress = 0.25f;
        int[] triangles = new int[tris.Length * 2 + poly.Length * 6];
        int count_tris = 0;
        for (int i = 0; i < tris.Length; i += 3)
        {
            triangles[i] = tris[i];
            triangles[i + 1] = tris[i + 1];
            triangles[i + 2] = tris[i + 2];
        } // front vertices
        progress = 0.5f;
        count_tris += tris.Length;
        for (int i = 0; i < tris.Length; i += 3)
        {
            triangles[count_tris + i] = tris[i + 2] + poly.Length;
            triangles[count_tris + i + 1] = tris[i + 1] + poly.Length;
            triangles[count_tris + i + 2] = tris[i] + poly.Length;
        } // back vertices
        progress = 0.75f;
        count_tris += tris.Length;
        for (int i = 0; i < poly.Length; i++)
        {
            // triangles around the perimeter of the object
            int n = (i + 1) % poly.Length;
            triangles[count_tris] = i;
            triangles[count_tris + 1] = n;
            triangles[count_tris + 2] = i + poly.Length;
            triangles[count_tris + 3] = n;
            triangles[count_tris + 4] = n + poly.Length;
            triangles[count_tris + 5] = i + poly.Length;
            count_tris += 6;
        }
        progress = 1f;
        m.vertices = vertices;
        m.triangles = triangles;
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private IEnumerator CreatePolygonCollider(Mesh mesh)
    {
        int vertexCount=0;
        float? myZ = null;
        Dictionary<int, int> vertexToVertex = new Dictionary<int, int>();
        List<int> verticesOnFace= new List<int>();
        List<int> verticesOnFaceWithDuplicates = new List<int>();
        List<int> triangleOnFace= new List<int>();
        List<Vector2> myPath= new List<Vector2>();
        List<int> myPathI= new List<int>();

        //Debug.Log(1);
        // Creo l'array di vertici che stanno sulla faccia frontale
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            if (myZ == null)
                myZ = mesh.vertices[i].z;
            if (myZ < mesh.vertices[i].z)
            {
                myZ = mesh.vertices[i].z;
                vertexCount = 1;

                verticesOnFace.Clear();
                AddIfNotPresent(verticesOnFace, i, mesh.vertices, vertexToVertex);

                verticesOnFaceWithDuplicates.Clear();
                verticesOnFaceWithDuplicates.Add(i);
            }
            else if (myZ == mesh.vertices[i].z)
            {
                vertexCount++;
                AddIfNotPresent(verticesOnFace, i, mesh.vertices, vertexToVertex);
                verticesOnFaceWithDuplicates.Add(i);
            }
        }

        yield return null;
        //Debug.Log(2);
        // Creo l'array di triangoli che stanno sulla faccia frontale
        for (int k=0;k<mesh.triangles.Length;k+=3)
        {
            if (verticesOnFaceWithDuplicates.Contains(mesh.triangles[k]) &&
                verticesOnFaceWithDuplicates.Contains(mesh.triangles[k + 1]) &&
                verticesOnFaceWithDuplicates.Contains(mesh.triangles[k + 2]))
            {
                triangleOnFace.Add(vertexToVertex[mesh.triangles[k]]);
                triangleOnFace.Add(vertexToVertex[mesh.triangles[k+1]]);
                triangleOnFace.Add(vertexToVertex[mesh.triangles[k+2]]);
            }
        }

        yield return null;

        // Debug.Log("vof=" + verticesOnFace.ToArray().Length);
        //foreach (int d in verticesOnFace)
        // Debug.Log(d +"=" + mesh.vertices[d]);
        // Debug.Log("vof=" + verticesOnFace.ToArray().Length);
        /*for(int n=0;n<triangleOnFace.ToArray().Length;n+=3)
        {
            // Debug.Log(triangleOnFace[n] + " - " + triangleOnFace[n + 1] + " - " + triangleOnFace[n + 2]);
        }
        */

        //Debug.Log(3);
        int stop = 100;
        for (int x = 0; x < verticesOnFace.ToArray().Length; x++)
        {
            int sec = 0;
            bool notCool = false;
            // Parto da un vertice a caso della faccia
            int v = verticesOnFace[x];
            myPath.Clear();
            myPathI.Clear();
            //myPath.Add(mesh.vertices[i]);
            //myPathI.Add(i);
            int startingI = v;
            int lastV = v;
            int count = 0;
            do
            {
                count++;
                //Debug.Log(4);
                lastV = v;
                sec++;
                // Cerco i triangoli che li contengono
                for (int j = 0; j < triangleOnFace.ToArray().Length; j += 3)
                {
                    notCool = false;
                    // Debug.Log("1");
                    if (triangleOnFace[j] == v || triangleOnFace[j + 1] == v || triangleOnFace[j + 2] == v)
                    {
                        // Debug.Log("2--->" + v );

                        int start = v;
                        int end1 = triangleOnFace[j] != v ? triangleOnFace[j] : triangleOnFace[j + 1];
                        int end2 = triangleOnFace[j + 1] != v && triangleOnFace[j + 1] != end1 ? triangleOnFace[j + 1] : triangleOnFace[j + 2];


                        // Se il triangolo contiene un vertice che non sta sulla faccia lo salto
                        //if (!verticesOnFace.Contains(end1) || !verticesOnFace.Contains(end2))
                        //continue;
                        // Debug.Log("3---->" + end1 + "-" + end2);

                        int end;

                        // è brutto ma chissene, se al primo giro cerco di tornare indietro evito che altrimenti vado in loop
                        if (!(end1 == startingI && myPath.ToArray().Length == 1))
                        {
                            end = end1;
                            // Se non sto tornando su un punto già inserito
                            if (!myPathI.Contains(end))
                            {
                                // Debug.Log("4");

                                // E sto considerando un edge esterno
                                if (IsExternal(start, end, triangleOnFace.ToArray()))
                                {
                                    // Debug.Log("5");

                                    // Aggiungo il punto di fine
                                    myPath.Add(mesh.vertices[end]);
                                    myPathI.Add(end);
                                    v = end;
                                    break;
                                }
                            }
                        }

                        // è brutto ma chissene, se al primo giro cerco di tornare indietro evito che altrimenti vado in loop
                        if (!(end2 == startingI && myPath.ToArray().Length == 1))
                        {
                            // Altrimenti considero l'altro e riprovo
                            end = end2;
                            if (!myPathI.Contains(end))
                            { 
                                // Debug.Log("4");

                                if (IsExternal(start, end, triangleOnFace.ToArray()))
                                {
                                    // Debug.Log("5");

                                    myPath.Add(mesh.vertices[end]);
                                    myPathI.Add(end);
                                    v = end;
                                    break;
                                }
                            }
                        }

                        notCool = true;
                        //continue;
                        //if (!notCool) break;
                    }
                }
                if(lastV==v)
                {
                    Debug.LogError("HEEEEEEELP!!!!");
                    notCool = true;
                    break;
                }
                stop--;
                if (stop == 0)
                {
                    progress = count/mesh.triangles.Length;
                    yield return null;
                    stop = 100;
                }
            } while (startingI != v || sec>100);

            //Se ce l'ho fatta con questo vertice esco
            if (!notCool) break;
            else notCool = false;
        }

        //foreach (int d in myPathI)
        // Debug.Log(d);

        // Debug.Log("I'm setting" + myPath.ToArray().Length);
        if(PolygonIsClockwise(myPath.ToArray()))
        {
            myPath.Reverse();
        }
        myPolyCol.SetPath(0, myPath.ToArray());
        myPolyCol.pathCount = 1;
        /*if(myPolyCol.pathCount>1)
        {
            for(int l=1;l< myPolyCol.pathCount;l++)
            {
                myPolyCol.SetPath(l, new Vector2[0]);
            }
        }*/

        //Debug.Log(5);

        UnityEditor.EditorUtility.SetDirty(myPolyCol);
        UnityEditor.EditorUtility.SetDirty(gameObject);
        working = false;

    }

    private static bool PolygonIsClockwise(params Vector2[] points)
    {
        int l = points.Length;

        float sum = 0f;

        for (int i = 0; i < l; i++)
        {
            int n = i + 1 >= l - 1 ? 0 : i + 1;

            float x = points[n].x - points[i].x;
            float y = points[n].y + points[i].y;
            sum += (x * y);
        }

        return (sum < 0) ? false : true;
    }

    private static bool AddIfNotPresent(List<int> list, int index,  Vector3[] values, Dictionary<int,int> duplicateDictionary)
    {
        bool duplicateFound = false;
        for (int q = 0; q < list.ToArray().Length; q++)
        {
            if (Vector3.Distance(values[index], values[list[q]]) == 0f)
            {
                duplicateDictionary.Add(index, q);
                duplicateFound = true;
            }
        }
        if (!duplicateFound)
        {
            // Debug.Log("Inserisco " + values[index]);
            duplicateDictionary.Add(index, index);
            list.Add(index);
        }

        return (!duplicateFound);
    }


    private static bool IsExternal(int startingPoint, int endingPoint, int[] tri)
    {

        // Debug.Log("External?----->" + startingPoint + "-" + endingPoint);
        bool foundOne = false;
        for (int i = 0; i <  tri.Length; i += 3)
        {
            // Debug.Log(tri[i] + "-" + tri[i + 1] + "-" + tri[i + 2]);
            if ((startingPoint == tri[i] || startingPoint == tri[i+1] || startingPoint == tri[i+2]) &&
               (endingPoint == tri[i] || endingPoint == tri[i + 1] || endingPoint == tri[i + 2]))
            {
                if(foundOne)
                {
                    return false;
                }
                else
                {
                    foundOne = true;
                }
            }
        }

        return true;
    }
}

/// <summary>
/// 
/// </summary>
/// <remarks>Source: http://wiki.unity3d.com/index.php?title=Triangulator </remarks>
public class Triangulator
{
    private List<Vector2> m_points = new List<Vector2>();

    public Triangulator(Vector2[] points)
    {
        m_points = new List<Vector2>(points);
    }

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();

        int n = m_points.Count;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int m = 0, v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area()
    {
        int n = m_points.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = m_points[p];
            Vector2 qval = m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];
        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}
#endif