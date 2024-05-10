using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public Transform groundCheck;
    public float groundDistance = .45f;
    public LayerMask groundMask;

    private Vector3 _velocity;
    public bool isGround;

    void Update()
    {
        // Ground Check
        isGround = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        Debug.DrawRay(groundCheck.position, -transform.up * groundDistance, Color.red);
        if (isGround && _velocity.y < 0) 
        {
            _velocity.y = -2f;
        }

        // Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * (speed * Time.deltaTime));

        // Jump
        if (Input.GetButtonDown("Jump") && isGround) 
        {   
            Debug.Log("JUMP");
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        

        // Gravity
        _velocity.y += gravity * Time.deltaTime;
        controller.Move(_velocity * Time.deltaTime);
        
        // Debug.Log(_velocity.y);
        
        // Debug.Log(isGround);

        
        
    }
}
;