using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //isJumping, grounded
    [Header("Animator")]
    public Animator animator;
    private bool isJumping;

    [Header("References")]
    public Climbing climbingScript;

    [Header("Movement")]
    private float moveSpeed;
    public float maxYSpeed;
    [SerializeField] float walkSpeed, sprintSpeed, climbSpeed, dashSpeed,swingSpeed,grappleSpeed;
    [SerializeField] float jumpForce, jumpCooldown, airMultiplier, ogJumpForce;
    bool readyToJump;
    [SerializeField] float groundDrag;
    [SerializeField] float wallRunSpeed;
    public bool wallrunning, climbing,freeze,unlimited,restricted,dashing, activeGrapple, swinging, sliding;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;

    [Header("Ground Check")]
    public float playerHeight;
    [SerializeField] LayerMask whatIsGround, whatIsBorder, whatIsWall;
    public bool grounded;
    bool isObjectAbove, tryingToStand;

    [Header("Slope Handling")]
    [SerializeField] float maxAngle;
    [SerializeField] RaycastHit slopeCast;
    private bool exitingSlope;

    [Header("Crouching")]
    [SerializeField] float crouchSpeed;
    [SerializeField] float crouchYScale;
    private float startYScale;

    [SerializeField] Transform orientation;

    float horizontalInput, verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    //MIGHT BE NEEDED LATER
    [SerializeField] Collider collider;

    public MovementState state;

    public enum MovementState {
        freeze,
        unlimited,
        swinging,
        grappling,
        walking,
        sprinting,
        wallRunning,
        dashing,
        crouching,
        air,
        sliding,
        climbing,
        idle
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        //collider = GetComponent<Collider>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
        ogJumpForce = jumpForce;
        tryingToStand = false;
    }

    private void Update()
    {
        //groundCheck
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        //handleDrag
        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching) {
            rb.drag = groundDrag;
        } else {
            rb.drag = 0;
        }
        if (tryingToStand)
        {
            isObjectAbove = Physics.Raycast(transform.position, Vector3.up, playerHeight * 2f + 0.2f, whatIsGround) || (Physics.Raycast(transform.position, Vector3.up, playerHeight * 2f + 0.2f, whatIsBorder));
            if (!isObjectAbove)
            {
                //CHANGEHERENOW
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                //collider.transform.Translate(0, 0.38f, 0);
                tryingToStand = false;
            }
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded) {
            grounded = false;
            animator.SetBool("isGrounded", false);
            animator.SetBool("isJumping", true);
            readyToJump = false;
            if (onSlope())
            {
                jumpForce = jumpForce * 0.75f;
                Jump();
                jumpForce = ogJumpForce;
            }
            else
            {
                Jump();
            }
            Invoke(nameof(resetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey) && grounded) {
            //grounded = false;
            //animator.SetBool("isGrounded", false);
            //animator.SetBool("isTryingToCrouch", true);
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            //collider.transform.Translate(0, -0.38f, 0);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(crouchKey))
        {
            isObjectAbove = Physics.Raycast(transform.position, Vector3.up, playerHeight * 2f + 0.8f, whatIsGround) || Physics.Raycast(transform.position, Vector3.up, playerHeight * 2f + 0.2f, whatIsBorder) || Physics.Raycast(transform.position, Vector3.up, playerHeight * 2f + 0.2f, whatIsWall);
            if (!isObjectAbove)
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                //collider.transform.Translate(0, 0.38f, 0);
            }
            else {
                tryingToStand = true;
            }
        }
    }

    public void cantStand(){
        state = MovementState.crouching;
        moveSpeed = crouchSpeed;
        tryingToStand = true;
    }

    private void StateHandler() {
        //Mode - Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            rb.velocity = Vector3.zero;
        }
        else if (unlimited)
        {
            state = MovementState.unlimited;
            moveSpeed = 20f;
            return;
        }
        // Mode - Swinging
        else if (activeGrapple) {
            state = MovementState.grappling;
        }
        else if (swinging)
        {
            state = MovementState.swinging;
            moveSpeed = swingSpeed;
            Debug.Log("Swinging");
            animator.SetBool("isSwinging", true);
            animator.SetBool("isGrappling", false);
        }
        else if (dashing)
        {
            state = MovementState.dashing;
            moveSpeed = dashSpeed;
            Debug.Log("AIRDASHING");
            animator.SetBool("isAirDashing", true);
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                animator.SetBool("isGrappling", false);
            }
            /*animator.SetBool("isSliding", false);
            animator.SetBool("isCrouchWalking", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isGrounded", false);
            animator.SetBool("isTryingToCrouch", false);
            animator.SetBool("isCrouching", false);*/
        }
        else if (climbing)
        {
            state = MovementState.climbing;
            moveSpeed = climbSpeed;
            Debug.Log("Climbing");
            animator.SetBool("isClimbing", true);
            animator.SetBool("isSwinging", false);
            animator.SetBool("isSliding", false);
            animator.SetBool("isCrouchWalking", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isGrounded", false);
            animator.SetBool("isTryingToCrouch", false);
            animator.SetBool("isCrouching", false);
            animator.SetBool("isAirDashing",false);
        }
        else if (wallrunning)
        {
            state = MovementState.wallRunning;
            moveSpeed = wallRunSpeed;
            Debug.Log("WallRunning");
            animator.SetBool("isAirDashing", false);
            animator.SetBool("isSwinging", false);
        }
        if (tryingToStand && (horizontalInput != 0 || verticalInput != 0))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
            animator.SetBool("isCrouchWalking", true);
            animator.SetBool("isSliding", false);
        }
        else if (tryingToStand)
        {
            animator.SetBool("isCrouchWalking", false);
            animator.SetBool("isCrouching", true);
            animator.SetBool("isSliding", false);
        }
        else if (sliding) {
            state = MovementState.sliding;
            animator.SetBool("isSliding", true);
            animator.SetBool("isCrouchWalking", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isGrounded", true);
            animator.SetBool("isTryingToCrouch", false);
            animator.SetBool("isCrouching", false);
            animator.SetBool("isAirDashing", false);
            animator.SetBool("isClimbing", false);
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                animator.SetBool("isSwinging", false);
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                animator.SetBool("isGrappling", false);
            }
        }
        //Crouch Walking
        else if (Input.GetKey(crouchKey) && grounded == true && (horizontalInput != 0 || verticalInput != 0))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
            animator.SetBool("isCrouchWalking", true);
            animator.SetBool("isSliding", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isGrounded", true);
            //animator.SetBool("isTryingToCrouch", false);
            animator.SetBool("isCrouching", true);
            animator.SetBool("isTryingToCrouch", true);
            animator.SetBool("isAirDashing", false);
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                animator.SetBool("isSwinging", false);
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                animator.SetBool("isGrappling", false);
            }
        }
        else if (Input.GetKey(crouchKey) && grounded == true)
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
            animator.SetBool("isCrouching", true);
            animator.SetBool("isCrouchWalking", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isGrounded", true);
            animator.SetBool("isTryingToCrouch", true);
            animator.SetBool("isSliding", false);
            animator.SetBool("isAirDashing", false);
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                animator.SetBool("isSwinging", false);
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                animator.SetBool("isGrappling", false);
            }
        }
        else if (grounded && Input.GetKey(sprintKey) && (horizontalInput != 0 || verticalInput != 0))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
            animator.SetBool("isSprinting", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("isCrouching", false);
            animator.SetBool("isGrounded", true);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isTryingToCrouch", false);
            animator.SetBool("isSliding", false); 
            animator.SetBool("isAirDashing", false);
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                animator.SetBool("isSwinging", false);
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                animator.SetBool("isGrappling", false);
            }
        }
        else if (grounded && (horizontalInput != 0 || verticalInput != 0))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            isJumping = false;
            animator.SetBool("isSprinting", false);
            animator.SetBool("isCrouching", false);
            animator.SetBool("isWalking", true);
            animator.SetBool("isGrounded", true);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isTryingToCrouch", false);
            animator.SetBool("isSliding", false);
            animator.SetBool("isAirDashing", false);
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                animator.SetBool("isSwinging", false);
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                animator.SetBool("isGrappling", false);
            }
        }
        else if (grounded)
        {
            state = MovementState.idle;
            moveSpeed = 0f;
            isJumping = false;
            animator.SetBool("isSprinting", false);
            animator.SetBool("isCrouching", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isGrounded", true);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isTryingToCrouch", false);
            animator.SetBool("isSliding", false);
            animator.SetBool("isAirDashing", false);
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                animator.SetBool("isSwinging", false);
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                animator.SetBool("isGrappling", false);
            }
        }
        else
        {
            state = MovementState.air;
            moveSpeed = walkSpeed;
            animator.SetBool("isGrounded", false);
            animator.SetBool("isFalling", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("isSprinting", false);
            animator.SetBool("isTryingToCrouch", false);
            animator.SetBool("isCrouching", false);
            animator.SetBool("isSliding", false);
            if (Input.GetKeyUp(KeyCode.Mouse0)) {
                animator.SetBool("isSwinging", false);
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                animator.SetBool("isGrappling", false);
            }
            //animator.SetBool("isClimbing", false);
            //animator.SetBool("isAirDashing", false);
        }
    }

    private void MovePlayer() {
        //If player is restricted, return out of the function
        if (restricted) {
            return;
        }
        //Stop all movement while exiting a wall
        if (climbingScript.exitingWall) {
            return;
        }

        //return while dashing
        if (state == MovementState.dashing) {
            return;
        }
        
        //Stop movement if swinging
        if (swinging) {
            Debug.Log("SwingingMove");
            return;
        }

        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (onSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0) {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded) {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (onSlope()) {
            if (!wallrunning)
            {
                //Might be needed later
                //GetComponent<Collider>().sharedMaterial = (PhysicMaterial)Resources.Load("PhysicMaterials/HighFriction");
                collider.sharedMaterial = (PhysicMaterial)Resources.Load("PhysicMaterials/HighFriction");
                // Set the friction of the object to a high value

            }
        }
        else
        {
            //Might be needed later
            collider.sharedMaterial = (PhysicMaterial)Resources.Load("PhysicMaterials/NoFriction");
        }
    }

    private void SpeedControl() {
        if (activeGrapple) {
            moveSpeed = grappleSpeed;
            return;
        }

        if (onSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else {
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }

        //limit Y Velocity if using camera forward for dashing
        if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed) {
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
        }
    }

    private void Jump() {
        animator.SetBool("isJumping", true);
        Debug.Log("JUMPING");
        isJumping = true;
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void resetJump() {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool onSlope() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeCast, playerHeight * 0.5f + 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeCast.normal);
            return angle < maxAngle && angle != 0;
        }

        return false;
    }

    //Remember this has to be normalized as it is a direction
    public Vector3 GetSlopeMoveDirection(Vector3 direction) {
        return Vector3.ProjectOnPlane(direction, slopeCast.normal).normalized;
    }

    //Equation from Kinematic Equations by Sebastian Lague for grappling:
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight) {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2*gravity*trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)+ Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    private bool enableMovementOnNextTouch;

    public void JumpTopPosition(Vector3 targetPosition, float trajectoryHeight) {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(resetRestrictions), 3f);
    }

    private Vector3 velocityToSet;

    private void SetVelocity() {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
        Debug.Log(rb.velocity);
    }

    public void resetRestrictions() {
        animator.SetBool("isGrappling", false);
        activeGrapple = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch) {
            enableMovementOnNextTouch = false;
            resetRestrictions();

            GetComponent<Grappling>().stopGrappling();
        }
    }
}
