using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement pm;
    [SerializeField] PlayerCamera pc;
    [SerializeField] Transform cam;
    [SerializeField] CinemachineFreeLook basicCam;
    [SerializeField] Transform hook;
    [SerializeField] LayerMask whatIsGrappleable;
    [SerializeField] LineRenderer lr;

    [Header("Grappling")]
    [SerializeField] float maxGrappleDistance;
    [SerializeField] float grappleDelayTime;
    [SerializeField] float overshootYAxis;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    [SerializeField] float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    [SerializeField] KeyCode activateGrapple = KeyCode.F;
    [SerializeField] KeyCode grappleKey = KeyCode.Mouse1;

    private bool grappling;
    private bool activated;


    // Start is called before the first frame update
    void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(activateGrapple)) {
            activateHook();
        }

        if (Input.GetKeyDown(grappleKey) && activated) {
            StartGrapple();
        }

        if (grapplingCdTimer > 0) {
            grapplingCdTimer -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (grappling) {
            lr.SetPosition(0, hook.position);
        }
    }

    private void activateHook() {
        activated = !activated;
        if (activated)
        {
            Debug.Log("activated");
            pc.currentStyle = PlayerCamera.CameraType.Grappling;
            basicCam.enabled = false;
        }
        else {
            pc.currentStyle = PlayerCamera.CameraType.Basic;
            basicCam.enabled = true;
            Debug.Log("deactivated");
        }
    }

    private void StartGrapple() {
        if (grapplingCdTimer > 0) {
            return;
        }

        //Stop swinging as you grapple
        GetComponent<Swinging>().stopSwing();

        //grappling = true;

        //pm.freeze = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grappling = true;
            pm.freeze = true;
            grapplePoint = hit.point;
            lr.enabled = true;
            lr.SetPosition(1, grapplePoint);
            Invoke(nameof(inGrappling), grappleDelayTime);
            pm.animator.SetBool("isGrappling", true);
        }
        else {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;
            Invoke(nameof(stopGrappling), grappleDelayTime);
        }

        /*lr.enabled = true;
        lr.SetPosition(1, grapplePoint);*/
    }

    private void inGrappling() {
        pm.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y-1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) {
            highestPointOnArc = overshootYAxis;
        }

        pm.JumpTopPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(stopGrappling), 1f);
    }

    public void stopGrappling() {
        pm.freeze = false;

        grappling = false;

        grapplingCdTimer = grapplingCd;

        lr.enabled = false;

        pm.activeGrapple = false;
    }
}
