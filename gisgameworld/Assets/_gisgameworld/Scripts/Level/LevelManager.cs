using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private Color gridColor = Color.black;
    [SerializeField]
    private bool rotateCamera = true;
    [SerializeField]
    private float cameraRotationSpeed = 15.0f;
    [SerializeField]
    private GameObject meshObject = null;

    private new Camera camera;

    private MeshFilter levelMeshFilter;
    private MeshRenderer levelMeshRenderer;

   // private LevelData levelData;

    private Vector3 origin;
    private float conversionFactor;

    private LevelData levelData;


    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;

        levelMeshFilter = meshObject.GetComponent<MeshFilter>();
        levelMeshRenderer = meshObject.GetComponent<MeshRenderer>();

       // levelMeshFilter.mesh = CreatePlane(10f, 10f);

        //Material material = levelMeshRenderer.materials[0];
        //material.mainTexture = CreateTestTexture(10, 10);

        //levelData = new LevelData();
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        if(rotateCamera)
        {
            camera.transform.RotateAround(Vector3.zero, Vector3.up, cameraRotationSpeed * dt);
        }
    }
    public void ProcessData(OSMData data, Box bounds)
    {
        float midLon = (bounds.topLeft.longitude + bounds.topright.longitude) / 2.0f;
        float midLat = (bounds.topLeft.latitude + bounds.bottomLeft.latitude) / 2.0f;

        calculateOrigin(midLat, midLon);

        //// creates simple rectangular planes out of the building bounds
        //List<Building> buildings = new List<Building>();

        //// process each element
        //foreach (OSMElement element in data.elements)
        //{
        //    if(element.bounds != null)
        //    {
        //        // convert bounds to plane consisting of two triangles
        //        Mesh lotRectangle = CreateLotRectangleFromBounds(element.bounds);

        //        // add it as a new building object to the list of buildings
        //        buildings.Add(new Building(lotRectangle));
        //    }
        //}

        //// create level data object with the list of buildings
        //levelData = new LevelData(buildings);

        //// combine all meshes and add to mesh object's mesh filter
        //CombineBuildingMeshes();


        // process building footprints and convert to meshes
        List<Building> buildings = new List<Building>();

        // process each element
        foreach (OSMElement element in data.elements)
        {
            // check if we have valid geomatry
            if(element.geometry == null)
            {
                continue;
            }

            // convert osm building footprint coords to a polygon made out of vectors
            List<Vector3> polygon = PrepareGeometry(element.geometry);

            // triangulate the polygon
            List<Triangle> geometry = Triangulator.TriangulatePolygon(polygon);

            // check to make sure the polygon is valid
            if(geometry == null)
            {
                continue;
            }

            // turn trianlges into a mesh
            List<Mesh> triangleMeshes = new List<Mesh>();

            for(int i = 0; i < geometry.Count; i++)
            {
                Mesh m = new Mesh();

                Triangle t = geometry[i];

                Vector3[] vertices = new Vector3[3];
                int[] triangles = new int[3];
                Vector2[] uv = new Vector2[3];
                Vector3[] normals = new Vector3[3];

                vertices[0] = t.v1.position;
                vertices[1] = t.v2.position;
                vertices[2] = t.v3.position;

                triangles[0] = 0;
                triangles[1] = 1;
                triangles[2] = 2;

                uv[0] = new Vector2(0, 0);
                uv[1] = new Vector2(0, 1);
                uv[2] = new Vector2(1, 0);

                normals[0] = Vector3.up;
                normals[1] = Vector3.up;
                normals[2] = Vector3.up;

                m.vertices = vertices;
                m.triangles = triangles;
                m.uv = uv;
                m.normals = normals;

                triangleMeshes.Add(m);
            }

            // combine triangle meshes into a single polygon
            Mesh polygonMesh = CombineTriangles(triangleMeshes);

            // create new building and store the mesh + original geometry
            buildings.Add(new Building(polygonMesh, geometry));

            //levelmeshfilter.mesh = m;
            //levelmeshfilter.mesh.combinemeshes(combine, true, false);

            //material material = levelmeshrenderer.materials[0];
            //material.maintexture = createtesttexture(10, 10);
        }

        // create level data object with the list of buildings
        levelData = new LevelData(buildings);


        //levelMeshFilter.mesh = levelData.Buildings[0].Mesh;

       // levelMeshFilter.mesh = new Mesh();

        //levelMeshFilter.mesh = CreatePlane(10f, 10f);

        //Material material = levelMeshRenderer.materials[0];
        //material.mainTexture = CreateTestTexture(10, 10);

        // combine all meshes and add to mesh object's mesh filter
        CombineBuildingMeshes();
    }

    // convert coords to vector data
    // remove last coord as it is a repeat of the first one (closed loop)
    List<Vector3> PrepareGeometry(List<OSMCoordinate> osmGeometry)
    {
        try
        {
            int i = osmGeometry.Count;
        }
        catch (NullReferenceException e)
        {
            //int p = 0;
        }


        List<Vector3> vertices = new List<Vector3>();

        for(int i = 0; i < osmGeometry.Count - 1; i++)
        {
            try
            {
                OSMCoordinate o = osmGeometry[i];
            }
            catch (NullReferenceException e)
            {
                //int p = 0;
            }

            Vector3 vertex = convertCoordinateToVector(osmGeometry[i].lat, osmGeometry[i].lon);
            vertices.Add(vertex);
        }

        return vertices;
    }

    void CombineBuildingMeshes()
    {
        List<Building> buildings = levelData.Buildings;
        CombineInstance[] combine = new CombineInstance[buildings.Count];
        int i = 0;
        while (i < buildings.Count)
        {
            combine[i].mesh = buildings[i].Mesh;
            combine[i].transform = Matrix4x4.zero;
            i++;
        }

        levelMeshFilter.mesh = new Mesh();
        levelMeshFilter.mesh.CombineMeshes(combine, true, false);

        Material material = levelMeshRenderer.materials[0];
        material.mainTexture = CreateTestTexture(10, 10);
    }

    // combines many single triangle meshes into a single polygonal mesh
    Mesh CombineTriangles(List<Mesh> triangles)
    {
        CombineInstance[] combine = new CombineInstance[triangles.Count];
        int i = 0;
        while (i < triangles.Count)
        {
            combine[i].mesh = triangles[i];
            combine[i].transform = Matrix4x4.zero;
            i++;
        }

        Mesh polygon = new Mesh();
        polygon.CombineMeshes(combine, true, false);

        return polygon;
    }

    Mesh CreateLotRectangleFromBounds(OSMBounds bounds)
    {
        Vector3 max = convertCoordinateToVector(bounds.maxlat, bounds.maxlon);
        Vector3 min = convertCoordinateToVector(bounds.minlat, bounds.minlon);

        //Vector3 newCoord = convertCoordinateToVector(testCoord.lat, testCoord.lon);

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];
        Mesh mesh = new Mesh();

        // Corner verts
        vertices[0] = new Vector3(min.x, 0, min.z);
        vertices[1] = new Vector3(max.x, 0, min.z);
        vertices[2] = new Vector3(min.x, 0, max.z);
        vertices[3] = new Vector3(max.x, 0, max.z);

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
                Color a = Color.Lerp(colors[0], colors[1], ((float) j / (float) height));
                texturePixelColors[i * width + j] = a;
            }
        }

        texture.SetPixels(texturePixelColors);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        return texture;
    }

    private void calculateOrigin(float lat, float lon)
    {
        conversionFactor = MercatorProjection.earthCircumferece(lat);
        origin = new Vector3(MercatorProjection.lonToX(lon) * conversionFactor, 0, MercatorProjection.latToZ(lat) * conversionFactor);
    }

    private Vector3 convertCoordinateToVector(float lat, float lon)
    {
        float x = MercatorProjection.lonToX(lon) * conversionFactor - origin.x;
        float z = MercatorProjection.latToZ(lat) * conversionFactor - origin.z;

        x = Mathf.Round(x * 1000.0f) / 1000.0f;
        z = Mathf.Round(z * 1000.0f) / 1000.0f;

        return new Vector3(x, 0.0f, z);
    }
}
