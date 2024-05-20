using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")] 
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody _rb;
    private PlayerMovementThree _pm;

    [Header("Dashing")] 
    public float dashForce;
    public float dashUpwardForce;
    public float maxDashYSpeed;
    public float dashDuration;

    [Header("Camera Effects")] 
    public PlayerCam cam;
    public float dashFOV;

    [Header("Settings")] 
    public bool useCameraForward;
    public bool allowedAllDirections;
    public bool disableGravity = false;
    public bool resetVel = true;
    

    [Header("Cooldown")] 
    public float dashCd;
    private float _dashCdTimer;

    [Header("Input")] 
    public KeyCode dashKey = KeyCode.Q;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _pm = GetComponent<PlayerMovementThree>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(dashKey))
            Dash();

        if (_dashCdTimer > 0)
            _dashCdTimer -= Time.deltaTime;
    }

    private void Dash()
    {
        if (_dashCdTimer > 0) return;
        else _dashCdTimer = dashCd;
        
        _pm.isDashing = true;
        _pm.maxYSpeed = maxDashYSpeed;

        Transform forwardT;
        if (useCameraForward)
            forwardT = playerCam;
        else
            forwardT = orientation;

        Vector3 direction = GetDirection(forwardT); 
            
        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity)
            _rb.useGravity = false;
        
        _delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), .025f);
        
        Invoke(nameof(ResetDash), dashDuration);
        
        //Camera Effects
        cam.DoFov(dashFOV);
        
        //Slow down time?
        Time.timeScale = 0.1f;
    }

    private Vector3 _delayedForceToApply;
    
    private void DelayedDashForce()
    {
        if(resetVel)
            _rb.velocity = Vector3.zero;
        
        _rb.AddForce(_delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        _pm.isDashing = false;
        _pm.maxYSpeed = 0;

        if (disableGravity)
            _rb.useGravity = true;
        
        //Reset Cam FOV
        cam.DoFov(85f);
        
        //Reset Time scale
        Time.timeScale = 1f;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float _hoInput = Input.GetAxisRaw("Horizontal");
        float _vertInput = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3();

        if (allowedAllDirections)
            dir = forwardT.forward * _vertInput + forwardT.right * _hoInput;
        else
            dir = forwardT.forward;

        if (_vertInput == 0 && _hoInput == 0f)
            dir = forwardT.forward;

        return dir.normalized;
    }

}
