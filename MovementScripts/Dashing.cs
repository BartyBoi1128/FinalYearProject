using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerCam;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Dashing")]
    [SerializeField] float dashForce;
    [SerializeField] float dashUpwardForce;
    [SerializeField] float dashDuration;
    [SerializeField] float maxDashYSpeed;
    [SerializeField] float dashCD;
    private float dashCDTimer;

    [Header("Tweening")]
    [SerializeField] PlayerCamera cam;
    [SerializeField] float dashFOV;

    [Header("Direction")]
    [SerializeField] bool useCameraForward = true;
    [SerializeField] bool allowAllDirections = true;
    [SerializeField] bool disableGravity = false;
    [SerializeField] bool resetVelocity = true;

    [Header("Input")]
    [SerializeField] KeyCode dashKey = KeyCode.E;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(dashKey) && !pm.grounded) {
            Dash();
        }

        if (dashCDTimer > 0) {
            dashCDTimer -= Time.deltaTime;
        }
    }


    private void Dash() {
        if (dashCDTimer > 0)
        {
            return;
        }
        else {
            dashCDTimer = dashCD;
        }
        pm.dashing = true;
        pm.maxYSpeed = maxDashYSpeed;

        cam.DoFov(dashFOV);

        Transform forwardT;
        if (useCameraForward)
        {
            forwardT = playerCam;
        }
        else {
            forwardT = orientation;
        }

        Vector3 dashDirection = GetDirection(forwardT);

        Vector3 forceToApply = dashDirection * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity) {
            rb.useGravity = false;
        }

        delayedForceToApply = forceToApply;

        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        if (resetVelocity) {
            rb.velocity = Vector3.zero;
        }

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash() {
        pm.dashing = false;
        pm.maxYSpeed = 0;

        cam.DoFov(85f);

        if (disableGravity) {
            rb.useGravity = true;
        }
    }

    private Vector3 GetDirection(Transform forwardT) {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalnput = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3();

        if (allowAllDirections)
        {
            direction = forwardT.forward * verticalnput + forwardT.right * horizontalInput;
        }
        else {
            direction = forwardT.forward;
        }
        if (verticalnput == 0 && horizontalInput == 0) {
            direction = forwardT.forward;
        }

        return direction.normalized;
    }
}
