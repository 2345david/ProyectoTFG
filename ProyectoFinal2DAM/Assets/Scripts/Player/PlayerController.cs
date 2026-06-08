using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[DefaultExecutionOrder(100)]
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

    [Header("Abilities")]
    public bool hasDoubleJump = false;

    private PlayerLadderMovement _ladderMovement;

    public static PlayerController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        // Freeze player physics initially to prevent falling through world while loading
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }
    }

    public void FreezePlayer()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.simulated = false;
        canMove = false;
    }

    public void UnfreezePlayer()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.simulated = true;
        canMove = true;
    }

    public bool hasDash = false;
    private bool canDoubleJump = false;
    
    void Start()
    {
        defaultSpeed = speed;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        _ladderMovement = GetComponent<PlayerLadderMovement>();
    }
    public float doubleJumpManaCost = 10f;

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        if (isGrounded)
        {
            animator.SetBool("Jump", false);
            canDoubleJump = true;
        }
        else
        {
            animator.SetBool("Jump", true);
        }
        
        FlipCharacter();
        Attack();
        HandleJumpInput();
        HandleDashInput();
    }
    public float dashManaCost = 20f;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float dashTimeRemaining;
    private float lastDashTime;
    private float defaultSpeed;

    public void ResetController()
    {
        UnfreezePlayer();
        if (defaultSpeed > 0) speed = defaultSpeed;
        isDashing = false;
        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.linearVelocity = Vector2.zero;
        }
        canDoubleJump = true;
        animator.SetBool("Walk", false);
        animator.SetBool("Jump", false);
        animator.SetBool("Attack", false);
    }

    private void JumpAction()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
    }

    private void HandleJumpInput()
    {
        if (!canMove || isDashing) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                JumpAction();
            }
            else if (hasDoubleJump && canDoubleJump)
            {
                if (PlayerMana.instance != null && PlayerMana.instance.HasEnoughMana(doubleJumpManaCost))
                {
                    JumpAction();
                    canDoubleJump = false;
                    PlayerMana.instance.UseMana(doubleJumpManaCost);
                }
            }
}
    }

    private void HandleDashInput()
    {
        if (!canMove || !hasDash || isDashing) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.leftShiftKey.wasPressedThisFrame && Time.time >= lastDashTime + dashCooldown)
        {
            if (PlayerMana.instance != null && PlayerMana.instance.HasEnoughMana(dashManaCost))
            {
                StartDash();
                PlayerMana.instance.UseMana(dashManaCost);
            }
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimeRemaining = dashDuration;
        lastDashTime = Time.time;
        
        float dashDir = transform.localScale.x; // Use scale to determine facing direction
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0);
        rb.gravityScale = 0; // Prevent falling during dash
        
        // You might want to add a dash animation here
        // animator.SetTrigger("Dash");
    }

    private void Movment()
    {
        if (isDashing) return;

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
        if (isDashing)
        {
            dashTimeRemaining -= Time.fixedDeltaTime;
            if (dashTimeRemaining <= 0)
            {
                EndDash();
            }
            return;
        }

        Movment();
        if (canMove)
        {
            // rb.linearVelocity logic is now in FlipCharacter and Movement logic is a bit split.
            // Original code had Jump() in FixedUpdate, but using wasPressedThisFrame in Update is better.
        }
    }

    private void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 1; // Assuming default gravity is 1
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void Attack()
    {
        if (isDashing) return;
        
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (mouse.leftButton.isPressed)
        {
            animator.SetBool("Attack", true);
            if (AudioManager.instance != null)
                AudioManager.instance.PlayAudio(AudioManager.instance.hit);
        }
        else
        {
            animator.SetBool("Attack", false);
        }
    }

    private IEnumerator DropDown()
    {
        // Find the platform we are standing on
        Collider2D[] results = new Collider2D[5];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(whatIsGround);
        int count = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, filter, results);

        for (int i = 0; i < count; i++)
        {
            if (results[i].GetComponent<PlatformEffector2D>() != null)
            {
                Collider2D platformCollider = results[i];
                Collider2D playerCollider = GetComponent<Collider2D>();
                
                Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
                yield return new WaitForSeconds(0.5f);
                Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
                break;
            }
        }
    }
    
    // Removing old Jump and keeping the rest logic
    
    private void FlipCharacter()
    {
        if (canMove && !isDashing)
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
