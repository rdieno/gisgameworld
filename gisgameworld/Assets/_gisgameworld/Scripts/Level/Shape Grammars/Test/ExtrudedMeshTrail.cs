using System.Collections.Generic;
using UnityEngine;

// Generates an extrusion trail from the attached mesh
// Uses the MeshExtrusion algorithm in MeshExtrusion.cs to generate and preprocess the mesh.
[RequireComponent(typeof(MeshFilter))]
public class ExtrudedMeshTrail : MonoBehaviour
{
    public float time = 2.0f;
    public bool autoCalculateOrientation = true;
    public float minDistance = 0.1f;
    public bool invertFaces = false;

    private Mesh srcMesh;
    private MeshExtrusion.Edge[] precomputedEdges;
    private List<ExtrudedTrailSection> sections = new List<ExtrudedTrailSection>();

    public bool HasInitialized = false;
    public bool runOnce = true;

    void Start()
    {


    }

    public void Init()
    {
        srcMesh = GetComponent<MeshFilter>().sharedMesh;
        precomputedEdges = MeshExtrusion.BuildManifoldEdges(srcMesh);
        HasInitialized = true;
    }

    private void Update()
    {
        if (HasInitialized && runOnce)
        {

            runOnce = false;

            Vector3 position = transform.position;
            float now = Time.time;

            //while (sections.Count > 0 && now > sections[sections.Count - 1].time + time)
            //{
            //    sections.RemoveAt(sections.Count - 1);
            //}


            ExtrudedTrailSection section = new ExtrudedTrailSection();
            section.point = position;
            section.matrix = transform.localToWorldMatrix;
            section.time = now;
            sections.Insert(0, section);

            transform.Translate(0.0f, 5.0f, 0.0f);
            Vector3 offset = new Vector3(0.0f, 5.0f, 0.0f);

            ExtrudedTrailSection section2 = new ExtrudedTrailSection();
            //section2.point = transform.position;
            //section2.matrix = transform.localToWorldMatrix;

            section2.point = offset;
            section2.matrix = Matrix4x4.Translate(offset);
            section2.time = now;
            sections.Insert(0, section2);

            //transform.Translate(0.0f, 5.0f, 0.0f);

            //ExtrudedTrailSection section3 = new ExtrudedTrailSection();
            //section3.point = transform.position;
            //section3.matrix = transform.localToWorldMatrix;
            //section3.time = now;
            //sections.Insert(0, section3);

            //transform.Translate(0.0f, 5.0f, 0.0f);

            //ExtrudedTrailSection section4 = new ExtrudedTrailSection();
            //section4.point = transform.position;
            //section4.matrix = transform.localToWorldMatrix;
            //section4.time = now;
            //sections.Insert(0, section4);

           /// transform.Translate(0.0f, -15.0f, 0.0f);
            //transform.Translate(0.0f, 10.0f, 0.0f);

            // We need at least 2 sections to create the line
            if (sections.Count < 2)
            {
                return;
            }

            Matrix4x4 worldToLocal = transform.worldToLocalMatrix;
            Matrix4x4[] finalSections = new Matrix4x4[sections.Count];
            Quaternion previousRotation = new Quaternion();

            finalSections[0] = Matrix4x4.identity;
            finalSections[1] = worldToLocal * sections[1].matrix;
           // finalSections[2] = worldToLocal * sections[2].matrix;
            //finalSections[3] = worldToLocal * sections[3].matrix;

            //    for (int i = 0; i < sections.Count; i++)
            //{
            //    if (autoCalculateOrientation)
            //    {
            //        if (i == 0)
            //        {
            //            Vector3 direction = sections[0].point - sections[1].point;
            //            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            //            previousRotation = rotation;
            //            finalSections[i] = worldToLocal * Matrix4x4.TRS(position, rotation, Vector3.one);
            //        }
            //        // all elements get the direction by looking up the next section
            //        else if (i != sections.Count - 1)
            //        {
            //            Vector3 direction = sections[i].point - sections[i + 1].point;
            //            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            //            // When the angle of the rotation compared to the last segment is too high
            //            // smooth the rotation a little bit. Optimally we would smooth the entire sections array.
            //            if (Quaternion.Angle(previousRotation, rotation) > 20)
            //            {
            //                rotation = Quaternion.Slerp(previousRotation, rotation, 0.5f);
            //            }

            //            previousRotation = rotation;
            //            finalSections[i] = worldToLocal * Matrix4x4.TRS(sections[i].point, rotation, Vector3.one);
            //        }
            //        // except the last one, which just copies the previous one
            //        else
            //        {
            //            finalSections[i] = finalSections[i - 1];
            //        }
            //    }
            //    else
            //    {
            //        if (i == 0)
            //        {
            //            finalSections[i] = Matrix4x4.identity;
            //        }
            //        else
            //        {
            //            finalSections[i] = worldToLocal * sections[i].matrix;
            //        }
            //    }
            //}
            MeshExtrusion.ExtrudeMesh(srcMesh, GetComponent<MeshFilter>().mesh, finalSections, precomputedEdges, invertFaces);


            int p = srcMesh.vertexCount;

            int i = 0;
        }
    }

    //private void Update()
    //{
    //    if (HasInitialized && runOnce)
    //    {
    //        srcMesh = GetComponent<MeshFilter>().sharedMesh;
    //        precomputedEdges = MeshExtrusion.BuildManifoldEdges(srcMesh);
    //        runOnce = false;
    //    }
    //}

    //void LateUpdate()
    //{
    //    if (HasInitialized)
    //    {
    //        Vector3 position = transform.position;
    //        float now = Time.time;

    //        while (sections.Count > 0 && now > sections[sections.Count - 1].time + time)
    //        {
    //            sections.RemoveAt(sections.Count - 1);
    //        }

    //        // Add a new trail section to beginning of array
    //        if (sections.Count == 0 || (sections[0].point - position).sqrMagnitude > minDistance * minDistance)
    //        {
    //            ExtrudedTrailSection section = new ExtrudedTrailSection();
    //            section.point = position;
    //            section.matrix = transform.localToWorldMatrix;
    //            section.time = now;
    //            sections.Insert(0, section);
    //        }

    //        // We need at least 2 sections to create the line
    //        if (sections.Count < 2)
    //        {
    //            return;
    //        }

    //        Matrix4x4 worldToLocal = transform.worldToLocalMatrix;
    //        Matrix4x4[] finalSections = new Matrix4x4[sections.Count];
    //        Quaternion previousRotation = new Quaternion();

    //        for (int i = 0; i < sections.Count; i++)
    //        {
    //            if (autoCalculateOrientation)
    //            {
    //                if (i == 0)
    //                {
    //                    Vector3 direction = sections[0].point - sections[1].point;
    //                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
    //                    previousRotation = rotation;
    //                    finalSections[i] = worldToLocal * Matrix4x4.TRS(position, rotation, Vector3.one);
    //                }
    //                // all elements get the direction by looking up the next section
    //                else if (i != sections.Count - 1)
    //                {
    //                    Vector3 direction = sections[i].point - sections[i + 1].point;
    //                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

    //                    // When the angle of the rotation compared to the last segment is too high
    //                    // smooth the rotation a little bit. Optimally we would smooth the entire sections array.
    //                    if (Quaternion.Angle(previousRotation, rotation) > 20)
    //                    {
    //                        rotation = Quaternion.Slerp(previousRotation, rotation, 0.5f);
    //                    }

    //                    previousRotation = rotation;
    //                    finalSections[i] = worldToLocal * Matrix4x4.TRS(sections[i].point, rotation, Vector3.one);
    //                }
    //                // except the last one, which just copies the previous one
    //                else
    //                {
    //                    finalSections[i] = finalSections[i - 1];
    //                }
    //            }
    //            else
    //            {
    //                if (i == 0)
    //                {
    //                    finalSections[i] = Matrix4x4.identity;
    //                }
    //                else
    //                {
    //                    finalSections[i] = worldToLocal * sections[i].matrix;
    //                }
    //            }
    //        }

    //        int p = 0;

    //        MeshExtrusion.ExtrudeMesh(srcMesh, GetComponent<MeshFilter>().mesh, finalSections, precomputedEdges, invertFaces);

    //        p = 0;
    //    }
    //}

    class ExtrudedTrailSection
    {
        public Vector3 point;
        public Matrix4x4 matrix;
        public float time;
    }
}