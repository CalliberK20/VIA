using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Camera cam;
    public GameObject targetCam;

    public float x, y;


    // Update is called once per frame
    void LateUpdate()
    {
        x += Input.GetAxis("Mouse X");
        y -= Input.GetAxis("Mouse Y");

        cam.transform.position = new Vector3(x, y, y);
        cam.transform.rotation = Quaternion.Euler(y * 2f, x * 2f, 0);
    }
}
