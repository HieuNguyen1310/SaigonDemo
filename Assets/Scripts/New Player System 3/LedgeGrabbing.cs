using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabbing : MonoBehaviour
{
    [Header("References")] 
    public PlayerMovementThree pm;
    public Transform orientation;
    public Transform cam;
    public Rigidbody rb;
   
    [Header("Ledge Grabbing")] 
    public float moveToLedgeSpeed;
    public float maxLedgeGrabDistance;

    public float minTimeOnLedge;
    private float _timeOnLedge;

    public bool holding;

    [Header("Ledge Jumping")] 
    public KeyCode jumpKey = KeyCode.Space;
    public float ledgeJumpForwardForce;
    public float ledgeJumpUpwardForce;
    

    [Header("Ledge Dectection")] 
    public float ledgeDetectionLength;
    public float ledgeSphereCastRadius;
    public LayerMask whatIsLedge;

    private Transform _lastLedge;
    private Transform _currLedge;

    private RaycastHit _ledgeHit;

    [Header("Exiting")] 
    public bool exitingLedge;
    public float exitLedgeTime;
    private float _exitLedgeTimer;

    

    private void Update()
    {
        LedgeDetection();
        SubStateMachine();
        
        // Debug.Log(pm.isRestricted);
    }

    private void SubStateMachine()
    {
        float hoInput = Input.GetAxisRaw("Horizontal");
        float vertInput = Input.GetAxisRaw("Vertical");

        bool anyInputKeyPressed = hoInput != 0 || vertInput != 0;
        
        //SubState 1 - Holding onto ledgee
        if (holding)
        {
            FreezeRigidbodyOnLedge();

            _timeOnLedge += Time.deltaTime;
            
            if (_timeOnLedge > minTimeOnLedge && anyInputKeyPressed) ExitLedgeHold();
            
            if (Input.GetKeyDown(jumpKey)) LedgeJump();
        }
        
        //SubState 2 - Exiting State
        else if (exitingLedge)
        {
            if (_exitLedgeTimer > 0) _exitLedgeTimer -= Time.deltaTime;
            else exitingLedge = false;
        }
    }
    
    private void LedgeDetection()
    {
        bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out _ledgeHit,
            ledgeDetectionLength, whatIsLedge);
        
        if (!ledgeDetected) return;

        float distanceToLedge = Vector3.Distance(transform.position, _ledgeHit.transform.position);
        
        if (_ledgeHit.transform == _lastLedge) return;
        
        if (distanceToLedge < maxLedgeGrabDistance && !holding) EnterLedgeHold();
    }

    private void LedgeJump()
    {
        ExitLedgeHold();

        Invoke(nameof(DelayedJumpForce), .05f);
    }

    private void DelayedJumpForce()
    {
        Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpwardForce;

        rb.velocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }

    private void EnterLedgeHold()
    {
        holding = true;

        pm.isUnlimited = true;
        pm.isRestricted = true;
        

        _currLedge = _ledgeHit.transform;
        _lastLedge = _ledgeHit.transform;

        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    private void FreezeRigidbodyOnLedge()
    {
        rb.useGravity = false;

        Vector3 dirToLedge = _currLedge.position - transform.position;
        float distanceToLedge = Vector3.Distance(transform.position, _currLedge.position);
        
        //Move player towards ledge
        if (distanceToLedge > 1f)
        {
            if(rb.velocity.magnitude < moveToLedgeSpeed)
                rb.AddForce(dirToLedge.normalized * (moveToLedgeSpeed * 1000f * Time.deltaTime));
        }

        //Hold Onto Ledge
        else
        {
            if (!pm.isFreeze) pm.isFreeze = true;

            if (!pm.isRestricted) pm.isRestricted = true;

            if (pm.isUnlimited) pm.isUnlimited = false;
        }
        
        //Exiting if sth go wrong
        if (distanceToLedge > maxLedgeGrabDistance) ExitLedgeHold();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void ExitLedgeHold()
    {
        exitingLedge = true;
        _exitLedgeTimer = exitLedgeTime;
        
        holding = false;
        _timeOnLedge = 0f;

        pm.isRestricted = false;
        pm.isFreeze = false;
        pm.isUnlimited = false;

        rb.useGravity = true;
        
        StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1f);
    }

    private void ResetLastLedge()
    {
        _lastLedge = null;
    }


}
