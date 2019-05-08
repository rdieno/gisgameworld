using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ShapeGrammarProcessor : MonoBehaviour
{
    [SerializeField]
    private GameObject meshObject = null;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    //public ExtrudedMeshTrail emt;

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = meshObject.GetComponent<MeshFilter>();
        meshRenderer = meshObject.GetComponent<MeshRenderer>();


        //meshFilter.mesh = CreatePlane(10, 10);

        /////////meshObject.transform.Rotate(180.0f, 0.0f, 0.0f);


        //CreateTestSquare();
        //LoadTestBuilding();

        //Mesh m = CreatePlane(10, 10);
        Mesh m = LoadTestBuilding();


        //m = ShapeGrammerOperations.ExtrudeMeshY(m, transform, 5.0f);

        //List<Mesh> splitMeshes = ShapeGrammerOperations.SplitX(m, transform, 0.5f);

        //Vector3[] verticesA = splitMeshes[0].vertices;
        //Vector3[] verticesB = splitMeshes[1].vertices;

        //for (int i = 0; i < splitMeshes[0].vertexCount; i++)
        //{
        //    verticesA[i] = new Vector3(verticesA[i].x + 20.0f, verticesA[i].y, verticesA[i].z);

        //}

        //for (int i = 0; i < splitMeshes[1].vertexCount; i++)
        //{
        //    verticesB[i] = new Vector3(verticesB[i].x - 20.0f, verticesB[i].y, verticesB[i].z);

        //}

        //splitMeshes[0].vertices = verticesA;
        //splitMeshes[1].vertices = verticesB;

        //m = BuildingUtility.CombineMeshes(splitMeshes);

        meshFilter.mesh = m;

        Material material = meshRenderer.materials[0];
        material.mainTexture = CreateTestTexture(10, 10);
        //m.RecalculateBounds();
        //m.RecalculateNormals();

        Vector3[] vertices = m.vertices;
        Vector3[] normals = m.normals;

        for (int i = 0; i < m.vertexCount; i++)
        {
            Vector3 currentVert = vertices[i];
            Vector3 currentNormal = normals[i];
            Debug.DrawLine(currentVert, currentVert + (currentNormal * 1.5f), Color.yellow, 1000.0f, false);
        }


        //List<Mesh> splitMeshes = ShapeGrammerOperations.SplitX(meshFilter.mesh, transform, 0.25f);
        //List<Mesh> moreSplitMeshes = ShapeGrammerOperations.SplitY(splitMeshes[0], transform, 0.5f);
        //splitMeshes.RemoveAt(0);

        //List<Mesh> evenMoreSplitMeshes = ShapeGrammerOperations.SplitY(moreSplitMeshes[0], transform, 0.5f);
        //moreSplitMeshes.RemoveAt(0);

        //splitMeshes.AddRange(moreSplitMeshes);
        //splitMeshes.AddRange(evenMoreSplitMeshes);

        //meshFilter.mesh = BuildingUtility.CombineMeshes(splitMeshes);


        // meshFilter.mesh = ShapeGrammerOperations.SplitY(meshFilter.mesh, transform, 0.5f);

        //emt.HasInitialized = true;
        //emt.Init();


    }

    void CreateTestSquare()
    {
        meshFilter.mesh = CreatePlane(10, 10);
    }

    Mesh LoadTestBuilding()
    {
        Building b = null;
        Mesh m = null;

        string appPath = Application.persistentDataPath;

        string folderPath = Path.Combine(appPath, "TestGeometry");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string dataPath = Path.Combine(folderPath, "test.bld");

        BinaryFormatter binaryFormatter = new BinaryFormatter();

        SurrogateSelector surrogateSelector = new SurrogateSelector();
        Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();

        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
        binaryFormatter.SurrogateSelector = surrogateSelector;

        using (FileStream fileStream = File.Open(dataPath, FileMode.Open))
        {
            b = (Building)binaryFormatter.Deserialize(fileStream);
        }

        if (b != null)
        {
            m = BuildingUtility.TrianglesToMesh(b.Geometry);
            Vector3[] vertices = m.vertices;

            Vector3 offset = m.bounds.center;

            for (int i = 0; i < m.vertexCount; i++)
            {
                vertices[i] = new Vector3(vertices[i].x - offset.x, vertices[i].y, vertices[i].z - offset.z);
            }

            m.vertices = vertices;

            m.RecalculateBounds();
            m.RecalculateNormals();

            return m;
            //return BuildingUtility.TrianglesToMesh(b.Geometry);

            //Material material = meshRenderer.materials[0];
            //material.mainTexture = CreateTestTexture(10, 10);
        }
        else
        {
            return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void Extrude()
    {

    }

    Mesh CreatePlane(float width, float depth)
    {
        float halfWidth = width / 2f;
        float halfDepth = depth / 2f;

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];
        Mesh mesh = new Mesh();

        // Corner verts
        vertices[0] = new Vector3(-halfWidth, 0, -halfDepth);
        vertices[1] = new Vector3(halfWidth, 0, -halfDepth);
        vertices[2] = new Vector3(-halfWidth, 0, halfDepth);
        vertices[3] = new Vector3(halfWidth, 0, halfDepth);

        //  Lower left triangle.
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;

        //  Upper right triangle.   
        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;

        normals[0] = Vector3.up;
        normals[1] = Vector3.up;
        normals[2] = Vector3.up;
        normals[3] = Vector3.up;

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        return mesh;
    }

    public Texture2D CreateTestTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        Color[] colors = new Color[2];
        colors[0] = Color.red;
        colors[1] = Color.green;

        Color[] texturePixelColors = new Color[width * height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color a = Color.Lerp(colors[0], colors[1], ((float)j / (float)height));
                texturePixelColors[i * width + j] = a;
            }
        }

        texture.SetPixels(texturePixelColors);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        return texture;
    }
}
