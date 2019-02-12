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

    public MeshFilter shapeGrammarMeshFilter;

    //-------------------------
    // Private

    private new Camera camera;


    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;


    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        if(rotateCamera)
        {
            camera.transform.RotateAround(Vector3.zero, Vector3.up, cameraRotationSpeed * dt);
        }

        shapeGrammarMeshFilter.mesh = CreateLotRectangle(10f, 5f);
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
}
