using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    [SerializeField] LayerMask whatIsWall;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] float wallRunForce;
    [SerializeField] float wallClimbSpeed;
    [SerializeField] float maxWallRunTime;
    [SerializeField] float wallJumpUpForce;
    [SerializeField] float wallJumpSideForce;
    private float wallRunTimer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    public KeyCode jumpKey = KeyCode.Space;
    private bool upwardsRunning, downwardsRunning;

    [Header("Detection")]
    //Some of these might have to be public
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;
    private Vector3 lastNormalHit;
    private bool allowRun;

    [Header("References")]
    [SerializeField] Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;
    private LedgeClimbing lc;

    [Header("ExitingWallRun")]
    private bool exitingWall;
    [SerializeField] float exitWallTime;
    private float exitWallTimer;
    private Vector3 lastNormalJump;

    [Header("Gravity")]
    [SerializeField] bool useGravity;
    [SerializeField] float gravityCounterForce;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        lc = GetComponent<LedgeClimbing>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!AboveGround()) {
            lastNormalHit.x = 0;
            lastNormalJump.x = 0;
        }
        CheckForWall();
        StateMachine();
    }

    private void CheckForWall() {
        //Start Point, Direction, store hit info, distance, wall
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround() {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        //WallRunning State
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            Vector3 currentWallNormal = wallRight ? rightWallhit.normal : leftWallHit.normal;
            if (!pm.wallrunning && (currentWallNormal.x != lastNormalHit.x))
            {
                StartWallRun();
            }

            if (wallRunTimer > 0) {
                wallRunTimer -= Time.deltaTime;
            }

            if (wallRunTimer <= 0 && pm.wallrunning) {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }
            if (Input.GetKeyDown(jumpKey) && !pm.climbing)
            {
                WallJump();
            }
        }
        else if (exitingWall) {
            if (pm.wallrunning)
            {
                StopWallRun();
            }
            if (exitWallTimer > 0) {
                exitWallTimer -= Time.deltaTime;
            }
            if (exitWallTimer <= 0) {
                exitingWall = false;
            }
        }
        else
        {
            if (pm.wallrunning)
            {
                StopWallRun();
            }
        }   
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning) {
            WallRunningMovement();
        }
    }

    private void StartWallRun() {
        pm.wallrunning = true;

        wallRunTimer = maxWallRunTime;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }

    private void WallRunningMovement()
    {
        Debug.Log("WallRunning");
        rb.useGravity = useGravity;
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallHit.normal;

            lastNormalHit = wallNormal;
            Vector3 wallForward = Vector3.Cross(lastNormalHit, transform.up);
            if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            {
                wallForward = -wallForward;
            }
            //forward force
            rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

            //upwards/downwards force
            if (upwardsRunning)
            {
                rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
            }
            if (downwardsRunning)
            {
                rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);
            }
            //push to wall force
            if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            {
                rb.AddForce(-wallNormal * 100, ForceMode.Force);
            }

        //Weaken gravity for better wallRunning experience
        if (useGravity) {
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
    }

    private void StopWallRun() {
        pm.wallrunning = false;
    }

    private void WallJump() {
        if (lc.holding || lc.exitingLedge) {
            return;
        }

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallHit.normal;
        if (lastNormalJump.x != wallNormal.x)
        {
            lastNormalJump = wallNormal;
            Vector3 forceToApply = transform.up * wallJumpUpForce + lastNormalJump * wallJumpSideForce;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(forceToApply, ForceMode.Impulse);
        }
    }
}
