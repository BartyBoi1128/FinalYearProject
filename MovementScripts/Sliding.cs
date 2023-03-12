using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform playerObject;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    [SerializeField] float maxSlideTime;
    [SerializeField] float slideForce;
    private float slideTimer;

    [SerializeField] float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    [Header("Ground Check")]
    [SerializeField] float playerHeight;
    [SerializeField] LayerMask whatIsGround, whatIsBorder;
    bool isObjectAbove;

    private bool sliding;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObject.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0) && (pm.state != PlayerMovement.MovementState.crouching) && pm.grounded)
        {
            startSlide();
        }
        if (Input.GetKeyUp(slideKey) && sliding) {
            stopSlide();
        }
    }

    private void FixedUpdate()
    {
        if (sliding)
        {
            slidingMovement();
        }
    }

    private void slidingMovement() { 
       Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (!pm.onSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
        else {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0) {
            stopSlide();
        }
    }

    private void startSlide() {
        sliding = true;
        pm.sliding = true;

        playerObject.localScale = new Vector3(playerObject.localScale.x, slideYScale, playerObject.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void stopSlide()
    {
        sliding = false;
        pm.sliding = false;
        isObjectAbove = Physics.Raycast(transform.position, Vector3.up, playerHeight * 2f + 0.8f, whatIsGround) || Physics.Raycast(transform.position, Vector3.up, playerHeight * 2f + 0.2f, whatIsBorder);
        if (!isObjectAbove)
        {
            playerObject.localScale = new Vector3(playerObject.localScale.x, startYScale, playerObject.localScale.z);
        }
        else
        {
            pm.cantStand();
        }
    }
}
