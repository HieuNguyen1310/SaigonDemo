using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementThree : MonoBehaviour
{
    [Header("Movement")] 
    private float moveSpeed;

    public float walkSpeed;
    public float sprintSpeed;

    public float dashSpeed;
    public float dashSpeedChangeFactor;

    public float maxYSpeed;

    public float groundDrag;

    public float slideSpeed;

    public float wallRunSpeed;

    public float climbSpeed;
    
    private float _desiredMoveSpeed;
    private float _lastDesiredMoveSpeed;
    private MovementState _lastState;

    [Header("Jump")] 
    public float jumpForce;
    public float jumpCooldown;
    public float airMuliplier;
    private bool _readyToJump;

    [Header("Crouching")] 
    public float crouchSpeed;
    public float crouchYScale;
    private float _startYScale;

    [Header("KeyBinds")] 
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")] 
    public float playerHeight;
    public LayerMask ground;
    public bool grounded;

    [Header("Slope Handling")] 
    public float maxSlopeAngle;
    private RaycastHit _slopeHit;
    private bool _excitingSlope;

    [Header("References")] 
    // public Climbing climbingScript;
    
    [Header("Orientation")]
    public Transform orientation;

    private float _hoInput;
    private float _vertInput;

    private Vector3 _moveDir;

    Rigidbody _rb;

    public MovementState state;
    public enum MovementState
    {   
        Freeze,
        Unlimited,
        Restricted,
        Walking,
        Sprinting,
        Dashing,
        Wallrunning,
        Climbing,
        Crouching,
        Sliding,
        Air
    }

    public bool isFreeze;
    public bool isUnlimited;

    public bool activeGrapple;
    
    public bool isRestricted; // no wasd movement
    public bool isSliding;

    public bool isDashing;
    public bool isWallrunning;
    public bool isClimbing;
    

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        _readyToJump = true;

        _startYScale = transform.localScale.y;
    }

    private void Update()
    {      
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * .5f + .2f, ground);
        
        MyInput();
        SpeedControl();
        StateHandler();
        
        // handle drag
        // if (grounded)
        if (state == MovementState.Walking && !activeGrapple ||
            state == MovementState.Sprinting && !activeGrapple || 
            state == MovementState.Crouching && !activeGrapple)
            _rb.drag = groundDrag;
        else
            _rb.drag = 0;
    }

    private void FixedUpdate()
    {   
        if (state != MovementState.Restricted)
            MovePlayer();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void MyInput()
    {
        _hoInput = Input.GetAxisRaw("Horizontal");
        _vertInput = Input.GetAxisRaw("Vertical");
        
        //when to JUMP
        if (Input.GetKey(jumpKey) && _readyToJump && grounded)
        {
            _readyToJump = false;
            
            Jump();
            
            Invoke(nameof(ResetJump), jumpCooldown);
            
            // Debug.Log("JUMP");
        }
        
        
        
        // Start crouching
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        
        //Stop Crouching
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, _startYScale, transform.localScale.z);
        }
    }

    private bool _keepMomentum;
    private void StateHandler()
    {   
        //Mode - Freeze
        if (isFreeze)
        {
            state = MovementState.Freeze;
            _rb.velocity = Vector3.zero;
            _desiredMoveSpeed = 0f;
        }
        
        //Mode - Unlimited
        else if (isUnlimited)
        {
            state = MovementState.Unlimited;
            _desiredMoveSpeed = 999f;
            return;
        }
        
        
        
        //Mode - Dashing
        else if (isDashing)
        {
            state = MovementState.Dashing;
            _desiredMoveSpeed = dashSpeed;
            _speedChangeFactor = dashSpeedChangeFactor;
        }
        
        //Mode - Climbing
        else if (isClimbing)
        {
            state = MovementState.Climbing;
            _desiredMoveSpeed = climbSpeed;
        }
        
        //Mode - Wall Running
        else if (isWallrunning)
        {
            state = MovementState.Wallrunning;
            _desiredMoveSpeed = wallRunSpeed;
        }
        
        //Mode - Sliding
        else if (isSliding)
        {
            state = MovementState.Sliding;

            if (OnSlope() && _rb.velocity.y < .1f)
            {
                _desiredMoveSpeed = slideSpeed;
                _keepMomentum = true;
            }
            else
                _desiredMoveSpeed = sprintSpeed;
           
        }
        
        //Mode - Crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.Crouching;
            _desiredMoveSpeed = crouchSpeed;
        }
        
        //Mode - sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.Sprinting;
            _desiredMoveSpeed = sprintSpeed;
        }
        
        //Mode - walking
        else if (grounded)
        {
            state = MovementState.Walking;
            _desiredMoveSpeed = walkSpeed;
        }
        
        //Mode - Air
        else
        {
            state = MovementState.Air;

            if (_desiredMoveSpeed < sprintSpeed)
                _desiredMoveSpeed = moveSpeed;
            else
                _desiredMoveSpeed = sprintSpeed;
        }
    
        // check if desire move speed has changed drastically
        bool desireMoveSpeedHasChanged = _desiredMoveSpeed != _lastDesiredMoveSpeed;
        if (_lastState == MovementState.Dashing) _keepMomentum = true;
        
        if (desireMoveSpeedHasChanged)
        {
            if (_keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());    
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = _desiredMoveSpeed;
            }
            
        }
        
        _lastDesiredMoveSpeed = _desiredMoveSpeed;
        _lastState = state;
        
        //Deactivate keep Momentum
        if (Mathf.Abs(_desiredMoveSpeed - moveSpeed) < .1f) _keepMomentum = false;
    }

    private float _speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desire value
        float time = 0;
        float difference = Mathf.Abs(_desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = _speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, _desiredMoveSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = _desiredMoveSpeed;
        _speedChangeFactor = 1f;
        _keepMomentum = false;
    }
    
    private void MovePlayer()
    {   
        
        if(activeGrapple) return;
        
        if (isRestricted) return;
        
        if(state == MovementState.Dashing) return;
        
        // calculate move dir
        _moveDir = orientation.forward * _vertInput + orientation.right * _hoInput;
        
        // on slope
        if (OnSlope() && !_excitingSlope)
        { 
            _rb.AddForce(GetSlopeMoveDir(_moveDir) * (moveSpeed * 40f), ForceMode.Force);
            
            if(_rb.velocity.y > 0)
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        
        // on ground
        if(grounded)
            _rb.AddForce(_moveDir.normalized * (moveSpeed * 10f), ForceMode.Force);
        
        //in air
        else if(!grounded)
            _rb.AddForce(_moveDir.normalized * (moveSpeed * 10f * airMuliplier), ForceMode.Force);
        
        //turn gravity off while on slope
        if (!isWallrunning) _rb.useGravity = !OnSlope();

    }

    private void SpeedControl()
    {
        if(activeGrapple) return;
        
        //limit speed on slope
        if (OnSlope() && !_excitingSlope)
        {
            if (_rb.velocity.magnitude > moveSpeed)
                _rb.velocity = _rb.velocity.normalized * moveSpeed;
        }
        
        //limit speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        
            // limit vel
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
            }
        }
        
        //limit y vel
        if (maxYSpeed != 0 && _rb.velocity.y > maxYSpeed)
            _rb.velocity = new Vector3(_rb.velocity.x, maxYSpeed, _rb.velocity.z);

    }

    private void Jump()
    {
        _excitingSlope = true;
        
        // reset y velo
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        
        // Debug.Log("JUMP");
        
    }

    private void ResetJump()
    {
        _readyToJump = true;
        
        _excitingSlope = false;
    }

    

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, playerHeight * .5f + .3f))
        {
            float angle = Vector3.Angle(Vector3.up,_slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDir(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;
    }

    private bool _enableMovementOnNextTouch;
    
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;
        
        _velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        
        Invoke(nameof(SetVelocity), 0.1f);
        
        Invoke(nameof(ResetRestriction), 3f);
    }

    private Vector3 _velocityToSet;

    private void SetVelocity()
    {
        _enableMovementOnNextTouch = true;
        _rb.velocity = _velocityToSet;
    }

    public void ResetRestriction()
    {
        activeGrapple = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_enableMovementOnNextTouch)
        {
            _enableMovementOnNextTouch = false;
            ResetRestriction();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;

        float displacementY = endPoint.y - startPoint.y;

        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);
        
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2  * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY; 
    }
    
    
}
