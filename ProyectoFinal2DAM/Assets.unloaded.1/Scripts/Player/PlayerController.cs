using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed, jumpHeight;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    public Transform groundCheck;
    public bool isGrounded;
    public float groundCheckRadius;
    public LayerMask whatIsGround;
    public Animator animator;
    public bool canMove = true;

    public static PlayerController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
            
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        if (isGrounded)
        {
            animator.SetBool("Jump", false);
        }
        else
        {
            animator.SetBool("Jump", true);
        }
        
        FlipCharacter();
        Attack();
    }

    private void Movment()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return; // No keyboard connected
        }
        
        float moveX = 0f;
        if (keyboard.aKey.isPressed)
        {
            moveX = -1f;
        }
        else if (keyboard.dKey.isPressed)
        {
            moveX = 1f;
        }
        
        moveInput = new Vector2(moveX, 0);

        if (moveInput.x != 0)
        {
            animator.SetBool("Walk", true);
        }
        else
        {
            animator.SetBool("Walk", false);
        }
    }

    private void FixedUpdate()
    {
        Movment();
        if (canMove)
        {
            Jump();
        }
    }

    public void Attack()
    {
        
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (mouse.leftButton.isPressed)
        {
            animator.SetBool("Attack", true);
        }
        else
        {
            animator.SetBool("Attack", false);

        }
    }
    
    public void Jump()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return; // No keyboard connected
        }

        if (keyboard.spaceKey.isPressed && isGrounded) 
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
        }
    }
    

    private void FlipCharacter()
    {
        if (canMove)
        {
            rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
            
            if (moveInput.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (moveInput.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }
}
