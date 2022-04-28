using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapeGrammarProcessor
{
    private GameManager manager;

    private DataManager dataManager;
    public DataManager DataManager
    {
        get { return dataManager; }
    }


    private TestManager testManager;

    private GameObject level;

    private MeshFilter levelMeshFilter;
    private MeshRenderer levelMeshRenderer;


    private Building currentBuilding;
    private Mesh currentBuildingMesh;


    private ShapeGrammarParser sgParser;

    private ShapeGrammarDatabase sgDatabase;

    public ShapeGrammarProcessor(GameManager manager)
    {
        this.manager = manager;
        this.dataManager = manager.DataManager;
        this.sgParser = manager.SGParser;
        this.level = manager.Level;
        this.levelMeshFilter = level.GetComponent<MeshFilter>();
        this.levelMeshRenderer = level.GetComponent<MeshRenderer>();
        this.currentBuilding = null;
        this.currentBuildingMesh = null;
        this.sgDatabase = manager.SGDatabase;
        this.testManager = manager.TestManager;
    }

    public void CreateTestSquare(float width = 10f, float depth = 10f)
    {
        Mesh plane = manager.LevelManager.CreatePlane(width, depth);
        LocalTransform lt = new LocalTransform(Vector3.zero, Vector3.up, Vector3.forward, Vector3.right);
        Shape s = new Shape(plane, lt);

        List<Vector3> footprint = plane.vertices.OfType<Vector3>().ToList();

        currentBuilding = new Building(footprint, -1, s);
        currentBuildingMesh = s.Mesh;

        currentBuildingMesh = manager.LevelManager.CreatePlane(10, 10);

        Material material = levelMeshRenderer.materials[0];
        material.mainTexture = manager.LevelManager.CreateTestTexture(10, 10);

        levelMeshFilter.mesh = currentBuildingMesh;
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


    public Building RetrieveBuilding2(int index, bool moveToOrigin = false)
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

            return building;
        }
        else
        {
            Debug.Log("Shape Grammar Processor: could not find data in data manager");
            Debug.Log("Shape Grammar Processor: loading test plane");
            CreateTestSquare();

            return null;
        }

        Material material = levelMeshRenderer.materials[0];
        material.mainTexture = manager.LevelManager.CreateTestTexture(10, 10);
    }

    public IEnumerator RunOperationSandboxTest()
    {
        if(!dataManager.HasLoadedData)
        {
            yield return null;
        }

        List<Shape> currentShapeList = new List<Shape>();

        currentBuilding = RetrieveBuilding2(1, true);

        Shape lot = currentBuilding.Root;

        Dictionary<string, string> offsetNames = new Dictionary<string, string>
        {
            { "Inside", "a" },
            { "Border", "b" },
        };


        List<Mesh> meshes = new List<Mesh>();

        IShapeGrammarOperation taperOperation = new TaperOperation(10.0f, 15.0f);
        ShapeWrapper taperOutput = taperOperation.PerformOperation(new List<Shape>() { lot });
        currentShapeList.AddRange(taperOutput.shapeList);

        IShapeGrammarOperation scaleOperation = new ScaleOperation(new Vector3(1.2f, 2.0f, 1.5f));
        ShapeWrapper scaleOutput = scaleOperation.PerformOperation(taperOutput.shapeList);

        currentBuilding.Mesh = BuildingUtility.CombineShapes(currentShapeList, false);

        //  draw normals
        if (true)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
        };
        levelMeshRenderer.materials = mats;

        yield return null;
    }

    public void CompLocalTranformFixTest()
    {
        List<Mesh> meshes = new List<Mesh>();

        //dataManager.LoadData();
        //RetrieveBuilding(1, true);
        //RetrieveBuilding(1);
        CreateTestSquare();

        Shape lot = currentBuilding.Root;

       

        IShapeGrammarOperation eo = new ExtrudeOperation(Axis.Up, 10.0f);

        ShapeWrapper shapeWrapper = eo.PerformOperation(new List<Shape>() { lot });

        Dictionary<string, string> compNames = new Dictionary<string, string>
        {
            { "Front", "a" },
            { "Back", "b" },
            { "Left", "c" },
            { "Right", "d" },
            { "Top", "e" },
            { "Bottom", "f" },
        };

        IShapeGrammarOperation co = new CompOperation(compNames);

        List<Shape> shapes = shapeWrapper.shapeList;

        //shapes[0].Debug_DrawOrientation(100.0f);

        shapeWrapper = null;
        shapeWrapper = co.PerformOperation(shapes);

        Dictionary<string, List<Shape>> shapes2 = shapeWrapper.shapeDictionary;


        foreach(KeyValuePair< string, List<Shape>> s in shapes2)
        {
            s.Value[0].Debug_DrawOrientation(25f);
        }

        //Shape extruded = ExtrudeOperation.ExtrudeNormal(lot, 10.0f, lot.LocalTransform.Up);


        //currentBuilding.UpdateProcessedBuilding(shapes2);

        //currentBuilding.Mesh = roofShed.Mesh;
        //currentBuilding.Mesh = BuildingUtility.CombineMeshes(meshes, true);
        //currentBuilding.Mesh = BuildingUtility.CombineMeshes(meshes);

        if (false)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
        };
        levelMeshRenderer.materials = mats;
    }

    public void SplitOffsetFixTest()
    {
        List<Mesh> meshes = new List<Mesh>();

        CreateTestSquare();

        Shape lot = currentBuilding.Root;

        Dictionary<string, string> offsetNames = new Dictionary<string, string>
        {
            { "Inside", "a" },
            { "Border", "b" },
        };

        IShapeGrammarOperation oo = new OffsetOperation(-1.1f, offsetNames);

        IShapeGrammarOperation eo = new ExtrudeOperation(Axis.Up, 10.0f);

        ShapeWrapper shapeWrapper = oo.PerformOperation(new List<Shape>() { lot });

        Dictionary<string, List<Shape>> dict = shapeWrapper.shapeDictionary;


        shapeWrapper = eo.PerformOperation(dict["b"]);

        List<Shape> offsetBorder = shapeWrapper.shapeList;

        SplitTerm a = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.10f, "a") });
        SplitTerm b = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.8f, "b") });
        SplitTerm c = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.10f, "c") });

        IShapeGrammarOperation so = new SplitOperation(Axis.Right, new List<SplitTerm>() { a, b, c });

        shapeWrapper = so.PerformOperation(offsetBorder);


        Dictionary<string, List<Shape>> dict2 = shapeWrapper.shapeDictionary;

        IShapeGrammarOperation to = new TranslateOperation(new Vector3(0, 12f, 0), CoordSystem.Local);

        shapeWrapper = to.PerformOperation(dict2["b"]);

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
        };
        levelMeshRenderer.materials = mats;
    }

    public void RotateNormalsFixTest()
    {
        List<Mesh> meshes = new List<Mesh>();

        CreateTestSquare();

        Shape lot = currentBuilding.Root;


        IShapeGrammarOperation ro = new RotateOperation(new Vector3(180f, 0f, 0f), CoordSystem.Local);

        ShapeWrapper shapeWrapper = ro.PerformOperation(new List<Shape>() { lot });


        foreach (Shape s in shapeWrapper.shapeList)
        {
            s.Debug_DrawOrientation();
        }

        currentBuilding.Mesh = BuildingUtility.CombineShapes(shapeWrapper.shapeList);

        if (true)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
                Resources.Load("Materials/TestMaterialBlue") as Material,
                Resources.Load("Materials/TestMaterialRed") as Material,
                Resources.Load("Materials/TestMaterialYellow") as Material,
                Resources.Load("Materials/TestMaterialPink") as Material,
                Resources.Load("Materials/TestMaterialOrange") as Material,
                Resources.Load("Materials/TestMaterialGreen") as Material,
                Resources.Load("Materials/TestMaterialPurple") as Material,
                Resources.Load("Materials/TestMaterialLightGreen") as Material,
                Resources.Load("Materials/TestMaterialLightBlue") as Material,
            };
        levelMeshRenderer.materials = mats;
    }

    public void TaperFixTest()
    {
        List<Mesh> meshes = new List<Mesh>();

        RetrieveBuilding(35, true);

        Shape lot = currentBuilding.Root;


        IShapeGrammarOperation to = new TaperOperation(5f, 2f);
        IShapeGrammarOperation eo = new ExtrudeOperation(Axis.Up, 5f);

        ShapeWrapper shapeWrapper = to.PerformOperation(new List<Shape>() { lot });

        currentBuilding.Mesh = BuildingUtility.CombineShapes(shapeWrapper.shapeList);

        if (false)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
        };

        levelMeshRenderer.materials = mats;
    }

    public void StairFixTest()
    {
        List<Mesh> meshes = new List<Mesh>();

        //dataManager.LoadData();
       // RetrieveBuilding(6, true);
        RetrieveBuilding(10, true);
        //RetrieveBuilding(1);
        //CreateTestSquare();

        Shape lot = currentBuilding.Root;

        Dictionary<string, string> offsetNames = new Dictionary<string, string>
        {
            { "Inside", "a" },
            { "Border", "b" },
        };

        Dictionary<string, string> compNames = new Dictionary<string, string>
        {
            { "Front", "a" },
            { "Back", "b" },
            { "Left", "c" },
            { "Right", "d" },
            { "Top", "e" },
            { "Bottom", "f" },
        };


        List<Shape> frontFaces = new List<Shape>();
        List<Shape> frontStairs = new List<Shape>();

        List<Shape> current = new List<Shape>();

        SplitTerm a = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.3f, "a") });
        SplitTerm b = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.4f, "b") });
        SplitTerm c = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.3f, "c") });

        IShapeGrammarOperation oo = new OffsetOperation(-2.5f, offsetNames);
        IShapeGrammarOperation eo = new ExtrudeOperation(Axis.Up, 6f);
        IShapeGrammarOperation so = new SplitOperation(Axis.Right, new List<SplitTerm>() { a, b, c });
        IShapeGrammarOperation co = new CompOperation(compNames);
        IShapeGrammarOperation so1 = new StairOperation(Direction.Forward, 10);
        IShapeGrammarOperation so2 = new StairOperation(Direction.Back, 10);

        ShapeWrapper shapeWrapper = oo.PerformOperation(new List<Shape>() { lot });

        shapeWrapper = eo.PerformOperation(shapeWrapper.shapeDictionary["a"]);
        shapeWrapper = so.PerformOperation(shapeWrapper.shapeList);
        shapeWrapper = co.PerformOperation(shapeWrapper.shapeDictionary["b"]);


        current = shapeWrapper.shapeDictionary["a"];

        foreach (Shape s in current)
        {
            s.Debug_DrawOrientation(25f);
        }

        frontFaces = shapeWrapper.shapeDictionary["a"];

        shapeWrapper = so1.PerformOperation(frontFaces);

        frontStairs = shapeWrapper.shapeList;

        List<Shape> allShapes = new List<Shape>(frontFaces);
        allShapes.AddRange(frontStairs);

        currentBuilding.Mesh = BuildingUtility.CombineShapes(allShapes);

        if (true)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
        };
        levelMeshRenderer.materials = mats;
    }

    public void CompFixTest()
    {
        List<Mesh> meshes = new List<Mesh>();

        CreateTestSquare();

        Shape lot = currentBuilding.Root;

        Dictionary<string, string> offsetNames = new Dictionary<string, string>
        {
            { "Inside", "a" },
            { "Border", "b" },
        };

        Dictionary<string, string> compNames = new Dictionary<string, string>
        {
            { "Front", "a" },
            { "Back", "b" },
            { "Left", "c" },
            { "Right", "d" },
            { "Top", "e" },
            { "Bottom", "f" },
        };


        List<Shape> frontFaces = new List<Shape>();
        List<Shape> frontStairs = new List<Shape>();

        List<Shape> current = new List<Shape>();

        SplitTerm a = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.3f, "a") });
        SplitTerm b = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.4f, "b") });
        SplitTerm c = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.3f, "c") });

        IShapeGrammarOperation eo = new ExtrudeOperation(Axis.Up, 6f);
        IShapeGrammarOperation co = new CompOperation(compNames);

        ShapeWrapper shapeWrapper = eo.PerformOperation(new List<Shape>() { lot });
        shapeWrapper = co.PerformOperation(shapeWrapper.shapeList);

        current.AddRange(shapeWrapper.shapeDictionary["a"]);
        current.AddRange(shapeWrapper.shapeDictionary["b"]);
        current.AddRange(shapeWrapper.shapeDictionary["c"]);
        current.AddRange(shapeWrapper.shapeDictionary["d"]);
        current.AddRange(shapeWrapper.shapeDictionary["e"]);
        current.AddRange(shapeWrapper.shapeDictionary["f"]);

        foreach (Shape s in current)
        {
            s.Debug_DrawOrientation(25f);
        }

        currentBuilding.Mesh = BuildingUtility.CombineShapes(current);

        if (false)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
        };
        levelMeshRenderer.materials = mats;
    }


    public void NewTriangulateStairTest()
    {
        List<Mesh> meshes = new List<Mesh>();

        CreateTestSquare();

        Shape lot = currentBuilding.Root;

        Dictionary<string, string> offsetNames = new Dictionary<string, string>
        {
            { "Inside", "a" },
            { "Border", "b" },
        };

        Dictionary<string, string> compNames = new Dictionary<string, string>
        {
            { "Front", "a" },
            { "Back", "b" },
            { "Left", "c" },
            { "Right", "d" },
            { "Top", "e" },
            { "Bottom", "f" },
        };


        List<Shape> frontFaces = new List<Shape>();
        List<Shape> frontStairs = new List<Shape>();

        List<Shape> current = new List<Shape>();

        SplitTerm a = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.3f, "a") });
        SplitTerm b = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.4f, "b") });
        SplitTerm c = new SplitTerm(false, new List<SplitRatio>() { new SplitRatio(false, 0.3f, "c") });

        IShapeGrammarOperation eo = new ExtrudeOperation(Axis.Up, 6f);
        IShapeGrammarOperation co = new CompOperation(compNames);

        ShapeWrapper shapeWrapper = eo.PerformOperation(new List<Shape>() { lot });
        shapeWrapper = co.PerformOperation(shapeWrapper.shapeList);

        current.AddRange(shapeWrapper.shapeDictionary["a"]);
        current.AddRange(shapeWrapper.shapeDictionary["b"]);
        current.AddRange(shapeWrapper.shapeDictionary["c"]);
        current.AddRange(shapeWrapper.shapeDictionary["d"]);
        current.AddRange(shapeWrapper.shapeDictionary["e"]);
        current.AddRange(shapeWrapper.shapeDictionary["f"]);

        foreach (Shape s in current)
        {
            s.Debug_DrawOrientation(25f);
        }

        currentBuilding.Mesh = BuildingUtility.CombineShapes(current);

        if (false)
        {
            Vector3[] verts = currentBuilding.Mesh.vertices;
            Vector3[] norms = currentBuilding.Mesh.normals;

            for (int i = 0; i < currentBuilding.Mesh.vertexCount; i++)
            {
                Debug.DrawLine(verts[i], verts[i] + norms[i], Color.green, 1000.0f);
            }
        }

        levelMeshFilter.mesh = currentBuilding.Mesh;

        Material[] mats = new Material[] {
            Resources.Load("Materials/TestMaterialBlue") as Material,
            Resources.Load("Materials/TestMaterialRed") as Material,
            Resources.Load("Materials/TestMaterialYellow") as Material,
            Resources.Load("Materials/TestMaterialPink") as Material,
            Resources.Load("Materials/TestMaterialOrange") as Material,
            Resources.Load("Materials/TestMaterialGreen") as Material,
            Resources.Load("Materials/TestMaterialPurple") as Material,
            Resources.Load("Materials/TestMaterialLightGreen") as Material,
            Resources.Load("Materials/TestMaterialLightBlue") as Material,
        };
        levelMeshRenderer.materials = mats;
    }

    public void ProcessBuildingsRange(int start = -1, int end = -1, bool processAtOrigin = true)
    {
        List<Building> buildings = dataManager.LevelData.Buildings;

        int startIndex = start > 0 ? start : 0;
        int endIndex = end > buildings.Count ? buildings.Count : end;

        for (int i = startIndex; i < endIndex; i++)
        {
            Building building = buildings[i];
            Shape root = building.Root;

            if (processAtOrigin)
            {
                Vector3[] vertices = root.Vertices;

                building.OriginalPosition = root.LocalTransform.Origin;

                Vector3 offset = building.OriginalPosition;

                for (int j = 0; j < vertices.Length; j++)
                {
                    vertices[j] = new Vector3(vertices[j].x - offset.x, vertices[j].y, vertices[j].z - offset.z);
                }

                root.LocalTransform.Origin = Vector3.zero;

                root.Vertices = vertices;
            }

            List<ShapeGrammarData> candidates = null;

            if (building.Info != null)
            {
                candidates = FindShapeGrammarCandidates(building.Info);
            }
            else
            {
                candidates = sgDatabase.shapeGrammarData.ToList();
            }

            bool success = false;

            foreach(ShapeGrammarData candidate in candidates)
            {
                try
                {
                    SGOperationDictionary ruleset = sgParser.ParseRuleFile(candidate.name);

                    ProcessingWrapper processedBuilding = ProcessRuleset(root, ruleset);

                    buildings[i].UpdateProcessedBuilding(processedBuilding);

                    buildings[i].Info.CGARuleset = candidate.name;

                    success = true;
                }
                catch (System.Exception e)
                {
                    success = false;
                    continue;
                }

                if(success)
                {
                    break;
                }
            }

            if(!success)
            {
                Debug.Log("ShapeGrammarProcessor: ProcessBuildings(): could not process building (" + i + ") from candidates");
            }
        }
    }

    public IEnumerator ProcessBuildings(int subset = -1, bool processAtOrigin = true)
    {
        List<Building> buildings = dataManager.LevelData.Buildings;

        int count = subset > 0 ? subset : buildings.Count;

        count = count > buildings.Count ? buildings.Count : count;

        for (int i = 0; i < count; i++)
        {
            Building building = buildings[i];
            Shape root = building.Root;

            if (processAtOrigin)
            {
                Vector3[] vertices = root.Vertices;

                building.OriginalPosition = root.LocalTransform.Origin;

                Vector3 offset = building.OriginalPosition;

                for (int j = 0; j < vertices.Length; j++)
                {
                    vertices[j] = new Vector3(vertices[j].x - offset.x, vertices[j].y, vertices[j].z - offset.z);
                }

                root.LocalTransform.Origin = Vector3.zero;

                root.Vertices = vertices;
            }

            List<ShapeGrammarData> candidates = null;

            if (building.Info != null)
            {
                candidates = FindShapeGrammarCandidates(building.Info);
            }
            else
            {
                candidates = sgDatabase.shapeGrammarData.ToList();
            }

            //candidates.Shuffle();

            bool success = false;

            foreach (ShapeGrammarData candidate in candidates)
            {
                try
                {

                    //

                    SGOperationDictionary ruleset = sgParser.ParseRuleFile(candidate.name);
                    ProcessingWrapper processedBuilding = ProcessRuleset(root, ruleset);

                    buildings[i].Info.CGARuleset = candidate.name;
                    buildings[i].UpdateProcessedBuilding(processedBuilding);

                    success = true;
                }
                catch (System.Exception e)
                {
                    success = false;
                    continue;
                }

                if (success)
                {
                    string extraLoadingText = "Processing Buildings: " + i + " / " + count;

                    Debug.Log(extraLoadingText + " | " + candidate.name);

                    manager.UIManager.UpdateExtraText(extraLoadingText);

                    yield return null;
                    break;
                }
            }

            if (!success)
            {
                Debug.Log("ShapeGrammarProcessor: ProcessBuildings(): could not process building (" + i + ") from candidates");
            }

            if(manager.IsLowMemory)
            {
                manager.UIManager.UpdateExtraText("Device is running low on memory");
                yield return null;
                break;
            }
        }

        yield return null;
    }

    public void ProcessBuildingsWithRuleset(string name, int subset = -1, bool processAtOrigin = true)
    {
        List<Building> buildings = dataManager.LevelData.Buildings;
        SGOperationDictionary simpleTestRuleset = sgParser.ParseRuleFile(name);

        int count = subset > 0 ? subset : buildings.Count;

        count = count > buildings.Count ? buildings.Count : count;

        for (int i = 0; i < count; i++)
        {
            try
            {
                Building building = buildings[i];
                Shape root = building.Root;

                if (processAtOrigin)
                {
                    Vector3[] vertices = root.Vertices;

                    building.OriginalPosition = root.LocalTransform.Origin;

                    Vector3 offset = building.OriginalPosition;

                    for (int j = 0; j < vertices.Length; j++)
                    {
                        vertices[j] = new Vector3(vertices[j].x - offset.x, vertices[j].y, vertices[j].z - offset.z);
                    }

                    root.LocalTransform.Origin = Vector3.zero;

                    root.Vertices = vertices;
                }

                ProcessingWrapper processedBuilding = ProcessRuleset(root, simpleTestRuleset);

                buildings[i].UpdateProcessedBuilding(processedBuilding);
            }
            catch (System.Exception e)
            {
                Debug.Log("ProcessBuildings: Error: (" + i + "): " + e.Message);
                continue;
            }

        }
    }


    public ProcessingWrapper ProcessRuleset(Shape lot, SGOperationDictionary ruleset)
    {

        Dictionary<string, List<Shape>> shapes = new Dictionary<string, List<Shape>>();

        List<ShapeTest> shapeTests = new List<ShapeTest>();

        Dictionary<string, List<IShapeGrammarOperation>> currentRuleset = ruleset._dict;

        shapes.Add("Lot", new List<Shape>() { lot });

        List<string> shapesToRemove = new List<string>();

        foreach (KeyValuePair<string, List<IShapeGrammarOperation>> operation in ruleset._dict)
        {
            List<IShapeGrammarOperation> currentOperationList = operation.Value;
            List<Shape> currentShapes = null;

            bool foundKey = shapes.TryGetValue(operation.Key, out currentShapes);
            if(foundKey)
            {
                ShapeTest shapeTest = null;
                List<OperationTest> operationTests = new List<OperationTest>();

                for (int i = 0; i < currentOperationList.Count; i++)
                {
                    IShapeGrammarOperation currentOperation = currentOperationList[i];
                    
                    ShapeWrapper operationResult = currentOperation.PerformOperation(currentShapes);
                    List<OperationTest> operationTestResult = operationResult.operationTest;

                    if (operationTestResult != null)
                    {
                        operationTests.AddRange(operationTestResult);
                    }

                    switch (operationResult.type)
                    {
                        default:
                        case ShapeContainerType.List:
                            currentShapes = operationResult.shapeList;
                            break;
                        case ShapeContainerType.Dictionary:
                            Dictionary<string, List<Shape>> resultShapeDictionary = operationResult.shapeDictionary;
                            shapes = CombineShapeDictionary(resultShapeDictionary, shapes);
                            break;
                    }

                    if(operationResult.removeParentShape)
                    {
                        shapesToRemove.Add(operation.Key);
                    }
                }

                shapes[operation.Key] = currentShapes;

                shapeTest = new ShapeTest(operation.Key, operationTests);
                shapeTests.Add(shapeTest);
            }
        }

        foreach(string name in shapesToRemove)
        {
            shapes.Remove(name);
        }

        return new ProcessingWrapper(shapes, shapeTests);
    }

    private Dictionary<string, List<Shape>> CombineShapeDictionary(Dictionary<string, List<Shape>> from, Dictionary<string, List<Shape>> to)
    {
        Dictionary<string, List<Shape>> output = to;

        foreach (KeyValuePair<string, List<Shape>> shapeList in from)
        {
            if(output.ContainsKey(shapeList.Key))
            {
                output[shapeList.Key].AddRange(shapeList.Value);
            }
            else
            {
                output.Add(shapeList.Key, shapeList.Value);
            }
        }

        return output;
    }

    private List<ShapeGrammarData> FindShapeGrammarCandidates(BuildingInfo info)
    {
        string name = string.Empty;

        ShapeGrammarData[] sgData = sgDatabase.shapeGrammarData;

        List<ShapeGrammarData> candidates = new List<ShapeGrammarData>();

        for (int i = 0; i < sgData.Length; i++)
        {
            bool success = true;


            ShapeGrammarData sg = sgData[i];

            if (sg.minSides != -1)
            {
                if (info.Sides >= sg.minSides)
                {
                    //success = true;
                }
                else
                {
                    success = false;
                }
            }

            if (sg.maxSides != -1)
            {
                if (info.Sides <= sg.maxSides)
                {
                    //success = true;
                }
                else
                {
                    success = false;
                }
            }


            if (sg.minArea != -1)
            {
                if (info.Area >= sg.minArea)
                {
                    //success = true;
                }
                else
                {
                    success = false;
                }
            }
            
            if (sg.maxArea != -1)
            {
                if (info.Area <= sg.maxArea)
                {
                    //success = true;
                }
                else
                {
                    success = false;
                }
            }

            if(!info.IsConvex)
            {
                if (sgData[i].canBeConcave)
                {
                   //success = true;
                }
                else
                {
                    success = false;
                }
            }

            if(success)
            {
                candidates.Add(sgData[i]);
            }
        }

        if(candidates.Count == 0)
        {
            candidates = sgData.ToList();
        }


        candidates.Shuffle<ShapeGrammarData>();

        return candidates;
    }

    //Create control building for testing, using a square footprint with the specified ruleset
    public Mesh CreateControlBuilding(SGOperationDictionary ruleset, float size)
    {
        Mesh floorPlane = manager.LevelManager.CreatePlane(size, size);
        LocalTransform localTransform = new LocalTransform(Vector3.zero, Vector3.up, Vector3.forward, Vector3.right);
  
        Shape rootShape = new Shape(floorPlane, localTransform);

        ProcessingWrapper processedBuilding = ProcessRuleset(rootShape, ruleset);

        Dictionary<string, List<Shape>> processedBuildingShapes = processedBuilding.processsedShapes;

        List<Shape> allShapes = new List<Shape>();

        allShapes.Add(rootShape);

        foreach (KeyValuePair<string, List<Shape>> currentRule in processedBuildingShapes)
        {
            if (currentRule.Key != "NIL")
            {
                allShapes.AddRange(currentRule.Value);
            }
        }

        Mesh mesh = BuildingUtility.CombineShapes(allShapes);

        return mesh;
    }
}