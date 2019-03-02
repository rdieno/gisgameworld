using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeGrammarManager : MonoBehaviour
{
    //-------------------------
    // Public

    public Color gridColor;

    public bool rotateCamera;
    public float cameraRotationSpeed;

    public GameObject meshObject;


    //-------------------------
    // Private

    private new Camera camera;

    private MeshFilter shapeGrammarMeshFilter;
    private MeshRenderer shapeGrammarMeshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;

        shapeGrammarMeshFilter = meshObject.GetComponent<MeshFilter>();
        shapeGrammarMeshRenderer = meshObject.GetComponent<MeshRenderer>();

        shapeGrammarMeshFilter.mesh = CreateLotRectangle(10f, 10f);

        Material material = shapeGrammarMeshRenderer.materials[0];
        material.mainTexture = CreateTestTexture(10, 10);
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

    Mesh CreateLotRectangle(float width, float depth)
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
}
