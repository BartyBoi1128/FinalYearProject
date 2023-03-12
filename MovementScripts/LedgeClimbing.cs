using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeClimbing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerMovement pm;
    [SerializeField] Climbing cm;
    [SerializeField] Transform orientation;
    [SerializeField] Transform cam;
    [SerializeField] Rigidbody rb;

    [Header("Ledge Grabbing")]
    [SerializeField] float moveToLedgeSpeed;
    [SerializeField] float maxLedgeGrabDistance;
    [SerializeField] float minTimeOnLedge;
    private float timeOnLedge;

    public bool holding;

    [Header("Ledge Jumping")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] float ledgeJumpForwardForce;
    [SerializeField] float ledgeJumpUpwardForce;

    [Header("Ledge Detection")]
    [SerializeField] float ledgeDetectionLength;
    [SerializeField] float ledgeSphereCastRadius;
    [SerializeField] LayerMask whatIsLedge;

    [Header("Exiting")]
    public bool exitingLedge;
    [SerializeField] float exitLedgeTime;
    private float exitLedgeTimer;

    private Transform lastLedge;
    private Transform currLedge;

    private RaycastHit ledgeHit;

    private Vector3 handContact= new Vector3(0f, 0.6f, 0f);


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ledgeCheck();
        StateMachine();
    }

    private void StateMachine() {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        bool keyPressed = horizontalInput != 0 || verticalInput != 0;

        //State 1 - Holding onto ledge
        if (holding)
        {
            ledgeHolding();

            timeOnLedge += Time.deltaTime;

            if (timeOnLedge > minTimeOnLedge && keyPressed)
            {
                exitLedgeHold();
            }

            if (Input.GetKeyDown(jumpKey))
            {
                LedgeJump();
            }
        }
        //State 2 - Exiting Ledge
        else if (exitingLedge) {
            if (exitLedgeTimer > 0)
            {
                exitLedgeTimer -= Time.deltaTime;
            }
            else {
                exitingLedge = false;
            }
        }
    }

    private void ledgeCheck() 
    {
        //Physics.Raycast(transform.position, transform.forward, detectionLength, whatIsClimbable);
        bool ledgeDetected = Physics.SphereCast(transform.position+handContact, ledgeSphereCastRadius, transform.forward, out ledgeHit, ledgeDetectionLength, whatIsLedge);
        //bool ledgeDetected = Physics.Raycast(transform.position + handContact, transform.forward, ledgeDetectionLength, whatIsLedge);
        Debug.DrawRay(transform.position+handContact, transform.forward, Color.green, ledgeDetectionLength);

        if (!ledgeDetected) {
            return;
        }

        float distanceToLedge = Vector3.Distance(transform.position+handContact, ledgeHit.transform.position);

        if (ledgeHit.transform == lastLedge) {
            return;
        }

        if (distanceToLedge < maxLedgeGrabDistance && !holding) {
            enterLedgeHold();
        }
    }

    private void LedgeJump() {
        exitLedgeHold();

        Invoke(nameof(delayedJumpForce), 0.05f);
    }

    private void delayedJumpForce() {
        Vector3 forceToAdd = transform.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpForwardForce;
        rb.velocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }

    private void enterLedgeHold() {
        holding = true;
        pm.animator.SetBool("isLedgeGrabbing", true);
        pm.unlimited = true;
        pm.restricted = true;
        
        currLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    private void ledgeHolding() {
        rb.useGravity = false;

        Vector3 directionToLedge = currLedge.position - transform.position;
        float distanceToLedge = Vector3.Distance(transform.position, currLedge.position);

        if (pm.unlimited)
        {
            pm.unlimited = false;
        }
        if (distanceToLedge > 1f)
        {
            if (rb.velocity.magnitude < moveToLedgeSpeed)
            {
                rb.AddForce(directionToLedge.normalized * moveToLedgeSpeed * 1000f * Time.deltaTime);
            }
        }
        else {
            if (!pm.freeze) {
                pm.freeze = true;
            }
            if (pm.unlimited) {
                pm.unlimited = false;
            }
        }

        if (distanceToLedge > maxLedgeGrabDistance) {
            exitLedgeHold();
        }
    }

    private void exitLedgeHold() {
        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;
        pm.animator.SetBool("isLedgeGrabbing", false);
        holding = false;
        timeOnLedge = 0f;
        cm.climbTimer = cm.maxClimbTime;

        pm.restricted = false;
        pm.freeze = false;

        rb.useGravity = true;

        StopAllCoroutines();
        //Invoke it after 1 second so you cant grab onto the same ledge after one second of leaving it.
        Invoke(nameof(ResetLastLedge),2f);
    }

    private void ResetLastLedge() {
        
        lastLedge = null;
    }
}
