using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Swinging : MonoBehaviour
{

    [Header("References")]
    [SerializeField] LineRenderer lr;
    [SerializeField] Transform hook, cam, player;
    [SerializeField] LayerMask whatIsGrappleable;
    [SerializeField] CinemachineFreeLook basicCam;
    [SerializeField] PlayerCamera pc;
    public PlayerMovement pm;

    [Header("Swinging")]
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;

    [Header("Input")]
    [SerializeField] KeyCode swingKey = KeyCode.Mouse0;
    [SerializeField] KeyCode activateGrapple = KeyCode.F;

    private Vector3 currentGrapplePosition;
    private bool activated;

    [Header("AirSwinging")]
    [SerializeField] Transform orientation;
    [SerializeField] Rigidbody rb;
    [SerializeField] float horizontalThrustForce;
    [SerializeField] float forwardThrustForce;
    [SerializeField] float extendCableSpeed;

    //Adjust for players missing slightly
    [Header("Prediction")]
    [SerializeField] RaycastHit predictedHit;
    [SerializeField] float predictedSphereCastRadius;
    [SerializeField] Transform predictedPoint;
    
    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(activateGrapple))
        {
            activateHook();
        }
        if (Input.GetKeyDown(swingKey) && activated){
            startSwing();
        }
        if (Input.GetKeyUp(swingKey))
        {
            stopSwing();
        }
        if (joint != null)
        {
            advancedSwingingAir();
        }
         checkForSwingPoints();
    }

    void LateUpdate()
    {
        drawRope();
    }

    private void startSwing()
    {
        if (predictedHit.point == Vector3.zero) {
            return;
        }
        //stop grappling as you're swinging
        if (GetComponent<Grappling>() != null)
        {
            GetComponent<Grappling>().stopGrappling();
        }
        pm.resetRestrictions();

        pm.swinging = true;

        swingPoint = predictedHit.point;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        //max and min values
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        lr.positionCount = 2;
        currentGrapplePosition = hook.position;
    }

    public void stopSwing() {
        pm.swinging = false;
        lr.positionCount = 0;
        Destroy(joint);
    }

    private void drawRope() {
        if (!joint) {
            return;
        }

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime*8f);

        lr.SetPosition(0, hook.position);
        lr.SetPosition(1, swingPoint);
    }

    private void activateHook()
    {
        activated = !activated;
        if (activated)
        {
            Debug.Log("activated");
            pc.currentStyle = PlayerCamera.CameraType.Grappling;
            basicCam.enabled = false;
        }
        else
        {
            pc.currentStyle = PlayerCamera.CameraType.Basic;
            basicCam.enabled = true;
            Debug.Log("deactivated");
            predictedPoint.gameObject.SetActive(false);
        }
    }

    private void advancedSwingingAir() {
        // right
        if (Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        // left
        if (Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);

        // forward
        if (Input.GetKey(KeyCode.W)) rb.AddForce(orientation.forward * horizontalThrustForce * Time.deltaTime);

        // shorten cable
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = swingPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }
        // extend cable
        if (Input.GetKey(KeyCode.S))
        {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }

    private void checkForSwingPoints() {
        if (joint != null) {
            return;
        }
        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictedSphereCastRadius, cam.forward, out sphereCastHit, maxSwingDistance, whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward, out raycastHit, maxSwingDistance, whatIsGrappleable);

        Vector3 realHitPoint;

        //Option 1 - Direct Hit
        if (raycastHit.point != Vector3.zero)
        {
            realHitPoint = raycastHit.point;
        }
        //Option 2 - Predicted Hit
        else if (sphereCastHit.point != Vector3.zero)
        {
            realHitPoint = sphereCastHit.point;
        }
        //Option 3 - Miss
        else {
            realHitPoint = Vector3.zero;
        }

        if (realHitPoint != Vector3.zero && activated)
        {
            predictedPoint.gameObject.SetActive(true);
            predictedPoint.position = realHitPoint;
        }
        else {
            predictedPoint.gameObject.SetActive(false);
        }

        predictedHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }
}
