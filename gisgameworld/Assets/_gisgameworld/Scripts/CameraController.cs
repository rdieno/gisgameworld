using UnityEngine;
using System.Collections;
using System;
using TouchScript.Gestures.TransformGestures;

public class CameraController : MonoBehaviour
{
    private const float MIN_CAMERA_ORTHO_SIZE = 3f;
    private const float MAX_CAMERA_ORTHO_SIZE = 1000f;

    public ScreenTransformGesture oneFingerMoveGesture;
    public ScreenTransformGesture twoFingerRotateZoomGesture;
    public float PanSpeed = 1f;
    public float RotationSpeed = 1f;
    public float ZoomSpeed = 200f;

    [SerializeField]
    private Camera cam = null;

    private Vector3 cameraFlatRight;
    //private Vector3 cameraFlatForward;

    private Plane groundPlane;

    private void Awake()
    {
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        cameraFlatRight = Vector3.right;
    }

    private void OnEnable()
    {
        oneFingerMoveGesture.Transformed += oneFingerMoveTransformedHandler;
        twoFingerRotateZoomGesture.Transformed += twoFingerRotateZoomTransformedHandler;
    }

    private void OnDisable()
    {
        oneFingerMoveGesture.Transformed -= oneFingerMoveTransformedHandler;
        twoFingerRotateZoomGesture.Transformed -= twoFingerRotateZoomTransformedHandler;
    }

    private void oneFingerMoveTransformedHandler(object sender, System.EventArgs e)
    {
        //Vector3 delta = new Vector3(oneFingerMoveGesture.DeltaPosition.x, 0f, oneFingerMoveGesture.DeltaPosition.y);
        //cam.transform.localPosition += delta;

        //Vector3 delta = new Vector3(oneFingerMoveGesture.DeltaPosition.x, 0f, oneFingerMoveGesture.DeltaPosition.y);
        cam.transform.localPosition += (oneFingerMoveGesture.DeltaPosition.x * cameraFlatRight);

        //Debug.DrawLine(cam.transform.position, cam.transform.position + (cameraFlatRight * 500f), Color.yellow);

    }

    private void twoFingerRotateZoomTransformedHandler(object sender, System.EventArgs e)
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Camera.main.scaledPixelWidth / 2f, Camera.main.scaledPixelHeight / 2f));
        float distance = 0;
        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 intersect = ray.GetPoint(distance);


            //Debug.Log(twoFingerRotateZoomGesture.DeltaRotation);



            float startYRotation = cam.transform.eulerAngles.y;

            cam.transform.RotateAround(intersect, Vector3.up, twoFingerRotateZoomGesture.DeltaRotation * RotationSpeed);

            //Vector3 endRotation = cam.transform.eulerAngles;

            float endYRotation = cam.transform.eulerAngles.y;

            float deltaYRotation = (endYRotation - startYRotation) % 360;

            Quaternion deltaRotation = Quaternion.AngleAxis(deltaYRotation, Vector3.up);

            //Debug.Log("S: " + startYRotation + ", T: " + endYRotation + ", D: " + deltaYRotation);

            cameraFlatRight = deltaRotation * cameraFlatRight;


            //Vector3 camForward = (intersect - cam.transform.position).normalized;

            //Debug.DrawLine(cam.transform.position, cam.transform.forward * 50f, Color.red);
        }

        //Vector3 a = new Vector3(cam.transform.forward.x, cam.transform.forward.y, cam.transform.forward.z);


        //Debug.DrawLine(cam.transform.position, cam.transform.position + (cam.transform.forward * 500f), Color.red);
        //Debug.DrawLine(cam.transform.position, cam.transform.position + (cameraFlatRight * 500f), Color.yellow);

        //.Log("from: " + cam.transform.position + " | to: " + (cam.transform.position + (cam.transform.forward * 500f)));

        //Debug.Log(cam.transform.forward * (twoFingerRotateZoomGesture.DeltaScale - 1f) * ZoomSpeed);

        //cam.transform.Translate(cam.transform.forward * (twoFingerRotateZoomGesture.DeltaScale - 1f) * ZoomSpeed);

        //cam.transform.localPosition += cam.transform.forward * (twoFingerRotateZoomGesture.DeltaScale - 1f) * ZoomSpeed;



        cam.orthographicSize += (twoFingerRotateZoomGesture.DeltaScale - 1f) * ZoomSpeed;

        if(cam.orthographicSize < MIN_CAMERA_ORTHO_SIZE)
        {
            cam.orthographicSize = MIN_CAMERA_ORTHO_SIZE;
        }
        else if(cam.orthographicSize > MAX_CAMERA_ORTHO_SIZE)
        {
            cam.orthographicSize = MAX_CAMERA_ORTHO_SIZE;
        }
    }

    //private void manipulationTransformedHandler(object sender, System.EventArgs e)
    //{
    //    var rotation = Quaternion.Euler(ManipulationGesture.DeltaPosition.y / Screen.height * RotationSpeed,
    //        -ManipulationGesture.DeltaPosition.x / Screen.width * RotationSpeed,
    //        ManipulationGesture.DeltaRotation);
    //    pivot.localRotation *= rotation;
    //    cam.transform.localPosition += Vector3.forward * (ManipulationGesture.DeltaScale - 1f) * ZoomSpeed;
    //}

    //private void twoFingerTransformHandler(object sender, System.EventArgs e)
    //{
    //    pivot.localPosition += pivot.rotation * TwoFingerMoveGesture.DeltaPosition * PanSpeed;
    //}
}