using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public bool lockOn;

    public new Camera camera;
    public Transform lockInObj;
    [Space]
    public Transform target;
    public float mouseSensitivity = 1f;
    private Vector3 Offset;
    [HideInInspector]
    public Vector3 rotation;

    private void Start()
    {
        Offset = transform.position;
    }

    private void Update()
    {
        transform.position = target.position + new Vector3(0, Offset.y);
        if(Input.GetKeyDown(KeyCode.Q))
        {
            lockOn = !lockOn;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        rotation.y -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        if(lockOn)
        {
            transform.LookAt(lockInObj.position);
            rotation = new Vector3(transform.eulerAngles.y, transform.eulerAngles.x);
            return;
        }

        rotation.x += Input.GetAxis("Mouse X") * mouseSensitivity;

        rotation = new Vector3(rotation.x, Mathf.Clamp(rotation.y, -50f, 52.14f));

        transform.rotation = Quaternion.Euler(rotation.y, rotation.x, 0);
    }
}
