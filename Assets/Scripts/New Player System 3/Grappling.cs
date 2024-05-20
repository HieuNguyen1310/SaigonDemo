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

    [Header("Grappling")] 
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overShootYAxis;

    private Vector3 _grapplePoint;

    [Header("Cooldown")] 
    public float grapplingCd;
    private float _grapplingCdTimer;

    [Header("Input")] 
    public KeyCode grappleKey = KeyCode.Mouse1;

    private bool _isGrappling;

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
        if(_isGrappling)
            lr.SetPosition(0, gunTip.position);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void StartGrapple()
    {
        if (_grapplingCdTimer > 0) return;

        _isGrappling = true;

        // _pm.isFreeze = true;

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
        // _pm.isFreeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = _grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overShootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overShootYAxis;
        
        // _pm.JumpToPosition(_grapplePoint, highestPointOnArc);
        
        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        // _pm.isFreeze = false;
        
        _isGrappling = false;

        _grapplingCdTimer = grapplingCd;

        lr.enabled = false;
    }

    public bool IsGrappling()
    {
        return _isGrappling;
    }

    public Vector3 GetGrapplingPoint()
    {
        return _grapplePoint;
    }
    
    
}
