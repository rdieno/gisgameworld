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
    private Vector3 cameraFlatForward;

    private Plane groundPlane;

    private void Awake()
    {
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        cameraFlatRight = Vector3.right;
        cameraFlatForward = Vector3.forward;
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
        Vector3 delta = new Vector3(oneFingerMoveGesture.DeltaPosition.x, 0f, oneFingerMoveGesture.DeltaPosition.y);

        cam.transform.localPosition += (delta.x * cameraFlatRight) * PanSpeed;
        cam.transform.localPosition += (delta.z * cameraFlatForward) * PanSpeed;
    }

    private void twoFingerRotateZoomTransformedHandler(object sender, System.EventArgs e)
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Camera.main.scaledPixelWidth / 2f, Camera.main.scaledPixelHeight / 2f));
        float distance = 0;
        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 intersect = ray.GetPoint(distance);

            float startYRotation = cam.transform.eulerAngles.y;

            cam.transform.RotateAround(intersect, Vector3.up, twoFingerRotateZoomGesture.DeltaRotation * RotationSpeed);

            float endYRotation = cam.transform.eulerAngles.y;

            float deltaYRotation = (endYRotation - startYRotation) % 360;

            Quaternion deltaRotation = Quaternion.AngleAxis(deltaYRotation, Vector3.up);

            cameraFlatRight = deltaRotation * cameraFlatRight;
            cameraFlatForward = deltaRotation * cameraFlatForward;
        }
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
}
