using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")] 
    private PlayerMovementThree _pm;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    public LineRenderer lr;

    public PlayerCam camEffect;

    [Header("Grappling")] 
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private Vector3 _grapplePoint;

    [Header("Cooldown")] 
    public float grapplingCd;
    private float _grapplingCdTimer;

    [Header("Input")] 
    public KeyCode grappleKey = KeyCode.Mouse1;

    private bool _grappling;

    private void Start()
    {
        _pm = GetComponent<PlayerMovementThree>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(grappleKey)) StartGrapple();

        if (_grapplingCdTimer > 0)
            _grapplingCdTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (_grappling)
            lr.SetPosition(0, gunTip.position);
    }

    private void StartGrapple()
    {
        if (_grapplingCdTimer > 0) return;

        _grappling = true;

        RaycastHit hit;

        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            _grapplePoint = hit.point;
            
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            _grapplePoint = cam.position + cam.forward * maxGrappleDistance;
            
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lr.enabled = true;
        lr.SetPosition(1, _grapplePoint);
    }

    private void ExecuteGrapple()
    {
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        float grapplePointRelativeYPos = _grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;
        
        _pm.JumpToPosition(_grapplePoint, highestPointOnArc);
        
        Invoke(nameof(StopGrapple), 1f);
        
        //Cam effect
        camEffect.DoFov(100f);
    }

    public void StopGrapple()
    {
        _grappling = false;

        _grapplingCdTimer = grapplingCd;


        lr.enabled = false; 
        
        //Cam eff
        camEffect.DoFov(85f);
    }

    public bool IsGrappling()
    {
        return _grappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return _grapplePoint;
    }
    
}
