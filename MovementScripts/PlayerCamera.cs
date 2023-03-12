using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform orientation, playerObj, player;
    [SerializeField] Rigidbody rb;

    [SerializeField] float rotationSpeed;

    public CameraType currentStyle;

    [SerializeField] Transform grapplingLook;

    public enum CameraType { 
        Basic,
        Grappling
    }
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector3 viewDirection = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDirection.normalized;

        if (currentStyle == CameraType.Basic)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDirection != Vector3.zero)
            {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDirection.normalized, Time.deltaTime * rotationSpeed);
            }
        }
        else if (currentStyle == CameraType.Grappling) {
            Vector3 grapplingLookAt = grapplingLook.position - new Vector3(transform.position.x, grapplingLook.position.y, transform.position.z);
            orientation.forward = grapplingLookAt.normalized;

            playerObj.forward = grapplingLookAt.normalized;
        }
    }

    public void DoFov(float endValue) {
        GetComponent<Camera>().DOFieldOfView(endValue,0.25f);
    }
}
