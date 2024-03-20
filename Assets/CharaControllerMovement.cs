using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharaControllerMovement : MonoBehaviour
{
    private CharacterController characterController;
    //public ThirdPersonCamera thirdPersonCamera;
    public LayerMask groundLayer;
    public float speed = 1f;
    public float shiftSpeed;
    [Space]
    public float dashTime = 1f;
    public float dashRate = 0.1f;
    public float rotationSmoothTime = 0.4f;
    [Space]
    public Animator animator;
    [Space]
    public float dashSpeed = 2;
    [Space]
    public float jumpForce;
    public float verticalVelocity = 0;
    public static float _targetRotation;

    public static Vector3 targetDirection;
    public bool grounded = false;


    [Space]
    public bool canDash = false;
    public bool currentlyDodging = false;
    public Vector3 prevMove;
    public float dodgePointArea = 1f;
    [Space]

    public float canDashRate = 0;
    public float canDashTime = 0;

    private bool isRunning = false;
    private float _rotationVelocity;
    private PlayerStats playerStats;
    private float characterSpeed;
    // Start is called before the first frame update
    void Start()
    {
        characterSpeed = speed;
        characterController = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>();
        canDashRate = dashRate;
        canDashTime = dashTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentlyDodging)
            return;

        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            characterSpeed = shiftSpeed;
            isRunning = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            characterSpeed = speed;
            isRunning = false;
        }

        if (AttackScript.inCombat)
        {
            float x = ThirdPersonCamera.rotation.x;
            transform.rotation = Quaternion.Euler(0, x, 0);
            _targetRotation = x;
        }

        if (Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical"))
        {
            ShiftDodge();
        }

        if (move != Vector3.zero)
        {
            float x = ThirdPersonCamera.rotation.x;

            float rot = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + x;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, rot, ref _rotationVelocity,
                    rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, rotation, 0f);

            if (isRunning && playerStats.orig_Stamina > 0)
                playerStats.ReduceStamina(Time.deltaTime);
            targetDirection = Quaternion.Euler(0.0f, rotation, 0.0f) * Vector3.forward;
            if (!AttackScript.inCombat)
            {
                characterController.Move(targetDirection.normalized * (characterSpeed * Time.deltaTime));
            }
        }

        if (canDash)
            canDashTime -= Time.deltaTime;

        if(canDashTime <= 0)
        {
            canDash = false;
            prevMove = Vector3.zero;
        }

        characterController.Move(new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime);

        animator.SetBool("Run", move != Vector3.zero);
        Jump();
        GroundCheck();
    }

    void Jump()
    {
        if(grounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * -15f);
        }

        if (verticalVelocity < 53f)
            verticalVelocity += -15 * Time.deltaTime;
    }

    void ShiftDodge()
    {
        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        if (canDash && move == prevMove)
        {
            animator.SetTrigger("Dodge");
            canDashTime = 0;
            currentlyDodging = true;
            StartCoroutine(DodgeMove(prevMove));
            prevMove = Vector3.zero;
            canDash = false;
            return;
        }
        canDash = true;
        prevMove = move;
        canDashTime = dashTime;
    }

    private IEnumerator DodgeMove(Vector3 dir)
    {
        float time = 0;
        transform.rotation = Quaternion.Euler(0, ThirdPersonCamera.rotation.x, 0);
        while (true)
        {
            yield return null;
            time += Time.deltaTime;
            characterController.Move((Quaternion.Euler(0, ThirdPersonCamera.rotation.x, 0) * dir) * (speed * Time.deltaTime));
            if (time >= dodgePointArea)
                break;
        }
        currentlyDodging = false;
    }

    void GroundCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y,
               transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, 0.4f, groundLayer,
            QueryTriggerInteraction.Ignore);
    }
}
