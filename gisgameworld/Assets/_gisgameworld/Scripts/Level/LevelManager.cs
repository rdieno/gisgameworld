using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;

public class LevelManager
{
    private GameManager manager;

    private DataManager dataManager;
    public DataManager DataManager
    {
        get { return dataManager; }
    }

    private GameObject level;
    private MeshFilter levelMeshFilter;
    private MeshRenderer levelMeshRenderer;
    private GameObject buildingPrefab;

    private Vector3 origin;
    private float conversionFactor;

    private Building currentBuilding;
    public Building CurrentBuilding
    {
        get => currentBuilding;
    }

    private Location currentLocation;
    public Location CurrentLocation
    {
        get => currentLocation;
        set => currentLocation = value;
    }


    public LevelManager(GameManager manager)
    {
        this.manager = manager;
        this.level = manager.Level;
        this.dataManager = manager.DataManager;
        this.levelMeshFilter = level.GetComponent<MeshFilter>();
        this.levelMeshRenderer = level.GetComponent<MeshRenderer>();

        this.levelMeshRenderer.material = Resources.Load("Materials/TestMaterial_Blank") as Material;
        buildingPrefab = this.manager.BuildingPrefab;
    }

    public void ProcessData(OSMData data, OSMInfo info)
    {
        Region bounds = info.bounds;

        float midLon = (bounds.topLeft.longitude + bounds.topright.longitude) / 2.0f;
        float midLat = (bounds.topLeft.latitude + bounds.bottomLeft.latitude) / 2.0f;

        conversionFactor = MercatorProjection.earthCircumferece(midLat);
        origin = info.origin;
        
        // process building footprints and convert to meshes
        List<Building> buildings = new List<Building>();

        for(int i = 0; i < data.elements.Count; i++)
        {
            OSMElement element = data.elements[i];

            // check if we have valid geomatry
            if (element.geometry == null)
            {
                continue;
            }
            else if (element.geometry.Count < 3)
            {
                continue;
            }

            // convert osm building footprint coords to a 2d polygon of vectors
            // also attempts to correct angles that are close to 180, 90 or 45 degrees
            List<Vector3> footprint = PrepareGeometry(element.geometry);

            Mesh mesh = Triangulator.TriangulatePolygon(footprint, Vector3.up);

            // determine origin
            mesh.RecalculateBounds();
            Vector3 origin = mesh.bounds.center;

            // determine local transform, y is pointing up
            LocalTransform transform = DetermineOrientation(origin, footprint);

            Shape rootShape = new Shape(mesh.vertices, mesh.normals, mesh.triangles, transform);
            Building building = new Building(footprint, i, rootShape);

            // create new building and store the mesh + original geometry
            buildings.Add(building);
        }

        // create level data object with the list of buildings
        dataManager.LevelData = new LevelData(buildings);
        
        // combine all meshes and add to mesh object's mesh filter
        CombineBuildingRoots();
    }

    // convert coords to vector data
    // remove last coord as it is a repeat of the first one (closed loop)
    // also rectifies angles that are near 180, 90, 135, 45
    private List<Vector3> PrepareGeometry(List<OSMCoordinate> osmGeometry, bool rectify = true)
    {
        List<Vector3> vertices = new List<Vector3>();

        for(int i = 0; i < osmGeometry.Count - 1; i++)
        {
            Vector3 vertex = convertCoordinateToVector(osmGeometry[i].lat, osmGeometry[i].lon);
            vertices.Add(vertex);
        }

        if(rectify)
        {
            // rectify angles
            vertices = BuildingUtility.Rectify(vertices, 10.0f, BuildingUtility.isPolygonXOriented(vertices));
        }

        return vertices;
    }

    public void CombineBuildingRoots()
    {
        List<Building> buildings = dataManager.LevelData.Buildings;
        CombineInstance[] combine = new CombineInstance[buildings.Count];
        int i = 0;
        while (i < buildings.Count)
        {
            Mesh mesh = new Mesh();
            Shape root = buildings[i].Root;

            mesh.vertices = root.Vertices;
            mesh.triangles = root.Triangles;
            mesh.normals = root.Normals;

            combine[i].mesh = mesh;
            combine[i].transform = Matrix4x4.zero;
            i++;
        }

        levelMeshFilter.mesh = new Mesh();
        levelMeshFilter.mesh.CombineMeshes(combine, true, false);
    }

    public void CombineBuildingMeshes()
    {
        List<Building> buildings = dataManager.LevelData.Buildings;
        CombineInstance[] combine = new CombineInstance[buildings.Count];
        int i = 0;
        while (i < buildings.Count)
        {
            Mesh mesh = new Mesh();

            combine[i].mesh = buildings[i].Mesh;
            combine[i].transform = Matrix4x4.zero;
            i++;
        }

        levelMeshFilter.mesh = new Mesh();
        levelMeshFilter.mesh.CombineMeshes(combine, true, false);
    }

    public void AddBuildingsToLevel(bool moveToOriginalLocation = false)
    {
        List<Building> buildings = dataManager.LevelData.Buildings;

        foreach(Building b in buildings)
        {
            GameObject building = null;

            if(moveToOriginalLocation)
            {
                building = UnityEngine.Object.Instantiate(buildingPrefab, b.OriginalPosition, Quaternion.identity, level.transform);
            }
            else
            {
                building = UnityEngine.Object.Instantiate(buildingPrefab, level.transform);
            }

            MeshFilter filter = building.GetComponent<MeshFilter>();
            filter.mesh = b.Mesh;

            if (b.Info != null)
            {
                UnityBuilding unityBuilding = building.GetComponent<UnityBuilding>();
                unityBuilding.SetValues(b.Info);
            }
        }
    }

    // combines many single triangle meshes into a single polygonal mesh
    private Mesh CombineTriangles(List<Mesh> triangles)
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

    private Mesh CreateLotRectangleFromBounds(OSMBounds bounds)
    {
        Vector3 max = convertCoordinateToVector(bounds.maxlat, bounds.maxlon);
        Vector3 min = convertCoordinateToVector(bounds.minlat, bounds.minlon);

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

    public Mesh CreatePlane(float width, float depth)
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
        vertices[1] = new Vector3(-halfWidth, 0, halfDepth);
        vertices[2] = new Vector3(halfWidth, 0, halfDepth); 
        vertices[3] = new Vector3(halfWidth, 0, -halfDepth);

        //  Lower left triangle.
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;

        //  Upper right triangle.   
        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        normals[0] = Vector3.up;
        normals[1] = Vector3.up;
        normals[2] = Vector3.up;
        normals[3] = Vector3.up;

        uv[0] = new Vector2(0, 1);
        uv[1] = new Vector2(0, 0);
        uv[2] = new Vector2(1, 0);
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

    private Vector3 convertCoordinateToVector(float lat, float lon)
    {
        float x = MercatorProjection.lonToX(lon) * this.conversionFactor - origin.x;
        float z = MercatorProjection.latToZ(lat) * this.conversionFactor - origin.z;

        x = Mathf.Round(x * 1000.0f) / 1000.0f;
        z = Mathf.Round(z * 1000.0f) / 1000.0f;

        return new Vector3(x, 0.0f, z);
    }

    public void ConstructLevelFromFile()
    {
        dataManager.LoadData();
        // combine all meshes and add to mesh object's mesh filter
        CombineBuildingRoots();
    }

    public LocalTransform DetermineOrientation(Vector3 origin, List<Vector3> footprint)
    {
        // pick random edge
        int randIndex = UnityEngine.Random.Range(0, footprint.Count);

        Vector3 p0 = footprint[randIndex];
        Vector3 p1 = footprint[MathUtility.ClampListIndex(randIndex + 1, footprint.Count)];

        float x = p1.x - p0.x;
        float z = p1.z - p0.z;

        Vector3 normal = new Vector3(z, origin.y, -x).normalized;
        
        return new LocalTransform(origin, Vector3.up, normal);
    }

    public void RetrieveBuilding(int index, bool moveToOrigin = false)
    {
        if (manager.DataManager.HasLoadedData)
        {
            Building building = this.DataManager.LevelData.Buildings[index];

            if (moveToOrigin)
            {
                Vector3[] vertices = building.Root.Vertices;

                Vector3 offset = building.Root.LocalTransform.Origin;

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new Vector3(vertices[i].x - offset.x, vertices[i].y, vertices[i].z - offset.z);
                }

                building.Root.LocalTransform.Origin = Vector3.zero;

                building.Root.Vertices = vertices;
            }

            currentBuilding = building;
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

    public void CreateTestSquare(float width = 10f, float depth = 10f)
    {
        Mesh plane = manager.LevelManager.CreatePlane(width, depth);
        LocalTransform lt = new LocalTransform(Vector3.zero, Vector3.up, Vector3.forward, Vector3.right);
        Shape s = new Shape(plane, lt);

        List<Vector3> footprint = plane.vertices.OfType<Vector3>().ToList();

        currentBuilding = new Building(footprint, -1, s);
        Mesh currentBuildingMesh = s.Mesh;

        currentBuildingMesh = manager.LevelManager.CreatePlane(10, 10);

        Material material = levelMeshRenderer.materials[0];
        material.mainTexture = manager.LevelManager.CreateTestTexture(10, 10);

        levelMeshFilter.mesh = currentBuildingMesh;
    }

    public void SetCurrentBuilding(Building building)
    {
        this.currentBuilding = building;

        levelMeshFilter.mesh = building.Mesh;
        levelMeshRenderer.materials = building.Materials;
    }

    public void ClassifyBuildings()
    {
        if(!dataManager.HasLoadedData)
        {
            Debug.Log("Level Manager: ClassifyBuildings(). Data has not been loaded yet. Couldn't classify.");
        }

        List<Building> buildings = dataManager.LevelData.Buildings;

        for(int i = 0; i < buildings.Count; i++)
        {
            Building b = buildings[i];
            Shape lot = b.Root;
            List<Vector3> footprint = b.Footprint;

            // determine

            // number of sides
            int sides = lot.Vertices.Length;

            if(sides > 0)
            {
                // dimensions
                Vector3 localDimensions = BuildingUtility.DetermineLocalDimensions(lot, lot.LocalTransform);

                // area
                float area = localDimensions.x * localDimensions.z;

                // isConvex
                bool isConvex = BuildingUtility.isConvexPolygon(b.Footprint);

                BuildingInfo info = new BuildingInfo(sides, localDimensions, area, isConvex, i);
                b.Info = info;
            }
            else
            {
                Debug.Log(i + ": " + sides + " sides, footprint: " + footprint.Count);
            }
        }
    }
}
