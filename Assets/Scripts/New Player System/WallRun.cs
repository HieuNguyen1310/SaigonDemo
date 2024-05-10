using UnityEngine;

public class WallRun : MonoBehaviour
{
    public float wallRunSpeed = 15f;
    public float wallMaxDistance = 1f;
    public string wallTag = "Wall"; 
    // public float minimumHeight = 1.5f; 
    public float downwardRaycastDistance = 3f; // How far to raycast downwards


    public Transform orientation;
    public PlayerMovement pm;
    public CharacterController controller;

    public bool allowAirControl; 
    public float wallGravityScale = 0.5f; 
    public float wallRunCameraTilt = 10f;
    public float timeToTilt = 0.2f; 

    private float tilt;
    private bool _isWallRight, _isWallLeft;
    private RaycastHit _rightWallHit, _leftWallHit;

    void Start()
    {
        orientation = GetComponent<Transform>(); 
        pm = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        CheckForWall(); 
        if (CanWallRun())
        {
            if (_isWallRight) WallRunning(true);
            else if (_isWallLeft) WallRunning(false);
            Debug.Log(CanWallRun());
        }
        // Debug.Log("Udating");
        // Debug.Log(!pm.isGround);
        // Debug.Log(CanWallRun());
        // Debug.Log(transform.position.y);
    }

    void CheckForWall()
    {
        _isWallRight = Physics.Raycast(transform.position, orientation.right, out _rightWallHit, wallMaxDistance);
        _isWallLeft = Physics.Raycast(transform.position, -orientation.right, out _leftWallHit, wallMaxDistance);

        if (_isWallRight && _rightWallHit.collider.CompareTag(wallTag)) 
        {
            Debug.DrawRay(transform.position, orientation.right * wallMaxDistance, Color.red);
            Debug.Log("Right Wall");
        } else {
            Debug.DrawRay(transform.position, orientation.right * wallMaxDistance, Color.green);
        }

        if (_isWallLeft && _leftWallHit.collider.CompareTag(wallTag)) 
        {
            Debug.DrawRay(transform.position, -orientation.right * wallMaxDistance, Color.red);
            Debug.Log("Left Wall");
        } else {
            Debug.DrawRay(transform.position, -orientation.right * wallMaxDistance, Color.green); 
        }
    }

    bool CanWallRun()
    {
        // return !pm.isGround && (_isWallLeft || _isWallRight) && transform.position.y > minimumHeight;
        
        // return !pm.isGround && (_isWallLeft && _leftWallHit.collider.CompareTag(wallTag)|| _isWallRight && _rightWallHit.collider.CompareTag(wallTag)) && transform.position.y > minimumHeight;
        
        // Check if there's a ground within the downward raycast distance
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, downwardRaycastDistance);

        return !pm.isGround && (_isWallLeft || _isWallRight) && isGrounded; 
    }

    // ReSharper disable Unity.PerformanceAnalysis
    void WallRunning(bool isWallRight)
    {
        Vector3 wallNormal = isWallRight ? _rightWallHit.normal : _leftWallHit.normal;

        Vector3 wallForce = Vector3.up * wallRunSpeed + wallNormal * 5f; 
        Vector3 movement = wallForce * Time.deltaTime;
        controller.Move(movement);

        tilt = Mathf.Lerp(tilt, isWallRight ? wallRunCameraTilt : -wallRunCameraTilt, timeToTilt * Time.deltaTime);
        orientation.localRotation = Quaternion.Euler(0, 0, tilt);
        
        // Debug.Log(tilt);

        if (allowAirControl)
        {
            Vector3 move = orientation.forward * Input.GetAxis("Vertical"); 
            if (isWallRight) move += orientation.right * Input.GetAxis("Horizontal");
            else move += -orientation.right * Input.GetAxis("Horizontal"); 
            move *= wallRunSpeed * .1f;
            controller.Move(move * Time.deltaTime);
        }

        // pm.enabled = false; // Disable standard gravity in PlayerMovement
        if (!controller.isGrounded)
        {
            controller.Move(Vector3.up * (-pm.gravity * wallGravityScale * Time.deltaTime));
        }
    }
}
