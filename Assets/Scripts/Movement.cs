using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float walkSpeed = 5f;
    private Rigidbody rb;
    public ThirdPersonCamera thirdPersonCamera;
    public Animator animator;
    private Vector3 jumpPoint;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Debug.DrawRay(jumpPoint, Vector3.down, Color.red);
        bool jump = Physics.Raycast(jumpPoint, Vector3.down * 10f);
        Debug.Log(jump);
        if (jump && Input.GetKeyDown(KeyCode.Space))
            rb.AddForce(transform.up * 100f);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        jumpPoint = transform.position + new Vector3(0, 0.5f, 0);
        float x = thirdPersonCamera.rotation.x;
        if (Input.GetButton("Vertical"))
            transform.rotation = Quaternion.Euler(new Vector3(0, x));
    }

    void Move()
    {
        if (AttackScript.inCombat)
            return;

        Vector3 forward = transform.forward * Input.GetAxisRaw("Vertical") * walkSpeed * Time.deltaTime;
        Vector3 side = transform.right * Input.GetAxisRaw("Horizontal") * walkSpeed * Time.deltaTime;
        Vector3 move = forward + side;
        rb.position = transform.position + new Vector3(move.x, 0, move.z) * walkSpeed * Time.fixedDeltaTime;

        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
            animator.SetBool("Run", true);
        else
            animator.SetBool("Run", false);

    }
}
