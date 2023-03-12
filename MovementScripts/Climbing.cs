using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : MonoBehaviour
{

    [Header("References")]
    [SerializeField] Transform orientation;
    [SerializeField] Rigidbody rb;
    [SerializeField] LayerMask whatIsClimbable;
    public PlayerMovement pm;
    public LedgeClimbing lc;

    [Header("Climbing")]
    [SerializeField] float climbSpeed;
    public float maxClimbTime;
    public float climbTimer;

    private bool climbing;

    [Header("ClimbJumping")]
    [SerializeField] float climbJumpUpForce;
    [SerializeField] float climbJumpBackForce;

    public KeyCode jumpKey = KeyCode.Space;
    [SerializeField] int climbJumps;
    private int climbJumpsLeft;

    [Header("Exting")]
    public bool exitingWall;
    [SerializeField] float exitWallTime;
    float exitWallTimer;

    [Header("Detection")]
    [SerializeField] float detectionLength;
    [SerializeField] float sphereCastRadius;
    [SerializeField] float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    [SerializeField] float minWallNormalAngleChange;
    // Start is called before the first frame update

    private void StateMachine()
    {
        //State 0 - LedgeGrabbing
        if (lc.holding) {
            if (climbing) {
                stopClimbing();
            }
        }

        //State 1 - Climbing
        else if (wallFront && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle && !exitingWall && pm.state != PlayerMovement.MovementState.crouching && pm.state!= PlayerMovement.MovementState.sliding)
        {
            if (!climbing && climbTimer > 0)
            {
                startClimbing();
            }

            //timer
            if (climbTimer > 0)
            {
                climbTimer = climbTimer - Time.deltaTime;
            }
            if (climbTimer < 0)
            {
                stopClimbing();
            }
        }
        //State 2 = Exiting
        else if (exitingWall) {
            if (climbing) { 
                stopClimbing(); 
            }
            if (exitWallTimer > 0) {
                exitWallTimer = exitWallTimer-Time.deltaTime;
            }
            if (exitWallTimer < 0) {
                exitingWall = false;
            }
        }

        //State 3 - None
        else
        {
            if (climbing)
            {
                stopClimbing();
            }
        }

        if (wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0)
        {
            climbJump();
        }
    }

    void Start()
    {
        lc = GetComponent<LedgeClimbing>();
    }

    // Update is called once per frame
    void Update()
    {
        wallCheck();
        StateMachine();
        if (climbing && !exitingWall) {
            climbingMovement();
        }
    }

    private void wallCheck() {
        //Transform position, spherecast, position it points, the wall its hitting, the length of the cast, the layermask
        //wallFront = Physics.SphereCast(transform.position, sphereCastRadius, transform.forward, out frontWallHit, detectionLength, whatIsClimbable);
        wallFront = Physics.Raycast(transform.position, transform.forward, detectionLength, whatIsClimbable);
        Debug.DrawRay(transform.position, transform.forward, Color.red, detectionLength);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || pm.grounded) {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    private void startClimbing() {
        climbing = true;
        pm.climbing = true;
        pm.wallrunning = false;
        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void climbingMovement() {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    private void stopClimbing() {
        climbing = false;
        pm.climbing = false;
        pm.animator.SetBool("isClimbing", false);
    }

    private void climbJump()
    {
        pm.animator.SetBool("isJumping", true);
        if (pm.grounded) {
            return;
        }
        if (lc.holding || lc.exitingLedge) {
            return;
        }
        exitingWall = true;
        exitWallTimer = exitWallTime;
        //Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;
        Vector3 forceToApply = transform.up * climbJumpUpForce + -transform.forward * climbJumpBackForce;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
        Debug.Log("Climb Jump");

        climbJumpsLeft--;
    }
}
