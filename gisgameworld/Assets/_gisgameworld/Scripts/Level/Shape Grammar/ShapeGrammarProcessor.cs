using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ShapeGrammarProcessor
{
    private GameManager manager;

    private DataManager dataManager;
    public DataManager DataManager
    {
        get { return dataManager; }
        //set { dataManager = value; }
    }

    private GameObject level;

    private MeshFilter levelMeshFilter;
    private MeshRenderer levelMeshRenderer;

    //public ExtrudedMeshTrail emt;

    private Building currentBuilding;
    private Mesh currentBuildingMesh;

    //private readonly Vector3[] referenceAngles = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back};
    //private readonly Vector3[] referenceDiagonals = { new Vector3(0.7f, 0.0f, 0.7f), new Vector3(0.7f, 0.0f, -0.7f), new Vector3(-0.7f, 0.0f, -0.7f), new Vector3(-0.7f, 0.0f, 0.7f) };


    //private readonly Vector3[] referenceAngles = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back, new Vector3(0.7f, 0.0f, 0.7f), new Vector3(0.7f, 0.0f, -0.7f), new Vector3(-0.7f, 0.0f, -0.7f), new Vector3(-0.7f, 0.0f, 0.7f) };

    public ShapeGrammarProcessor(GameManager manager)
    {
        this.manager = manager;
        this.dataManager = manager.DataManager;
        this.level = manager.Level;
        this.levelMeshFilter = level.GetComponent<MeshFilter>();
        this.levelMeshRenderer = level.GetComponent<MeshRenderer>();
        this.currentBuilding = null;
        this.currentBuildingMesh = null;
    }

    public void RetrieveBuilding(int index)
    {
        if(manager.DataManager.HasLoadedData)
        {
            currentBuilding = this.DataManager.LevelData.Buildings[index];

            // convert building to mesh
            currentBuildingMesh = BuildingUtility.BuildingToMesh(currentBuilding, true);
            levelMeshFilter.mesh = currentBuildingMesh;


            //currentBuildingMesh.RecalculateBounds();
            //currentBuildingMesh.RecalculateNormals();
        }
        else
        {
            Debug.Log("Shape Grammar Processor: could not find data in data manager");
            Debug.Log("Shape Grammar Processor: loading test plane");
            CreateTestSquare();
        }

        Material material = levelMeshRenderer.materials[0];
        material.mainTexture = manager.LevelManager.CreateTestTexture(10, 10);
    }

    public void RunSplitExample()
    {
        currentBuildingMesh = ShapeGrammerOperations.ExtrudeMeshY(currentBuildingMesh, level.transform, 5.0f);

        List<Mesh> splitMeshes = ShapeGrammerOperations.SplitX(currentBuildingMesh, level.transform, 0.5f);

        Vector3[] verticesA = splitMeshes[0].vertices;
        Vector3[] verticesB = splitMeshes[1].vertices;

        for (int i = 0; i < splitMeshes[0].vertexCount; i++)
        {
            verticesA[i] = new Vector3(verticesA[i].x + 5.0f, verticesA[i].y, verticesA[i].z);

        }

        for (int i = 0; i < splitMeshes[1].vertexCount; i++)
        {
            verticesB[i] = new Vector3(verticesB[i].x - 5.0f, verticesB[i].y, verticesB[i].z);

        }

        splitMeshes[0].vertices = verticesA;
        splitMeshes[1].vertices = verticesB;

        currentBuildingMesh = BuildingUtility.CombineMeshes(splitMeshes);

        levelMeshFilter.mesh = currentBuildingMesh;

        //Material material = levelMeshRenderer.materials[0];
        //material.mainTexture = CreateTestTexture(10, 10);
        //m.RecalculateBounds();
        //m.RecalculateNormals();

        //Vector3[] vertices = m.vertices;
        //Vector3[] normals = m.normals;

        //for (int i = 0; i < m.vertexCount; i++)
        //{
        //    Vector3 currentVert = vertices[i];
        //    Vector3 currentNormal = normals[i];
        //    Debug.DrawLine(currentVert, currentVert + (currentNormal * 1.5f), Color.yellow, 1000.0f, false);
        //}


        //List<Mesh> splitMeshes = ShapeGrammerOperations.SplitX(meshFilter.mesh, transform, 0.25f);
        //List<Mesh> moreSplitMeshes = ShapeGrammerOperations.SplitY(splitMeshes[0], transform, 0.5f);
        //splitMeshes.RemoveAt(0);

        //List<Mesh> evenMoreSplitMeshes = ShapeGrammerOperations.SplitY(moreSplitMeshes[0], transform, 0.5f);
        //moreSplitMeshes.RemoveAt(0);

        //splitMeshes.AddRange(moreSplitMeshes);
        //splitMeshes.AddRange(evenMoreSplitMeshes);

        //meshFilter.mesh = BuildingUtility.CombineMeshes(splitMeshes);


        //meshFilter.mesh = ShapeGrammerOperations.SplitY(meshFilter.mesh, transform, 0.5f);

        //emt.HasInitialized = true;
        //emt.Init();


    }

    // Start is called before the first frame update
    //void Start()
    //{
    //    levelMeshFilter = meshObject.GetComponent<MeshFilter>();
    //    levelMeshRenderer = meshObject.GetComponent<MeshRenderer>();

    //    currentBuilding = null;
    //    //meshFilter.mesh = CreatePlane(10, 10);

    //    /////////meshObject.transform.Rotate(180.0f, 0.0f, 0.0f);

    //    //CreateTestSquare();
    //    //LoadTestBuilding();

    //    //Mesh m = CreatePlane(10, 10);
    //    Mesh m = LoadTestBuilding();

    //    // choose orientation
    //    //bool isXOriented = true;
    //    //const float margin = 10.0f;

    //    //List<Vector3> edgeVertices = testBuilding.EdgeVertices;



    //    //edgeVertices = BuildingUtility.Rectify(edgeVertices, margin, isXOriented);

    //    //// int once = 0;
    //    //int f = 0;
    //    //int once = 0;

    //    //// choose orientation
    //    //bool isXOriented = true;
    //    //const float margin = 15.0f;
    //    ////const float tolerance = 0.5f;

    //    //// draw original polygon
    //    //for (int i = 1; i < edgeVertices.Count; i++)
    //    //{
    //    //    Debug.DrawLine(edgeVertices[i - 1], edgeVertices[i], Color.red, 1000.0f, false);

    //    //    if (i == edgeVertices.Count - 1) // last edge
    //    //    {
    //    //        Debug.DrawLine(edgeVertices[i], edgeVertices[0], Color.red, 1000.0f, false);
    //    //    }
    //    //}

    //    //// 180 pass
    //    //for (int i = 1; i < edgeVertices.Count + 1; i++)
    //    //{
    //    //    // save the indices of our chosen vertices so they can be overridden later
    //    //    // we use the modulo operater because for the last two angles we need to wrap around
    //    //    int index1 = (i - 1) % edgeVertices.Count;
    //    //    int index2 = i % edgeVertices.Count;
    //    //    int index3 = (i + 1) % edgeVertices.Count;

    //    //    // get the first three vertices in the polygon
    //    //    Vector3 vert1 = edgeVertices[index1];
    //    //    Vector3 vert2 = edgeVertices[index2];
    //    //    Vector3 vert3 = edgeVertices[index3];

    //    //    // calculate the normals pointing away from the middle vertex
    //    //    Vector3 norm1 = (vert1 - vert2).normalized;
    //    //    Vector3 norm2 = (vert3 - vert2).normalized;
    //    //    // calculate the angle between the two normals in degrees
    //    //    float angle = Mathf.Acos(Vector3.Dot(norm1, norm2)) * Mathf.Rad2Deg;


    //    //    // check for angles near 180 degrees
    //    //    if (angle < 180.0f + margin && angle > 180.0f - margin)
    //    //    {

    //    //        // remove the middle vertex, essentially joining the outer two with a straight line
    //    //        edgeVertices.RemoveAt(index2);
    //    //    }
    //    //}

    //    //// for debugging
    //    //CheckAllAngles(edgeVertices);

    //    //// 90 pass

    //    //Vector3 orientationAxis = Vector3.forward;

    //    //if (isXOriented)
    //    //{
    //    //    orientationAxis = Vector3.right;
    //    //}

    //    //int startIndex = 0;


    //    ////GameObject go = (GameObject)Instantiate(Resources.Load("PinkCube", typeof(GameObject)), edgeVertices[0], Quaternion.identity);
    //    ////go.transform.localScale = new Vector3(go.transform.localScale.x * 5.0f, go.transform.localScale.y * 5.0f, go.transform.localScale.z * 5.0f);

    //    //// check for reference angle
    //    //int? referenceVertexIndex = CheckForReferenceAngle(edgeVertices, orientationAxis);

    //    //if(referenceVertexIndex.HasValue)
    //    //{
    //    //    Debug.Log("found reference edge: " + referenceVertexIndex);

    //    //    //go = (GameObject)Instantiate(Resources.Load("YellowCube", typeof(GameObject)), edgeVertices[referenceVertexIndex.Value], Quaternion.identity);
    //    //    //go.transform.localScale = new Vector3(go.transform.localScale.x * 5.0f, go.transform.localScale.y * 5.0f, go.transform.localScale.z * 5.0f);

    //    //    // if an edge that matches the reference angle is found, start at that index
    //    //    startIndex = referenceVertexIndex.Value;
    //    //}


    //    //int edgeVerticesSize = edgeVertices.Count;


    //    ////int endIndex = startIndex;

    //    //for (int i = 0; i < edgeVerticesSize - 1; i++)
    //    //{
    //    //    int index0 = GetCircularIndex(startIndex, edgeVerticesSize);
    //    //    int index1 = GetCircularIndex(startIndex + 1, edgeVerticesSize);
    //    //    int index2 = GetCircularIndex(startIndex + 2, edgeVerticesSize);

    //    //    // get the first three vertices in the polygon
    //    //    Vector3 vert0 = edgeVertices[index0];
    //    //    Vector3 vert1 = edgeVertices[index1];
    //    //    Vector3 vert2 = edgeVertices[index2];

    //    //    // calculate the normals pointing away from the middle vertex
    //    //    Vector3 norm1 = (vert0 - vert1).normalized;
    //    //    Vector3 norm2 = (vert2 - vert1).normalized;

    //    //    // calculate the angle between the two normals in degrees
    //    //    float angle = Mathf.Acos(Vector3.Dot(norm1, norm2)) * Mathf.Rad2Deg;

    //    //    float targetAngle = 0.0f;

    //    //    //ignore angles that are already squared
    //    //    if (angle != 90.0f)
    //    //    {
    //    //        if (angle > 90.0f - margin && angle < 90.0f + margin)
    //    //        {
    //    //            targetAngle = 90.0f;

    //    //            float offsetAngle = Mathf.Abs(angle - targetAngle);

    //    //            if (angle < targetAngle)
    //    //            {
    //    //                offsetAngle *= -1.0f;
    //    //            }

    //    //            Vector3 oldEdge = vert2 - vert1;

    //    //            Vector3 newDirection = Vector3.RotateTowards(norm2, norm1, offsetAngle * Mathf.Deg2Rad, 0.0f);

    //    //            //Debug.DrawLine(vert1, vert1 + newDirection, Color.white, 1000.0f);

    //    //            angle = Mathf.Acos(Vector3.Dot(norm1, newDirection)) * Mathf.Rad2Deg;
    //    //            Debug.Log("corrected angle: " + angle);

    //    //            Vector3 projectedVector = Vector3.Project(oldEdge, newDirection);
    //    //            Vector3 newPoint = vert1 + projectedVector;
    //    //            //Debug.DrawLine(vert1, projectedVector, Color.white, 1000.0f);

    //    //            //GameObject go = (GameObject)Instantiate(Resources.Load("OrangeCube", typeof(GameObject)), newPoint, Quaternion.identity);
    //    //            //go.transform.localScale = new Vector3(go.transform.localScale.x * 5.0f, go.transform.localScale.y * 5.0f, go.transform.localScale.z * 5.0f);


    //    //            edgeVertices[index2] = newPoint;
    //    //        }
    //    //    }

    //    //    // increment the index
    //    //    startIndex = GetCircularIndex(startIndex + 1, edgeVerticesSize);
    //    //}

    //    //// 45/135 pass

    //    //startIndex = 0;

    //    //for (int i = 0; i < edgeVerticesSize - 1; i++)
    //    //{
    //    //    int index0 = GetCircularIndex(startIndex, edgeVerticesSize);
    //    //    int index1 = GetCircularIndex(startIndex + 1, edgeVerticesSize);
    //    //    int index2 = GetCircularIndex(startIndex + 2, edgeVerticesSize);

    //    //    // get the first three vertices in the polygon
    //    //    Vector3 vert0 = edgeVertices[index0];
    //    //    Vector3 vert1 = edgeVertices[index1];
    //    //    Vector3 vert2 = edgeVertices[index2];

    //    //    // calculate the normals pointing away from the middle vertex
    //    //    Vector3 norm1 = (vert0 - vert1).normalized;
    //    //    Vector3 norm2 = (vert2 - vert1).normalized;

    //    //    // calculate the angle between the two normals in degrees
    //    //    float angle = Mathf.Acos(Vector3.Dot(norm1, norm2)) * Mathf.Rad2Deg;

    //    //    float targetAngle = 0.0f;

    //    //    //ignore angles that are already squared or at a 45 degree angle
    //    //    if (angle != 45.0f && angle != 135.0f)
    //    //    {


    //    //        if (angle > 45.0f - margin && angle < 45.0f + margin)
    //    //    {
    //    //        targetAngle = 45.0f;

    //    //        float offsetAngle = Mathf.Abs(angle - targetAngle);

    //    //        if (angle < targetAngle)
    //    //        {
    //    //            offsetAngle *= -1.0f;
    //    //        }

    //    //        Vector3 oldEdge = vert2 - vert1;

    //    //        Vector3 newDirection = Vector3.RotateTowards(norm2, norm1, offsetAngle * Mathf.Deg2Rad, 0.0f);

    //    //        //Debug.DrawLine(vert1, vert1 + newDirection, Color.white, 1000.0f);

    //    //        angle = Mathf.Acos(Vector3.Dot(norm1, newDirection)) * Mathf.Rad2Deg;
    //    //        Debug.Log("corrected angle: " + angle);

    //    //        Vector3 projectedVector = Vector3.Project(oldEdge, newDirection);
    //    //        Vector3 newPoint = vert1 + projectedVector;
    //    //        //Debug.DrawLine(vert1, projectedVector, Color.white, 1000.0f);

    //    //        //GameObject go = (GameObject)Instantiate(Resources.Load("OrangeCube", typeof(GameObject)), newPoint, Quaternion.identity);
    //    //        //go.transform.localScale = new Vector3(go.transform.localScale.x * 5.0f, go.transform.localScale.y * 5.0f, go.transform.localScale.z * 5.0f);


    //    //        edgeVertices[index2] = newPoint;

    //    //    }
    //    //    else if (angle > 135.0f - margin && angle < 135.0f + margin)
    //    //    {
    //    //        targetAngle = 135.0f;

    //    //        float offsetAngle = Mathf.Abs(angle - targetAngle);

    //    //        if (angle < targetAngle)
    //    //        {
    //    //            offsetAngle *= -1.0f;
    //    //        }

    //    //        Vector3 oldEdge = vert2 - vert1;

    //    //        Vector3 newDirection = Vector3.RotateTowards(norm2, norm1, offsetAngle * Mathf.Deg2Rad, 0.0f);

    //    //        //Debug.DrawLine(vert1, vert1 + newDirection, Color.white, 1000.0f);

    //    //        angle = Mathf.Acos(Vector3.Dot(norm1, newDirection)) * Mathf.Rad2Deg;
    //    //        Debug.Log("corrected angle: " + angle);

    //    //        Vector3 projectedVector = Vector3.Project(oldEdge, newDirection);
    //    //        Vector3 newPoint = vert1 + projectedVector;
    //    //        //Debug.DrawLine(vert1, projectedVector, Color.white, 1000.0f);

    //    //        //GameObject go = (GameObject)Instantiate(Resources.Load("OrangeCube", typeof(GameObject)), newPoint, Quaternion.identity);
    //    //        //go.transform.localScale = new Vector3(go.transform.localScale.x * 5.0f, go.transform.localScale.y * 5.0f, go.transform.localScale.z * 5.0f);

    //    //        edgeVertices[index2] = newPoint;

    //    //    }
    //    //                    }

    //    //    // increment the index
    //    //    startIndex = GetCircularIndex(startIndex + 1, edgeVerticesSize);
    //    //}

    //    //// draw modified polygon
    //    //for (int i = 1; i < edgeVertices.Count; i++)
    //    //{
    //    //    Debug.DrawLine(edgeVertices[i - 1], edgeVertices[i], Color.green, 1000.0f, false);


    //    //    if (i == edgeVertices.Count - 1) // last edge
    //    //    {
    //    //        Debug.DrawLine(edgeVertices[i], edgeVertices[0], Color.green, 1000.0f, false);
    //    //    }
    //    //}

    //    //// for debugging
    //    //CheckAllAngles(edgeVertices);

    //    #region splitting example


    //    m = ShapeGrammerOperations.ExtrudeMeshY(m, transform, 5.0f);

    //    //List<Mesh> splitMeshes = ShapeGrammerOperations.SplitX(m, transform, 0.5f);

    //    //Vector3[] verticesA = splitMeshes[0].vertices;
    //    //Vector3[] verticesB = splitMeshes[1].vertices;

    //    //for (int i = 0; i < splitMeshes[0].vertexCount; i++)
    //    //{
    //    //    verticesA[i] = new Vector3(verticesA[i].x + 20.0f, verticesA[i].y, verticesA[i].z);

    //    //}

    //    //for (int i = 0; i < splitMeshes[1].vertexCount; i++)
    //    //{
    //    //    verticesB[i] = new Vector3(verticesB[i].x - 20.0f, verticesB[i].y, verticesB[i].z);

    //    //}

    //    //splitMeshes[0].vertices = verticesA;
    //    //splitMeshes[1].vertices = verticesB;

    //    //m = BuildingUtility.CombineMeshes(splitMeshes);

    //    levelMeshFilter.mesh = m;

    //    Material material = levelMeshRenderer.materials[0];
    //    material.mainTexture = CreateTestTexture(10, 10);
    //    m.RecalculateBounds();
    //    m.RecalculateNormals();

    //    //Vector3[] vertices = m.vertices;
    //    //Vector3[] normals = m.normals;

    //    //for (int i = 0; i < m.vertexCount; i++)
    //    //{
    //    //    Vector3 currentVert = vertices[i];
    //    //    Vector3 currentNormal = normals[i];
    //    //    Debug.DrawLine(currentVert, currentVert + (currentNormal * 1.5f), Color.yellow, 1000.0f, false);
    //    //}


    //    //List<Mesh> splitMeshes = ShapeGrammerOperations.SplitX(meshFilter.mesh, transform, 0.25f);
    //    //List<Mesh> moreSplitMeshes = ShapeGrammerOperations.SplitY(splitMeshes[0], transform, 0.5f);
    //    //splitMeshes.RemoveAt(0);

    //    //List<Mesh> evenMoreSplitMeshes = ShapeGrammerOperations.SplitY(moreSplitMeshes[0], transform, 0.5f);
    //    //moreSplitMeshes.RemoveAt(0);

    //    //splitMeshes.AddRange(moreSplitMeshes);
    //    //splitMeshes.AddRange(evenMoreSplitMeshes);

    //    //meshFilter.mesh = BuildingUtility.CombineMeshes(splitMeshes);


    //    //meshFilter.mesh = ShapeGrammerOperations.SplitY(meshFilter.mesh, transform, 0.5f);

    //    //emt.HasInitialized = true;
    //    //emt.Init();

    //    #endregion
    //}

    void CreateTestSquare()
    {
        currentBuildingMesh = manager.LevelManager.CreatePlane(10, 10);
        levelMeshFilter.mesh = currentBuildingMesh;
    }

    //Mesh LoadTestBuilding()
    //{
    //    Building b = null;
    //    Mesh m = null;

    //    string appPath = Application.persistentDataPath;

    //    string folderPath = Path.Combine(appPath, "TestGeometry");
    //    if (!Directory.Exists(folderPath))
    //        Directory.CreateDirectory(folderPath);

    //    string dataPath = Path.Combine(folderPath, "5.bld");

    //    BinaryFormatter binaryFormatter = new BinaryFormatter();

    //    SurrogateSelector surrogateSelector = new SurrogateSelector();
    //    Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();

    //    surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
    //    binaryFormatter.SurrogateSelector = surrogateSelector;

    //    using (FileStream fileStream = File.Open(dataPath, FileMode.Open))
    //    {
    //        b = (Building)binaryFormatter.Deserialize(fileStream);
    //    }

    //    if (b != null)
    //    {
    //        bool isXOriented = true;

    //        //if (m.bounds.size.z > m.bounds.size.x)
    //        //{
    //        //    isXOriented = false;
    //        //}

    //        List<Vector3> correctedPolygon = BuildingUtility.Rectify(b.Polygon, 10.0f, isXOriented);

    //        // triangulate the polygon
    //        List<Triangle> geometry = Triangulator.TriangulatePolygon(correctedPolygon);


    //        m = BuildingUtility.TrianglesToMesh(b.Geometry);
    //        Vector3[] vertices = m.vertices;

    //        Vector3 offset = m.bounds.center;

    //        for (int i = 0; i < m.vertexCount; i++)
    //        {
    //            vertices[i] = new Vector3(vertices[i].x - offset.x, vertices[i].y, vertices[i].z - offset.z);
    //        }

    //        List<Vector3> edgeVertices = b.Polygon;

    //        for (int i = 0; i < edgeVertices.Count; i++)
    //        {
    //            edgeVertices[i] = new Vector3(edgeVertices[i].x - offset.x, edgeVertices[i].y, edgeVertices[i].z - offset.z);
    //        }


    //        m.vertices = vertices;

    //        m.RecalculateBounds();
    //        m.RecalculateNormals();

    //       // testBuilding = b;

    //        return m;
    //        //return BuildingUtility.TrianglesToMesh(b.Geometry);

    //        //Material material = meshRenderer.materials[0];
    //        //material.mainTexture = CreateTestTexture(10, 10);
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}

    //Mesh CreatePlane(float width, float depth)
    //{
    //    float halfWidth = width / 2f;
    //    float halfDepth = depth / 2f;

    //    Vector3[] vertices = new Vector3[4];
    //    Vector3[] normals = new Vector3[4];
    //    Vector2[] uv = new Vector2[4];
    //    int[] triangles = new int[6];
    //    Mesh mesh = new Mesh();

    //    // Corner verts
    //    vertices[0] = new Vector3(-halfWidth, 0, -halfDepth);
    //    vertices[1] = new Vector3(halfWidth, 0, -halfDepth);
    //    vertices[2] = new Vector3(-halfWidth, 0, halfDepth);
    //    vertices[3] = new Vector3(halfWidth, 0, halfDepth);

    //    //  Lower left triangle.
    //    triangles[0] = 0;
    //    triangles[1] = 2;
    //    triangles[2] = 1;

    //    //  Upper right triangle.   
    //    triangles[3] = 2;
    //    triangles[4] = 3;
    //    triangles[5] = 1;

    //    normals[0] = Vector3.up;
    //    normals[1] = Vector3.up;
    //    normals[2] = Vector3.up;
    //    normals[3] = Vector3.up;

    //    uv[0] = new Vector2(0, 0);
    //    uv[1] = new Vector2(1, 0);
    //    uv[2] = new Vector2(0, 1);
    //    uv[3] = new Vector2(1, 1);

    //    mesh.vertices = vertices;
    //    mesh.triangles = triangles;
    //    mesh.normals = normals;
    //    mesh.uv = uv;

    //    return mesh;
    //}

    //public Texture2D CreateTestTexture(int width, int height)
    //{
    //    Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

    //    Color[] colors = new Color[2];
    //    colors[0] = Color.red;
    //    colors[1] = Color.green;

    //    Color[] texturePixelColors = new Color[width * height];

    //    for (int i = 0; i < width; i++)
    //    {
    //        for (int j = 0; j < height; j++)
    //        {
    //            Color a = Color.Lerp(colors[0], colors[1], ((float)j / (float)height));
    //            texturePixelColors[i * width + j] = a;
    //        }
    //    }

    //    texture.SetPixels(texturePixelColors);
    //    texture.wrapMode = TextureWrapMode.Clamp;
    //    texture.Apply();

    //    return texture;
    //}



    //Vector3 ClosestDirection(Vector3 v)
    //{
    //    float maxDot = -Mathf.Infinity;
    //    Vector3 ret = Vector3.zero;

    //    foreach (Vector3 dir in referenceAngles)
    //    {
    //        float t = Vector3.Dot(v, dir);
    //        if (t > maxDot)
    //        {
    //            ret = dir;
    //            maxDot = t;
    //        }
    //    }

    //    return ret;
    //}

    //Vector3 ClosestDiagonal(Vector3 v)
    //{
    //    float maxDot = -Mathf.Infinity;
    //    Vector3 ret = Vector3.zero;

    //    foreach(Vector3 dir in referenceDiagonals)
    //    {
    //        float t = Vector3.Dot(v, dir);
    //        if(t > maxDot)
    //        {
    //            ret = dir;
    //            maxDot = t;
    //        }
    //    }
    //}

    //private int GetCircularIndex(int index, int arraySize)
    //{
    //    if(index < 0)
    //    {
    //        return arraySize + index;
    //    }
    //    else
    //    {
    //        return index % arraySize;
    //    }
    //}


    //private void CheckAllAngles(List<Vector3> edgeVertices)
    //{
    //    Debug.Log("Checking Angles");

    //    string s = "";

    //    for (int i = 1; i < edgeVertices.Count + 1; i++)
    //    {
    //        // save the indices of our chosen vertices so they can be overridden later
    //        // we use the modulo operater because for the last two angles we need to wrap around
    //        int edgeVerticesSize = edgeVertices.Count;

    //        int index0 = GetCircularIndex(i - 2, edgeVerticesSize);
    //        int index1 = GetCircularIndex(i - 1, edgeVerticesSize);
    //        int index2 = GetCircularIndex(i, edgeVerticesSize);
    //        int index3 = GetCircularIndex(i + 1, edgeVerticesSize);
    //        int index4 = GetCircularIndex(i + 2, edgeVerticesSize);

    //        // get the first three vertices in the polygon
    //        Vector3 vert1 = edgeVertices[index1];
    //        Vector3 vert2 = edgeVertices[index2];
    //        Vector3 vert3 = edgeVertices[index3];

    //        // calculate the normals pointing away from the middle vertex
    //        Vector3 norm1 = (vert1 - vert2).normalized;
    //        Vector3 norm2 = (vert3 - vert2).normalized;

    //        // calculate the angle between the two normals in degrees
    //        float angle = Mathf.Acos(Vector3.Dot(norm1, norm2)) * Mathf.Rad2Deg;

    //        s += angle;
    //        s += " ";


    //    }

    //    Debug.Log(s);
    //}


    //// checks if there is an edge that lines up with the reference angle, otherwise returns null
    //// returns index of first vertex of matching edge
    //private int? CheckForReferenceAngle(List<Vector3> edgeVertices, Vector3 referenceNormal)
    //{
    //    const float tolerance = 0.1f;

    //    float minDot = Mathf.Infinity;
    //    int returnIndex = 0;

    //    for (int i = 0; i < edgeVertices.Count; i++)
    //    {
    //        // save the indices of our chosen vertices so they can be overridden later
    //        // we use the modulo operater because for the last two angles we need to wrap around
    //        int edgeVerticesSize = edgeVertices.Count;

    //        int index0 = GetCircularIndex(i, edgeVerticesSize);
    //        int index1 = GetCircularIndex(i + 1, edgeVerticesSize);

    //        // get the first three vertices in the polygon
    //        Vector3 vert0 = edgeVertices[index0];
    //        Vector3 vert1 = edgeVertices[index1];

    //        // calculate the normals pointing away from the middle vertex
    //        Vector3 edgeNormal = (vert1 - vert0).normalized;

    //        // check the dot product between the edge and the reference edge
    //        float dot = Mathf.Abs(Vector3.Dot(edgeNormal, referenceNormal));
            
    //        if(dot < minDot)
    //        {
    //            minDot = dot;
    //            returnIndex = i;
    //        }
    //    }

    //    // if the edge is almost perpendicular return the index
    //    if (minDot <= tolerance)
    //    {
    //        //Debug.Log("min dot: " + minDot);
    //        return returnIndex;
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}
}
