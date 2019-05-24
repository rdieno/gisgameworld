using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHelper : MonoBehaviour
{
    [SerializeField]
    private bool rotateCamera = true;
    [SerializeField]
    private float cameraRotationSpeed = 15.0f;

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

        if (rotateCamera)
        {
            camera.transform.RotateAround(Vector3.zero, Vector3.up, cameraRotationSpeed * dt);
        }
    }
}
