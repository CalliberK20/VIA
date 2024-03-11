using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharaControllerMovement : MonoBehaviour
{
    private CharacterController characterController;
    public ThirdPersonCamera thirdPersonCamera;
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
    private bool isRunning = false;

    private float characterSpeed;

    private float _rotationVelocity;
    private float canDashRate = 0;
    private float canDashTime = 0;

    private PlayerStats playerStats;

    // Start is called before the first frame update
    void Start()
    {
        characterSpeed = speed;
        characterController = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>();
        canDashTime = dashTime;
        canDashRate = dashRate;
    }

    // Update is called once per frame
    void Update()
    {
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
            float x = thirdPersonCamera.rotation.x;
            transform.rotation = Quaternion.Euler(0, x, 0);
            _targetRotation = x;
        }

        if (move != Vector3.zero)
        {
            float x = thirdPersonCamera.rotation.x;

            float rot = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + x;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, rot, ref _rotationVelocity,
                    rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, rotation, 0f);

            if (isRunning && playerStats.stamina > 0)
                playerStats.ReduceStamina(Time.deltaTime);

            targetDirection = Quaternion.Euler(0.0f, rotation, 0.0f) * Vector3.forward;
            if (!AttackScript.inCombat)
            {
                if(canDashRate >= 0 && canDashTime >= dashTime && playerStats.stamina > 10)
                {
                    characterController.Move(targetDirection * (dashSpeed * Time.deltaTime));
                    canDashRate = 0;
                    canDashTime = 0;
                    playerStats.ReduceStamina(10f);
                }
                else
                {
                    characterController.Move(targetDirection.normalized * (characterSpeed * Time.deltaTime));
                }
            }
        }

        if(Input.GetButtonUp("Vertical"))
        {
            canDashRate = dashRate;
        }
        canDashRate -= Time.deltaTime;
        canDashTime += Time.deltaTime;

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

    void GroundCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y,
               transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, 0.4f, groundLayer,
            QueryTriggerInteraction.Ignore);
    }
}
