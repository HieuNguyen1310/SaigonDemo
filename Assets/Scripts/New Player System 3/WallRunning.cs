using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")] 
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpForce;
    public float wallJumpSideForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float _wallRunTimer;

    [Header("Input")] 
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool _upwardsRunning;
    private bool _downwardsRunning;
    private float _hoInput;
    private float _vertInput;

    [Header("Detection")] 
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit _leftWallHit;
    private RaycastHit _rightWallHit;
    private bool _wallLeft;
    private bool _wallRight;

    [Header("Limitations")] 
    // public bool doJumpOnEndOfTimer = false;
    // public bool resetDblJumpOnNewWall = true;
    // public bool resetDblJumpOnEveryWall = false;
    public int allowedWallJumps = 1;

    // private bool _wallRemembered;
    // private Transform _lastWall;

    // private int _wallJumpDone;
    

    [Header("Exiting Wall")] 
    private bool _exitWall;
    private float _exitWallTimer;
    public float exitWallTime;

    [Header("Gravity")] 
    public bool useGravity;
    [Range(0f, 10f)]
    public float gravityCounterForce;

    [Header("References")] 
    public Transform orientation;
    public PlayerCam cam;
    private PlayerMovementThree _pm;
    private Rigidbody _rb;
    private LedgeGrabbing _lg;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _pm = GetComponent<PlayerMovementThree>();

        _lg = GetComponent<LedgeGrabbing>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();

        // if (_pm.grounded && _lastWall != null)
        //     _lastWall = null;

    }

    private void FixedUpdate()
    {
        if(_pm.isWallrunning && !_exitWall)
            WallRunMovement();
    }

    private void CheckForWall()
    {
        _wallRight = Physics.Raycast(transform.position, orientation.right, out _rightWallHit, wallCheckDistance,whatIsWall);
        _wallLeft = Physics.Raycast(transform.position, -orientation.right, out _leftWallHit, wallCheckDistance,whatIsWall);

        // if ((_wallLeft || _wallRight))
        // {
        //     // _wallJumpDone = 0;
        //     _wallRunTimer = maxWallRunTime;
        // }
        
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        //Getting Inputs
        _hoInput = Input.GetAxisRaw("Horizontal");
        _vertInput = Input.GetAxisRaw("Vertical");

        _upwardsRunning = Input.GetKey(upwardsRunKey);
        _downwardsRunning = Input.GetKey(downwardsRunKey);
        
        //State 1 - Wallrunning
        if ((_wallLeft || _wallRight) && _vertInput > 0 && AboveGround() && !_exitWall) 
        {
            if(!_pm.isWallrunning)
                StartWallRun();
            
            //Wall run timer
            if (_wallRunTimer > 0)
                _wallRunTimer -= Time.deltaTime;

            if (_wallRunTimer <= 0 && _pm.isWallrunning)
            {
                _exitWall = true;
                _exitWallTimer = exitWallTime;
                
            }
            
            //Wall Jump
            if (Input.GetKeyDown(jumpKey)) WallJump();
        }
        
        //State 2 - Exiting Wall
        else if (_exitWall)
        {
            // _pm.isRestricted = true;
            
            if(_pm.isWallrunning)
                StopWallRun();
        
            if (_exitWallTimer > 0)
                _exitWallTimer -= Time.deltaTime;
        
            if (_exitWallTimer <= 0)
                _exitWall = false;
        }
        
        //State 3 - None
        else
        {
            if (_pm.isWallrunning)
            {
                StopWallRun();
            }
        }

        if (!_exitWall && _pm.isRestricted)
            _pm.isRestricted = false;

    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void StartWallRun()
    {
        _pm.isWallrunning = true;

        _wallRunTimer = maxWallRunTime;
        
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        // _wallRemembered = false;

        // _rb.useGravity = useGravity;
        
        
        
        //Apply camera effects (Tilting)
        cam.DoFov(90f);
        if (_wallLeft) cam.DoTilt(-5f);
        if (_wallRight) cam.DoTilt(5f);
    }
    
    private void WallRunMovement()
    {
        //Set gravity
        _rb.useGravity = useGravity;
        
        //Calc dir
        Vector3 wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;
        
        //Forward force
        _rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
        
        //upwards/downwards force
        if (_upwardsRunning)
            _rb.velocity = new Vector3(_rb.velocity.x, wallClimbSpeed, _rb.velocity.z);
        if (_downwardsRunning)
            _rb.velocity = new Vector3(_rb.velocity.x, -wallClimbSpeed, _rb.velocity.z);
        
        // push to wall force
        if(!(_wallLeft && _hoInput > 0 ) && !(_wallRight && _hoInput < 0))
            _rb.AddForce(-wallNormal * 100f, ForceMode.Force);
        
        //weaken gravity
        if(useGravity)
            _rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        
        // //Remember the last wall
        // if (!_wallRemembered)
        // {   
        //     // RememberLastWall();
        //     _wallRemembered = true;
        // }
            
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private void StopWallRun()
    {
        _pm.isWallrunning = false;
        // _rb.useGravity = true;
         
        //reset cam effect
        cam.DoFov(80f);
        cam.DoTilt(0f);
    }

    private void WallJump()
    {
        // bool firstJump = true;
        
        if(_lg.holding || _lg.exitingLedge) return;
        
        // Enter exiting wall state
        _exitWall = true;
        _exitWallTimer = exitWallTime;
        
        Vector3 wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;
        
        Vector3 forceToApply = transform.up * wallJumpForce + wallNormal * wallJumpSideForce;

        // Vector3 forceToApply = new Vector3();
        //
        // if (_wallLeft)
        //     forceToApply = transform.up * wallJumpForce + _leftWallHit.normal * wallJumpSideForce;
        //
        // else if (_wallRight)
        //     forceToApply = transform.up * wallJumpForce + _rightWallHit.normal * wallJumpSideForce;
        //
        // firstJump = _wallJumpDone < allowedWallJumps;
        // _wallJumpDone++;
        //
        // //if not 1sst jump, remove y force
        // if (!firstJump)
        //     forceToApply = new Vector3(forceToApply.x, 0f, forceToApply.z);
        
        //Reset y velo & Add force
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _rb.AddForce(forceToApply, ForceMode.Impulse);
        
        // RememberLastWall();
        
        StopWallRun();
    }

    // private void RememberLastWall()
    // {
    //     if (_wallLeft)
    //         _lastWall = _leftWallHit.transform;
    //
    //     if (_wallRight)
    //         _lastWall = _rightWallHit.transform;
    // }
    //
    // private bool NewWallHit()
    // {
    //     if (_lastWall == null)
    //         return true;
    //
    //     if (_wallLeft && _leftWallHit.transform != _lastWall)
    //         return true;
    //     
    //     else if (_wallRight && _rightWallHit.transform != _lastWall)
    //         return true;
    //
    //     return true;
    // }
    
    
    
}
