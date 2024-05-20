using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")] 
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody _rb;
    private PlayerMovementThree _pm;

    [Header("Sliding")] 
    public float maxSlideTime;
    public float slideForce;
    private float _slideTimer;

    public float slideYScale;
    private float _startYScale;

    [Header("Input")] 
    public KeyCode slideKey = KeyCode.LeftAlt;
    private float _hoInput;
    private float _vertInput;

    // private bool _isSliding;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _pm = GetComponent<PlayerMovementThree>();

        _startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        _hoInput = Input.GetAxisRaw("Horizontal");
        _vertInput = Input.GetAxisRaw("Vertical");
        
        if(Input.GetKeyDown(slideKey) && (_hoInput != 0 || _vertInput != 0))
            StartSlide();
        
        if(Input.GetKeyUp(slideKey) && _pm.isSliding)
            StopSlide();
    }

    private void StartSlide()
    {
        _pm.isSliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        _slideTimer = maxSlideTime;
    }

    private void FixedUpdate()
    {
        if (_pm.isSliding)
            SlidingMovement();
    }

    private void SlidingMovement()
    {
        Vector3 inputDir = orientation.forward * _vertInput + orientation.right * _hoInput;
        
        // sliding normal
        if (!_pm.OnSlope() || _rb.velocity.y > -.1f)
        {
            _rb.AddForce(inputDir.normalized * slideForce, ForceMode.Force);

            _slideTimer -= Time.deltaTime;
        }
        
        // sliding down a slope
        else
        {
            _rb.AddForce(_pm.GetSlopeMoveDir(inputDir) * slideForce,ForceMode.Force);
        }
        
        if (_slideTimer <= 0)
            StopSlide();
    }
    
    private void StopSlide()
    {
        _pm.isSliding = false;
        
        playerObj.localScale = new Vector3(playerObj.localScale.x, _startYScale, playerObj.localScale.z);
    }
    
    
    
    
    
    
    
    
    
}
