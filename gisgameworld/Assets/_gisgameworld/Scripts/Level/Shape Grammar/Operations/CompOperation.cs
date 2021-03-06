using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using g3;

// Component operation: split the input shape(s) into separated components
// currently only able to separate by faces
public class CompOperation : IShapeGrammarOperation
{
    private const float FACE_IGNORE_THRESHOLD = 6f;

    private Dictionary<string, string> componentNames;

    public static bool once = true;

    public CompOperation(Dictionary<string, string> componentNames)
    {
        this.componentNames = componentNames;
    }

    // separates a shapes faces and organizes them into their respective direction based on the shapes local orientation
    public Dictionary<string, List<Shape>> CompFaces(Shape shape)
    {
        DMesh3 dmesh = g3UnityUtils.UnityMeshToDMesh(shape.Mesh);

        // separate the input shape by its faces
        DMesh3[] parts = MeshConnectedComponents.Separate(dmesh);
        List<DMesh3> partsList = new List<DMesh3>(parts);

        // create reference vectors from input shape's orientation
        LocalTransform transform = shape.LocalTransform;

        // Use a list of key value pairs instead of dict because iteration order matters
        List<KeyValuePair<string, Vector3>> directionNormals = new List<KeyValuePair<string, Vector3>>();
        directionNormals.Add(new KeyValuePair<string, Vector3>("Front", transform.Forward));
        directionNormals.Add(new KeyValuePair<string, Vector3>("Left", -transform.Right));
        directionNormals.Add(new KeyValuePair<string, Vector3>("Right", transform.Right));
        directionNormals.Add(new KeyValuePair<string, Vector3>("Back", -transform.Forward));

        // used to determine new orientation vectors
        // required because  we reference different values for front/back vs left/right
        Dictionary<string, Vector3> directionFaceParallelReference = new Dictionary<string, Vector3>();
        directionFaceParallelReference.Add("Front", transform.Right);
        directionFaceParallelReference.Add("Back", transform.Right);
        directionFaceParallelReference.Add("Left", transform.Forward);
        directionFaceParallelReference.Add("Right", transform.Forward);
        
        // separate top and bottom vectors because these are handled slightly differently
        KeyValuePair<string, Vector3> topDirection = new KeyValuePair<string, Vector3>("Top", transform.Up);
        KeyValuePair<string, Vector3> bottomDirection = new KeyValuePair<string, Vector3>("Bottom", -transform.Up);

        Dictionary<string, List<Shape>> output = new Dictionary<string, List<Shape>>();
        List<Shape> currentFaces = new List<Shape>();

        // sort faces by direction
        foreach (KeyValuePair<string, Vector3> direction in directionNormals)
        {
            //Debug.Log("direction order:" + direction.Key);

            // iterate over all faces and to find front faces by comparing face normals to reference vectors
            currentFaces = new List<Shape>();
            for (int i = partsList.Count - 1; i >= 0; i--)
            {
                DMesh3 face = partsList[i];

                // perform dot product between face normal and reference vector
                // a dot product between 0 and 1 means the face's normal is within 90 degrees of the reference
                Vector3 normal = MathUtility.ConvertToVector3(face.GetVertexNormal(0));
                float dot = Vector3.Dot(direction.Value, normal);
                if (dot > 0.0001f && dot <= 1.0001f) // 0.0001 is added to account for float rounding
                {
                    // successfully matched a face to the reference normal
                    // now we must determine the faces new orientation
                    // the faces normal becomes the up vector
                    // the right and forward vectors must be calculated using its vertices

                    Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);

                    Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                    // create a plane that will represent the shape's floor based on it's center and normal
                    Vector3 faceCenter = MathUtility.ConvertToVector3(face.GetBounds().Center);
                    Plane floorPlane = new Plane(transform.Up, faceCenter);

                    List<Vector3> topVerts = new List<Vector3>();
                    List<Vector3> bottomVerts = new List<Vector3>();

                    List<Vector3> topRightVerts = new List<Vector3>();
                    List<Vector3> bottomRightVerts = new List<Vector3>();

                    List<Vector3> topLeftVerts = new List<Vector3>();
                    List<Vector3> bottomLeftVerts = new List<Vector3>();

                    List<Vector3> refVerts = new List<Vector3>();
                    for (int j = 0; j < face.VertexCount; j++)
                    {
                        Vector3 refVert = MathUtility.ConvertToVector3(face.GetVertex(j));
                        refVerts.Add(refVert);
                    }

                    // sort vertices into top and bottom by comparing distance to input shape's floor plane
                    foreach (Vector3 v in refVerts)
                    {
                        if (floorPlane.GetDistanceToPoint(v) > 0f)
                        {
                            topVerts.Add(v);
                        }
                        else
                        {
                            bottomVerts.Add(v);
                        }
                    }

                    // find a vector that will represent the face's new right orientation
                    Vector3? parallelToFace = null;
                    if (topVerts.Count > 1)
                    {
                        Vector3 vertA = Vector3.zero;
                        Vector3 vertB = Vector3.zero;

                        for (int j = 0; j < topVerts.Count - 1; j++)
                        {
                            vertA = topVerts[j];
                            vertB = topVerts[j + 1];

                            // only use vertices that have a distance of over 0.01f
                            // this is due to errors with float rounding not allowing close vertices to give a proper normalized direction vector
                            if (Vector3.SqrMagnitude(vertA - vertB) > 0.01f)
                            {
                                break;
                            }
                        }

                        parallelToFace = (vertB - vertA).normalized;
                    }
                    // if less than two top vertices we can try bottom vertices
                    // because the smallest shape possible is a triangle we are guaranteed to find at least two
                    else if (bottomVerts.Count > 1)
                    {
                        Vector3 vertA = Vector3.zero;
                        Vector3 vertB = Vector3.zero;

                        for (int j = 0; j < bottomVerts.Count - 1; j++)
                        {
                            vertA = bottomVerts[j];
                            vertB = bottomVerts[j + 1];

                            if (Vector3.SqrMagnitude(vertA - vertB) > 0.01f)
                            {
                                break;
                            }
                        }

                        parallelToFace = (vertB - vertA).normalized;
                    }

                    // create a plane representing the shapes 'right' direction
                    // we use this calculation to keep consistency between faces that are opposite of each other
                    // use the reference vector defined above for a direction parallel to the face

                    Vector3 parallelReference = transform.Right;

                    if (directionFaceParallelReference.ContainsKey(direction.Key))
                    {
                        parallelReference = directionFaceParallelReference[direction.Key];
                    }
                    else
                    {
                        Debug.Log("Comp Operation: parallel reference vector key not found");
                    }

                    float parallelDirection = Vector3.Dot(parallelToFace.Value, parallelReference);
                    bool isPointingRight = true;
                    if (parallelDirection < 0)
                    {
                        isPointingRight = false;
                    }
                    Plane rightPlane = new Plane(parallelToFace.Value, newCenter);

                    // sort input shape's face vectors into 4 categories: top left, top right, bottom left, bottom right
                    foreach (Vector3 v in topVerts)
                    {
                        if (rightPlane.GetDistanceToPoint(v) >= 0)
                        {
                            if (isPointingRight)
                            {
                                topRightVerts.Add(v);
                            }
                            else
                            {
                                topLeftVerts.Add(v);
                            }
                        }
                        else
                        {
                            if (isPointingRight)
                            {
                                topLeftVerts.Add(v);
                            }
                            else
                            {
                                topRightVerts.Add(v);
                            }
                        }
                    }

                    foreach (Vector3 v in bottomVerts)
                    {
                        if (rightPlane.GetDistanceToPoint(v) > 0)
                        {
                            if (isPointingRight)
                            {
                                bottomRightVerts.Add(v);
                            }
                            else
                            {
                                bottomLeftVerts.Add(v);
                            }
                        }
                        else
                        {
                            if (isPointingRight)
                            {
                                bottomLeftVerts.Add(v);
                            }
                            else
                            {
                                bottomRightVerts.Add(v);
                            }
                        }
                    }

                    // the faces normal becomes the new up vector
                    Vector3 newUp = normal;

                    // determine the new forward and right vectors using the sorted vertices
                    // if there are not enough vertices in a certain category, use the other vertices that are available
                    // this is not ideal but will create consistent situations so further processing does not fail
                    Vector3? newForward = null;
                    Vector3? newRight = null;

                    // Different procedures for each direction help to ensure
                    // processing rules remained consistent between directions
                    // i.e stairs on left and right sides. one of the stairs could be oriented differently
                    // unless caution is used to ensure consistent results between shapes directional faces orientation vectors
                    switch (direction.Key)
                    {
                        default:
                        case "Front":
                            if (topLeftVerts.Count > 0 && topRightVerts.Count > 0)
                            {
                                newRight = (topRightVerts[0] - topLeftVerts[0]).normalized;
                            }
                            else if (bottomLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                            {
                                newRight = (bottomRightVerts[0] - bottomLeftVerts[0]).normalized;
                            }
                            else if (topLeftVerts.Count > 1)
                            {
                                newRight = (topLeftVerts[0] - topLeftVerts[1]).normalized;
                            }
                            else if (topRightVerts.Count > 1)
                            {
                                newRight = (topRightVerts[0] - topRightVerts[1]).normalized;
                            }

                            if (topLeftVerts.Count > 0 && bottomLeftVerts.Count > 0)
                            {
                                newForward = (bottomLeftVerts[0] - topLeftVerts[0]).normalized;
                            }
                            else if (topRightVerts.Count > 0 && bottomRightVerts.Count > 0)
                            {
                                newForward = (bottomRightVerts[0] - topRightVerts[0]).normalized;
                            }
                            else if (bottomLeftVerts.Count > 1)
                            {
                                newForward = (bottomLeftVerts[0] - bottomLeftVerts[1]).normalized;
                            }
                            else if (bottomRightVerts.Count > 1)
                            {
                                newForward = (bottomRightVerts[0] - bottomRightVerts[1]).normalized;
                            }
                            break;

                        case "Left":
                            if (topLeftVerts.Count > 0 && topRightVerts.Count > 0)
                            {
                                newForward = (topLeftVerts[0] - topRightVerts[0]).normalized;
                            }
                            else if (bottomLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                            {
                                newForward = (bottomLeftVerts[0] - bottomRightVerts[0]).normalized;
                            }
                            else if (bottomLeftVerts.Count > 1)
                            {
                                newForward = (bottomLeftVerts[1] - bottomLeftVerts[0]).normalized;
                            }
                            else if (bottomRightVerts.Count > 1)
                            {
                                newForward = (bottomRightVerts[1] - bottomRightVerts[0]).normalized;
                            }

                            if (topLeftVerts.Count > 0 && bottomLeftVerts.Count > 0)
                            {
                                newRight = (topLeftVerts[0] - bottomLeftVerts[0]).normalized;
                            }
                            else if (topRightVerts.Count > 0 && bottomRightVerts.Count > 0)
                            {
                                newRight = (topRightVerts[0] - bottomRightVerts[0]).normalized;
                            }
                            else if (topLeftVerts.Count > 1)
                            {
                                newRight = (topLeftVerts[1] - topLeftVerts[0]).normalized;
                            }
                            else if (topRightVerts.Count > 1)
                            {
                                newRight = (topRightVerts[1] - topRightVerts[0]).normalized;
                            }
                            break;

                        case "Right":
                            if (topLeftVerts.Count > 0 && topRightVerts.Count > 0)
                            {
                                newForward = (topRightVerts[0] - topLeftVerts[0]).normalized;
                            }
                            else if (bottomLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                            {
                                newForward = (bottomRightVerts[0] - bottomLeftVerts[0]).normalized;
                            }
                            else if (bottomLeftVerts.Count > 1)
                            {
                                newForward = (bottomLeftVerts[0] - bottomLeftVerts[1]).normalized;
                            }
                            else if (bottomRightVerts.Count > 1)
                            {
                                newForward = (bottomRightVerts[0] - bottomRightVerts[1]).normalized;
                            }


                            if (topLeftVerts.Count > 0 && bottomLeftVerts.Count > 0)
                            {
                                newRight = (bottomLeftVerts[0] - topLeftVerts[0]).normalized;
                            }
                            else if (topRightVerts.Count > 0 && bottomRightVerts.Count > 0)
                            {
                                newRight = (bottomRightVerts[0] - topRightVerts[0]).normalized;
                            }
                            else if (topLeftVerts.Count > 1)
                            {
                                newRight = (topLeftVerts[0] - topLeftVerts[1]).normalized;
                            }
                            else if (topRightVerts.Count > 1)
                            {
                                newRight = (topRightVerts[0] - topRightVerts[1]).normalized;
                            }
                            break;

                        case "Back":
                            if (topLeftVerts.Count > 0 && topRightVerts.Count > 0)
                            {
                                newRight = (topLeftVerts[0] - topRightVerts[0]).normalized;
                            }
                            else if (bottomLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                            {
                                newRight = (bottomLeftVerts[0] - bottomRightVerts[0]).normalized;
                            }
                            else if (topLeftVerts.Count > 1)
                            {
                                newRight = (topLeftVerts[1] - topLeftVerts[0]).normalized;
                            }
                            else if (topRightVerts.Count > 1)
                            {
                                newRight = (topRightVerts[1] - topRightVerts[0]).normalized;
                            }

                            if (topLeftVerts.Count > 0 && bottomLeftVerts.Count > 0)
                            {
                                newForward = (topLeftVerts[0] - bottomLeftVerts[0]).normalized;
                            }
                            else if (topRightVerts.Count > 0 && bottomRightVerts.Count > 0)
                            {
                                newForward = (topRightVerts[0] - bottomRightVerts[0]).normalized;
                            }
                            else if (bottomLeftVerts.Count > 1)
                            {
                                newForward = (bottomLeftVerts[1] - bottomLeftVerts[0]).normalized;
                            }
                            else if (bottomRightVerts.Count > 1)
                            {
                                newForward = (bottomRightVerts[1] - bottomRightVerts[0]).normalized;
                            }

                            break;

                    }


                    // if we still do not have a right or forward vector
                    // attempt to use what we have available
                    // this is even less ideal but guarantees we have orientation vectors available to use in future processing
                    if (!newRight.HasValue || !newForward.HasValue)
                    {
                        if (!newRight.HasValue)
                        {
                            if (topVerts.Count > 1)
                            {
                                newRight = (topVerts[0] - topVerts[1]).normalized;
                            }
                            else if (bottomVerts.Count > 1)
                            {
                                newRight = (bottomVerts[0] - bottomVerts[1]).normalized;
                            }
                        }

                        if (!newForward.HasValue)
                        {
                            if (topLeftVerts.Count > 0 && bottomRightVerts.Count > 0)
                            {
                                newForward = (topLeftVerts[0] - bottomRightVerts[0]).normalized;
                            }
                            else if (bottomLeftVerts.Count > 0 && topRightVerts.Count > 0)
                            {
                                newForward = (bottomLeftVerts[0] - topRightVerts[0]).normalized;
                            }
                        }

                        if (!newRight.HasValue || !newForward.HasValue)
                        {
                            Debug.Log("CompOperation: could not determine " + direction.Key.ToLower() + " face orientation vectors");
                        }
                    }


                    // create new local transform object using calculated orientation vectors
                    LocalTransform newTransform = new LocalTransform(newCenter, newUp, newForward.Value, newRight.Value);

                    // create new shape object and add to front faces list
                    Shape newShape = new Shape(newMesh, newTransform);
                    currentFaces.Add(newShape);

                    // remove this face from the original list as it has now been sorted
                    partsList.RemoveAt(i);
                }
            }

            output.Add(direction.Key, currentFaces);
            currentFaces = null;
        }

        // find top faces
        List<Shape> topFaces = new List<Shape>();
        List<Shape> extraFaces = new List<Shape>();
        for (int i = partsList.Count - 1; i >= 0; i--)
        {
            DMesh3 face = partsList[i];
            Vector3 normal = MathUtility.ConvertToVector3(face.GetVertexNormal(0));

            float dot = Vector3.Dot(topDirection.Value, normal);
            if (dot > 0.0001f && dot <= 1.0001f)
            {
                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                // top faces have the same orientation vectors as the input shape
                LocalTransform newTransform = new LocalTransform(newCenter, transform.Up, transform.Forward, transform.Right);

                Shape newShape = new Shape(newMesh, newTransform);

                // hack for ignoring very small faces that cause problems for future processing
                double diag = face.GetBounds().DiagonalLength;
                if (diag <= FACE_IGNORE_THRESHOLD)
                {
                    extraFaces.Add(newShape);
                }
                else
                {
                    topFaces.Add(newShape);
                }

                partsList.RemoveAt(i);
            }
        }

        // find bottom faces
        List<Shape> bottomFaces = new List<Shape>();
        for (int i = partsList.Count - 1; i >= 0; i--)
        {
            DMesh3 face = partsList[i];
            Vector3 normal = MathUtility.ConvertToVector3(face.GetVertexNormal(0));

            float dot = Vector3.Dot(bottomDirection.Value, normal);

            if (dot > 0.0001f && dot <= 1.0001f)
            {
                Mesh newMesh = g3UnityUtils.DMeshToUnityMesh(face);

                Vector3 newCenter = BuildingUtility.FindPolygonCenter(face, normal);

                // bottom faces have an orientation opposite of the input shapes
                // rotate up and right vectors 180 degrees around forward axis
                Vector3 newUp = Quaternion.AngleAxis(180, transform.Forward) * transform.Up;
                Vector3 newRight = Quaternion.AngleAxis(180, transform.Forward) * transform.Right;

                // flip forward axis
                Vector3 newForward = -transform.Forward;

                LocalTransform newTransform = new LocalTransform(newCenter, newUp, newForward, newRight);

                Shape newShape = new Shape(newMesh, newTransform);
                bottomFaces.Add(newShape);

                partsList.RemoveAt(i);

                //bottomPlane = new Plane(newUp, newCenter);
            }
        }

        output.Add("Top", topFaces);
        output.Add("Bottom", bottomFaces);
        output.Add("Extra", extraFaces);

        return output;
    }

    ShapeWrapper IShapeGrammarOperation.PerformOperation(List<Shape> input)
    {
        bool test = true;
        Shape originalShape = null;
        Dictionary<string, Vector3> directionNormals = new Dictionary<string, Vector3>();

        Dictionary<string, List<Shape>> output = new Dictionary<string, List<Shape>>();

        List<bool> part1results = new List<bool>();
        List<bool> part2results = new List<bool>();
        
        foreach (Shape shape in input)
        {
            if (test)
            {
                originalShape = new Shape(shape);
                LocalTransform originalTransform =  originalShape.LocalTransform;

                directionNormals.Add("Front", originalTransform.Forward);
                directionNormals.Add("Left", -originalTransform.Right);
                directionNormals.Add("Right", originalTransform.Right);
                directionNormals.Add("Back", -originalTransform.Forward);
                directionNormals.Add("Top", -originalTransform.Forward);
                directionNormals.Add("Bottom", -originalTransform.Forward);
            }

            Dictionary<string, List<Shape>> currentResult = CompFaces(shape);

            foreach (KeyValuePair<string, List<Shape>> component in currentResult)
            {
                if (output.ContainsKey(componentNames[component.Key]))
                {
                    output[componentNames[component.Key]].AddRange(component.Value);
                }
                else
                {
                    output.Add(componentNames[component.Key], component.Value);
                }
            }

            if(test)
            {
                foreach(KeyValuePair<string, List<Shape>> result in currentResult)
                {
                    List<Shape> processedShapes = result.Value;

                    foreach (Shape processedShape in processedShapes)
                    {
                        LocalTransform processedTransform = processedShape.LocalTransform;
                        bool testResult = CheckForOrthogonalReferenceVectors(processedTransform);
                        part2results.Add(testResult);
                    }
                    
                    Vector3 originalReference = Vector3.zero;

                    bool foundKey = directionNormals.TryGetValue(result.Key, out originalReference);
                    if (foundKey)
                    {
                        foreach (Shape processedShape in processedShapes)
                        {
                            LocalTransform processedTransform = processedShape.LocalTransform;
                            bool testResult = CheckIfWithin90Degrees(originalReference, processedTransform);
                            part1results.Add(testResult);
                        }
                    }
                }
            }
        }

        if(test)
        {
            List<OperationTest> operationTests = new List<OperationTest>();
            operationTests.Add(new OperationTest("comp", "part 1", part1results));
            operationTests.Add(new OperationTest("comp", "part 2", part2results));
            return new ShapeWrapper(output, operationTests, true);
        }

        return new ShapeWrapper(output, true);
    }

    bool CheckIfWithin90Degrees(Vector3 originalReference, LocalTransform processedTransform)
    {
        Vector3 up = new Vector3(processedTransform.Up.x, processedTransform.Up.y, processedTransform.Up.z);

        float testC = Vector3.Dot(up, originalReference);
        if (testC <= 0.0001f || testC > 1.0001f)
        {
           return false;
        }

        return true;
    }


    bool CheckForOrthogonalReferenceVectors(LocalTransform localTransform)
    {
        Vector3 forward = new Vector3(localTransform.Forward.x, localTransform.Forward.y, localTransform.Forward.z);
        Vector3 right = new Vector3(localTransform.Right.x, localTransform.Right.y, localTransform.Right.z);
        Vector3 up = new Vector3(localTransform.Up.x, localTransform.Up.y, localTransform.Up.z);

        float testA = Vector3.Dot(forward, right);
        if(testA < -0.0001f || testA > 0.0001f)
        {
            return false;
        }

        float testB = Vector3.Dot(right, up);
        if (testB <= -0.0001f || testB > 0.0001f)
        {
            return false;
        }

        float testC = Vector3.Dot(up, forward);
        if (testC <= -0.0001f || testC > 0.0001f)
        {
            return false;
        }

        return true;
    }
}
