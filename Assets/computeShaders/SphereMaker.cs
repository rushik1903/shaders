using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SphereMaker : MonoBehaviour
{
    public int maxNumberOfBreaks = 10;
    public bool spawnDebugCubes = true;
    public float breakingFrequency = 1000;
    public AnimationCurve distToWtCurve;
    public int initialBreaks;
    public Craters craters;
    public float radius = 5;
    public float testValue = 1;
    public ComputeShader planetHeightShader;
    public int edgeCuts = 3;
    public GameObject camProxy;
    private MeshCollider meshCollider;
    private MeshCollider invertedMeshCollider;
    public GameObject camera;
    public GameObject prefabTriangleCenter;
    public GameObject prefab;
    Mesh mesh;
    float gr = 1.6180339887499f;

    Vector3[] initialVertices;

    Vector3[] vertices;
    public float[] heights;
    public List<Vector3> verticesList = new List<Vector3>();
    int[] triangles;
    public List<int> trianglesList = new List<int>();
    int[] trianglesBreakenWts;
    public List<int> trianglesBreakenWtsList = new List<int>();

    public List<int> breakableVertices = new List<int>();

    public List<int> wt = new List<int>();


    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        meshCollider = GetComponent<MeshCollider>();
        meshCollider.convex = true;
        GetComponent<MeshFilter>().mesh = mesh;

        //invertedMeshCollider = gameObject.AddComponent<MeshCollider>();

        CreateShapeIcosohedron();
        //CreateShapeCube();
        //CreatePlane();

        //initialising trianglesBreakenWts
        trianglesBreakenWts = new int[(int)(triangles.Length/3)];
        Array.Clear(trianglesBreakenWts, 0, trianglesBreakenWts.Length);
        trianglesBreakenWtsList = trianglesBreakenWts.ToList();

        FlipNormals();
        UpdateMesh();
        DrawEdges();
        meshCollider.sharedMesh = mesh;

        //calling vertices to break initially
        //doesnt work anymore
        for (int i = 0; i < initialBreaks; i++)
        {
            CheckDistances();
            UpdateMesh();
        }
    }

    private void FlipNormals()
    {
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        mesh.normals = normals;
    }

    void CreateShapeIcosohedron()
    {
        initialVertices = new Vector3[]
        {
            //rect1
            new Vector3(1,gr,0).normalized,
            new Vector3(1,-gr,0).normalized,
            new Vector3(-1,-gr,0).normalized,
            new Vector3(-1,gr,0).normalized,
            //rect2
            new Vector3(gr,0,1).normalized,
            new Vector3(-gr,0,1).normalized,
            new Vector3(-gr,0,-1).normalized,
            new Vector3(gr,0,-1).normalized,
            //rect3
            new Vector3(0,1,gr).normalized,
            new Vector3(0,1,-gr).normalized,
            new Vector3(0,-1,-gr).normalized,
            new Vector3(0,-1,gr).normalized,
        };

        for (int i = 0; i < initialVertices.Length; i++)
        {
            initialVertices[i] = initialVertices[i] * radius;
        }

        verticesList = initialVertices.ToList();
        vertices = verticesList.ToArray();

        triangles = new int[]
        {
            0,9,3,
            0,3,8,
            0,8,4,
            0,4,7,
            0,7,9,

            1,11,2,
            1,2,10,
            1,10,7,
            1,7,4,
            1,4,11,

            2,11,5,
            2,5,6,
            2,6,10,

            3,5,8,
            3,9,6,
            3,6,5,

            4,8,11,
            8,5,11,
            9,7,10,
            9,10,6
        };
        trianglesList = triangles.ToList();
        DrawEdges();
        Vector3[] newVertices;
        int[] newTriangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {

        }
    }

    private void CreateShapeCube()
    {
        initialVertices = new Vector3[]
        {
            //rect1
            new Vector3(1,1,1),
            new Vector3(1,1,-1),
            new Vector3(1,-1,-1),
            new Vector3(1,-1,1),
            //rect2
            new Vector3(-1,1,1),
            new Vector3(-1,1,-1),
            new Vector3(-1,-1,-1),
            new Vector3(-1,-1,1),
        };
        for (int i = 0; i < initialVertices.Length; i++)
        {
            initialVertices[i] = initialVertices[i] * 5;
        }

        vertices = initialVertices;
        verticesList = vertices.ToList();


        triangles = new int[]
        {
            0,3,2,
            0,2,1,
            1,2,5,
            5,2,6,
            5,6,4,
            4,6,7,
            4,7,0,
            0,7,3,
            5,4,0,
            5,0,1,
            6,3,7,
            6,2,3
        };
        trianglesList = triangles.ToList();
    }

    private void CreatePlane()
    {
        initialVertices = new Vector3[]
        {
            //rect1
            new Vector3(1,1,-5),
            new Vector3(1,-1,-5),
            new Vector3(-1,-1,-5),
            new Vector3(-1,1,-5),
        };
        for (int i = 0; i < initialVertices.Length; i++)
        {
            initialVertices[i] = initialVertices[i] * 1;
        }

        vertices = initialVertices;
        verticesList = vertices.ToList();


        triangles = new int[]
        {
            0,1,2,
            0,2,3,
        };
        trianglesList = triangles.ToList();
    }

    private void DrawEdges()
    {
        for (int i = 0; i < triangles.Length - 2; i += 3)
        {
            Debug.DrawLine(vertices[triangles[i]], vertices[triangles[i + 1]], Color.black);
            Debug.DrawLine(vertices[triangles[i + 1]], vertices[triangles[i + 2]], Color.black);
            Debug.DrawLine(vertices[triangles[i + 2]], vertices[triangles[i]], Color.black);
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    // Update is called once per frame

    //below function is being called from camMove
    public void UpdateMeshOnMoveCam()
    {
        CheckDistances();
        UpdateMesh();
        DebugEdgeLines();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckDistances();
            UpdateMesh();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            MakeHeights();
            UpdateMesh();
            craters.UpdateCraters();
            craters.SpawnDebugCubes();
            //SpawnDebugCubes();
            Debug.Log(vertices.Length);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            MakeHeights();
            UpdateMesh();
            craters.SpawnDebugCubes();
            //SpawnDebugCubes();
            Debug.Log(vertices.Length);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            SpawnDebugTriangleCenters();
        }
        //MakeHeights();
        //UpdateMesh();
        DebugEdgeLines();
    }

    /*private void CheckDistances()
    {
        Vector3 camPoint = camProxy.transform.position;

        if (wt.Count == 0)
        {
            for (int i = 0; i < verticesList.Count; i++)
            {
                Debug.Log("1");
                int temp = CalcWtUsingDistFromVertex(vertices[i]);
                wt.Add(temp);
            }
        }


        breakableVertices = new List<int>();
        for(int i = 0; i < vertices.Length; i++)
        {
            Debug.Log("2");
            if (CalcWtUsingDistFromVertex(vertices[i]) < wt[i])
            {
                Debug.Log("3");
                wt[i] = CalcWtUsingDistFromVertex(vertices[i]);
                breakableVertices.Add(i);
            }
        }
        IterateVerticesToBreak();
        SpawnDebugCubes();
    }*/

    //O(onOfVertices)
    //selecting all the vertices that need to be broken
    //iterating through all vertices and deciding if they need to be broken
    private void CheckDistances()
    {
        Vector3 camPoint = camProxy.transform.position;

        //initial calc of wts
        if (wt.Count == 0)
        {
            for (int i = 0; i < verticesList.Count; i++)
            {
                int temp = CalcWtUsingDistFromVertex(vertices[i]);
                wt.Add(temp);
            }
        }

        //chaneg here

        for (int i = 0; i < vertices.Length; i++)
        {
            if (CalcWtUsingDistFromVertex(vertices[i]) > wt[i])
            {
                wt[i] = CalcWtUsingDistFromVertex(vertices[i]);
                AddVertexToBreakableVertices(i);
            }
            /*if(((int)Mathf.Ceil(Vector3.Distance(camPoint, vertices[i]))) < wt[i])
            {
                wt[i] = (int)Mathf.Ceil(Vector3.Distance(camPoint, vertices[i]));
                breakableVertices.Add(i);
            }*/
        }

        //use below function if u want to remake mesh every single time u calc wts
        IterateVerticesToBreak();
    }

    private void AddVertexToBreakableVertices(int index)
    {
        if (!breakableVertices.Contains(index))
        {
            breakableVertices.Add(index);
        }
    }

    //O(1)
    private int CalcWtUsingDistFromVertex(Vector3 vertexPosition)
    {
        //this is just distance
        float dist = Vector3.Distance(vertexPosition, camProxy.transform.position) / radius;

        //this is distance measured only in z direction
        //float dist = Vector3.Dot(vertexPosition - camProxy.transform.position, gameObject.transform.position - camProxy.transform.position);
        //dist /= Vector3.Distance(gameObject.transform.position, camProxy.transform.position);
        //dist /= radius;

        dist = 1 - Mathf.Clamp01(dist);
        //int wt = (int)Mathf.Ceil(distToWtCurve.Evaluate(dist) * breakingFrequency);
        int wt = (int)Mathf.Ceil(dist * maxNumberOfBreaks);
        return wt;
    }

    //O(noOfTriangles * noOfBreakableVerticecs)
    //iterating through each triangle * checking if each triangle has any vertices that need to be broken
    private void IterateVerticesToBreak()
    {
        int limit = trianglesList.Count - 2;
        for (int i = 0; i < limit; i += 3)
        {
            for (int j = 0; j < breakableVertices.Count; j++)
            {
                if (breakableVertices[j] == trianglesList[i])
                {
                    if (trianglesBreakenWtsList[i/3] + 1 <= wt[breakableVertices[j]])
                    {
                        //Debug.Log(i.ToString() + j.ToString() + '1');
                        BreakTriangle(i, wt[trianglesList[i]]);
                        i -= 3;
                        limit -= 3;
                        break;
                    }
                }
                else if (breakableVertices[j] == trianglesList[i + 1])
                {
                    if (trianglesBreakenWtsList[i/3] + 1 <= wt[breakableVertices[j]])
                    {
                        //Debug.Log(i.ToString() + j.ToString() + '1');
                        BreakTriangle(i, wt[trianglesList[i]]);
                        i -= 3;
                        limit -= 3;
                        break;
                    }
                }
                else if (breakableVertices[j] == trianglesList[i + 2])
                {
                    if (trianglesBreakenWtsList[i/3] + 1 <= wt[breakableVertices[j]])
                    {
                        //Debug.Log(i.ToString() + j.ToString() + '1');
                        BreakTriangle(i, wt[trianglesList[i]]);
                        i -= 3;
                        limit -= 3;
                        break;
                    }
                }
            }
            SpawnDebugCubes();
        }

        //we need to clear breakable vertices after breaking the vertices
        breakableVertices = new List<int>();
        //Debug.Log("end");
        //BreakTriangle(0);

        vertices = verticesList.ToArray();
        triangles = trianglesList.ToArray();
        trianglesBreakenWts = trianglesBreakenWtsList.ToArray();
        //mesh.RecalculateNormals();
        //DrawEdges();
    }

    //O(verticesList.Count)
    //iterating through vertices 
    //i am checking each new vertex, if its present in previous vertices list
    private void BreakTriangle(int index_in_triangles, int currentWt)
    {
        int a, b, c;
        a = trianglesList[index_in_triangles];
        b = trianglesList[index_in_triangles + 1];
        c = trianglesList[index_in_triangles + 2];
        int current_no_of_vertices = vertices.Length;

        Vector3 a_b = (vertices[a] + vertices[b]) / 2;
        Vector3 b_c = (vertices[b] + vertices[c]) / 2;
        Vector3 c_a = (vertices[c] + vertices[a]) / 2;

        int ab = -1;
        int bc = -1;
        int ca = -1;

        for (int i = 0; i < verticesList.Count; i++)
        {
            if (verticesList[i] == a_b)
            {
                ab = i;
            }
            if (verticesList[i] == b_c)
            {
                bc = i;
            }
            if (verticesList[i] == c_a)
            {
                ca = i;
            }
        }

        /*if (!(ab==-1 || bc==-1 || ca == -1))
        {
            return; 
            //returning if there are no new vertices
        }*/
        Vector3 camPoint = camProxy.transform.position;
        if (ab == -1)
        {
            verticesList.Add(a_b);
            wt.Add(CalcWtUsingDistFromVertex(a_b));
            ab = current_no_of_vertices;
            current_no_of_vertices++;
        }
        if(bc == -1)
        {
            verticesList.Add(b_c);
            wt.Add(CalcWtUsingDistFromVertex(b_c));
            bc = current_no_of_vertices;
            current_no_of_vertices++;
        }
        if (ca == -1)
        {
            verticesList.Add(c_a);
            wt.Add(CalcWtUsingDistFromVertex(c_a));
            ca = current_no_of_vertices;
            current_no_of_vertices++;
        }

        vertices = verticesList.ToArray();

        //removing old triangle from triangles
        trianglesList.RemoveAt(index_in_triangles); //time complexity O(1)
        trianglesList.RemoveAt(index_in_triangles);
        trianglesList.RemoveAt(index_in_triangles);
        int newBrokenNum = trianglesBreakenWtsList[(int)(index_in_triangles / 3)] + 1;
        trianglesBreakenWtsList.RemoveAt((int)(index_in_triangles / 3));

        //adding new 4 triangles into triangles
        List<int> newFaces = new List<int>() { 
            a,ab,ca,
            b,bc,ab,
            c,ca,bc,
            ab,bc,ca,
        };
        trianglesList.AddRange(newFaces);
        List<int> newBreakenWts = new List<int>()
        {
            newBrokenNum,newBrokenNum,newBrokenNum,newBrokenNum,
        };
        trianglesBreakenWtsList.AddRange(newBreakenWts);
    }

    //for changing the surface to spherical and apply craters
    private void MakeHeights()
    {
        heights = new float[vertices.Length];

        ComputeBuffer shaderVertices = new ComputeBuffer(vertices.Length, 3*sizeof(float));
        shaderVertices.SetData(vertices);
        ComputeBuffer shaderHeights = new ComputeBuffer(heights.Length, sizeof(float));
        shaderHeights.SetData(heights);
        ComputeBuffer shaderCraterCentres = new ComputeBuffer(craters.craterCentres.Length, 3 * sizeof(float));
        shaderCraterCentres.SetData(craters.craterCentres);
        ComputeBuffer shaderCraterRadii = new ComputeBuffer(craters.craterRadii.Length, sizeof(float));
        shaderCraterRadii.SetData(craters.craterRadii);
        ComputeBuffer shaderCraterDepth = new ComputeBuffer(craters.craterDepth.Length, sizeof(float));
        shaderCraterDepth.SetData(craters.craterDepth);

        planetHeightShader.SetBuffer(0, "vertices", shaderVertices);
        planetHeightShader.SetBuffer(0, "heights", shaderHeights);
        planetHeightShader.SetBuffer(0, "craterCentres", shaderCraterCentres);
        planetHeightShader.SetBuffer(0, "craterRadii", shaderCraterRadii);
        planetHeightShader.SetBuffer(0, "craterDepth", shaderCraterDepth);
        planetHeightShader.SetFloat("planetRadius", radius);
        planetHeightShader.SetInt("numCraters", craters.numCraters);
        planetHeightShader.SetInt("numVertices", vertices.Length);
        planetHeightShader.SetFloat("smoothness", craters.smoothness);
        planetHeightShader.SetFloat("rimWidth", craters.rimWidth);
        planetHeightShader.SetFloat("rimHeight", craters.rimHeight);

        planetHeightShader.Dispatch(0, vertices.Length / 10, 1, 1);

        //getting shaderData(shader heights) into c# heights
        shaderHeights.GetData(heights);

        shaderVertices.Dispose();
        shaderHeights.Dispose();
        shaderCraterCentres.Dispose();
        shaderCraterRadii.Dispose();
        shaderCraterDepth.Dispose();

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices[i].normalized * heights[i];
        }
        verticesList = vertices.ToList();
    }

    private void SpawnDebugCubes()
    {
        if (!spawnDebugCubes)
        {
            return;
        }
        GameObject[] deleteThem = GameObject.FindGameObjectsWithTag("debugCubes");
        for(int i = 0; i < deleteThem.Length; i++)
        {
            GameObject.Destroy(deleteThem[i]);
        }


        for(int i = 0; i < vertices.Length; i++)
        {
            GameObject cube = GameObject.Instantiate(prefab, vertices[i], Quaternion.identity);
            cube.GetComponent<number>().index = i;
            cube.GetComponent<number>().wt = wt[i];
        }
    }

    private void DebugEdgeLines()
    {
        for (int i = 0; i < triangles.Length - 2; i += 3)
        {
            Debug.DrawLine(vertices[triangles[i]], vertices[triangles[i + 1]], Color.black);
            Debug.DrawLine(vertices[triangles[i + 1]], vertices[triangles[i + 2]], Color.black);
            Debug.DrawLine(vertices[triangles[i + 2]], vertices[triangles[i]], Color.black);
        }
    }

    private void SpawnDebugTriangleCenters()
    {
        GameObject[] deleteThem = GameObject.FindGameObjectsWithTag("triCenter");
        for (int i = 0; i < deleteThem.Length; i++)
        {
            GameObject.Destroy(deleteThem[i]);
        }
        for (int i = 0; i < triangles.Length - 2; i += 3)
        {
            Vector3 spawnPosition = (vertices[triangles[i]] + vertices[triangles[i + 1]] + vertices[triangles[i + 2]]) / 3;
            GameObject temp = GameObject.Instantiate(prefabTriangleCenter, spawnPosition, Quaternion.identity);
            temp.GetComponent<triangleCenter>().triangleIndex = i;
        }
    }
}
