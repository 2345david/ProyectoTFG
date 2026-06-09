using UnityEngine;
using UnityEngine.InputSystem; // Sirve para leer el teclado y el ratón con el sistema nuevo de Unity.

/// <summary>
/// Este es el script principal que controla al jugador en un juego de plataformas en 2D.
/// Se encarga de moverlo, saltar (y doble salto), hacer dash (impulso rápido),
/// saber si está en el suelo, voltear el dibujo según hacia dónde va y atacar.
/// </summary>
[DefaultExecutionOrder(100)] // Hace que este script arranque después de otros, para que todo esté listo.
public class PlayerController : MonoBehaviour
{
    public float speed, jumpHeight;     // Velocidad al caminar y fuerza al saltar.
    private Rigidbody2D rb;              // El cuerpo físico del jugador (lo que hace que se mueva con fuerzas).
    private Vector2 moveInput;           // Hacia dónde quiere moverse el jugador ahora mismo.
    public Transform groundCheck;       // Punto debajo de los pies para comprobar si toca el suelo.
    public bool isGrounded;             // Vale "true" si el jugador está tocando el suelo.
    public float groundCheckRadius;     // Tamaño del círculo que usamos para detectar el suelo.
    public LayerMask whatIsGround;      // Qué cosas cuentan como "suelo".
    public Animator animator;           // El que reproduce las animaciones (caminar, saltar, atacar).
    public bool canMove = true;         // Si vale "true" el jugador puede moverse; si no, está bloqueado.

    [Header("Abilities")]
    public bool hasDoubleJump = false;  // Indica si ya se desbloqueó la habilidad de doble salto.

    [Header("Attack Settings")]
    public float attackRate = 0.5f;     // Cuánto hay que esperar entre golpe y golpe (en segundos).
    private float nextAttackTime = 0f;  // Momento a partir del cual ya se puede volver a atacar.

    // "instance" es un atajo para que otros scripts puedan usar al jugador desde cualquier sitio.
    public static PlayerController instance;

    // Esto se ejecuta al crearse el objeto. Guardamos el atajo y congelamos la física para no caer al cargar.
    private void Awake()
    {
        // Guardamos el atajo "instance" solo si no había uno antes.
        if (instance == null)
        {
            instance = this;
        }
        
        // Apagamos la física al principio para que el jugador no se caiga del mundo mientras carga la escena.
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }
    }

    // Congela al jugador: apaga la física, lo bloquea y para su movimiento y animaciones.
    public void FreezePlayer()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.simulated = false;   // Apagamos la física para que no se mueva.
        canMove = false;        // Bloqueamos el control.
        rb.linearVelocity = Vector2.zero;
        moveInput = Vector2.zero;
        
        // Apagamos las animaciones de caminar y saltar.
        if (animator != null)
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Jump", false);
        }
    }

    // Descongela al jugador: vuelve a encender la física, le devuelve el control y recupera su velocidad.
    public void UnfreezePlayer()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.simulated = true;    // Encendemos otra vez la física.
        canMove = true;         // Le devolvemos el control al jugador.
        rb.linearVelocity = Vector2.zero;
        moveInput = Vector2.zero;
        
        // Si la velocidad había quedado en 0 (por ejemplo en una pelea de jefe), la recuperamos.
        if (speed <= 0 && defaultSpeed > 0)
        {
            speed = defaultSpeed;
        }
    }

    public bool hasDash = false;        // Indica si ya se desbloqueó la habilidad de dash.
    private bool canDoubleJump = false; // Vale "true" mientras todavía se pueda hacer el doble salto en el aire.
    
    // Esto se ejecuta una vez al empezar. Buscamos componentes y guardamos la velocidad inicial.
    void Start()
    {
        defaultSpeed = speed;                   // Guardamos la velocidad inicial para poder recuperarla después.
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    public float doubleJumpManaCost = 10f;  // Cuánto maná cuesta el doble salto.

    // Esto se repite muchas veces por segundo. Mira si está en el suelo, ajusta animaciones y revisa acciones.
    void Update()
    {
        // Miramos si hay suelo justo debajo de los pies del jugador.
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        if (isGrounded)
        {
            // Si está en el suelo: quitamos la animación de salto y le devolvemos el doble salto.
            animator.SetBool("Jump", false);
            canDoubleJump = true;
        }
        else
        {
            // Si está en el aire: ponemos la animación de salto.
            animator.SetBool("Jump", true);
        }
        
        // Revisamos todas las acciones del jugador en este momento.
        FlipCharacter();
        Attack();
        HandleJumpInput();
        HandleDashInput();
    }
    public float dashManaCost = 20f;    // Cuánto maná cuesta hacer un dash.

    [Header("Dash Settings")]
    public float dashSpeed = 8f;        // Lo rápido que se mueve el jugador durante el dash.
    public float dashDuration = 0.2f;   // Cuántos segundos dura el dash.
    public float dashCooldown = 1f;     // Cuánto hay que esperar entre un dash y otro.
    private bool isDashing = false;     // Vale "true" mientras el jugador está haciendo el dash.
    private float dashTimeRemaining;    // Tiempo que le queda al dash actual.
    private float lastDashTime;         // Cuándo se hizo el último dash (para controlar la espera).
    private float defaultSpeed;         // Velocidad inicial guardada para poder recuperarla.

    // Deja al jugador como nuevo (se usa al revivir): lo descongela, recupera velocidad, gravedad y animaciones.
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
    }

    // Hace saltar al jugador: le da impulso hacia arriba sin cambiar su movimiento de lado.
    private void JumpAction()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
    }

    // Mira si se aprieta la tecla de salto: salta normal en el suelo, o doble salto en el aire si hay maná.
    private void HandleJumpInput()
    {
        // Si no puede moverse o está en dash, no saltamos.
        if (!canMove || isDashing) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return; // No hay teclado conectado.

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                // Está en el suelo: salto normal.
                JumpAction();
            }
            else if (hasDoubleJump && canDoubleJump)
            {
                // Está en el aire: doble salto, pero solo si tiene maná suficiente.
                if (PlayerMana.instance != null && PlayerMana.instance.HasEnoughMana(doubleJumpManaCost))
                {
                    JumpAction();
                    canDoubleJump = false;                  // Ya usó el doble salto, no puede repetirlo hasta tocar suelo.
                    PlayerMana.instance.UseMana(doubleJumpManaCost);
                }
            }
}
    }

    // Mira si se aprieta la tecla de dash y lo empieza si está desbloqueado, ya pasó la espera y hay maná.
    private void HandleDashInput()
    {
        // Solo seguimos si puede moverse, tiene la habilidad de dash y no está ya haciendo uno.
        if (!canMove || !hasDash || isDashing) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Si apretó Shift izquierdo y ya pasó suficiente tiempo desde el último dash.
        if (keyboard.leftShiftKey.wasPressedThisFrame && Time.time >= lastDashTime + dashCooldown)
        {
            if (PlayerMana.instance != null && PlayerMana.instance.HasEnoughMana(dashManaCost))
            {
                StartDash();
                PlayerMana.instance.UseMana(dashManaCost);
            }
        }
    }

    // Empieza el dash: marca el tiempo, lanza al jugador rápido hacia donde mira y le quita la gravedad un momento.
    private void StartDash()
    {
        isDashing = true;
        dashTimeRemaining = dashDuration;
        lastDashTime = Time.time;
        
        float dashDir = transform.localScale.x; // Usamos hacia dónde mira el jugador para saber la dirección.
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0);
        rb.gravityScale = 0; // Quitamos la gravedad para que no se caiga durante el dash.
        
        // Aquí se podría poner una animación de dash si quisieras.
        // animator.SetTrigger("Dash");
    }

    // Lee las teclas A (izquierda) y D (derecha) para saber hacia dónde se mueve y enciende la animación de caminar.
    private void Movment()
    {
        if (isDashing) return; // Mientras hace dash no usamos el movimiento normal.

        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return; // No hay teclado conectado.
        }
        
        // Vemos qué tecla está apretada para decidir la dirección.
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

        // Si se está moviendo, ponemos la animación de caminar; si no, la quitamos.
        if (moveInput.x != 0)
        {
            animator.SetBool("Walk", true);
        }
        else
        {
            animator.SetBool("Walk", false);
        }
    }

    // Esto lo usa Unity para la física. Controla el tiempo del dash y, si no hay dash, lee el movimiento.
    private void FixedUpdate()
    {
        if (isDashing)
        {
            // Le vamos quitando tiempo al dash y, cuando se acaba, lo terminamos.
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
            // El movimiento real se aplica en FlipCharacter. Aquí no hace falta nada más.
        }
    }

    // Termina el dash: vuelve a poner la gravedad normal y frena el movimiento de lado.
    private void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 1; // Dejamos la gravedad como estaba normalmente.
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // Hace que el jugador ataque al mantener el botón izquierdo del ratón, esperando entre golpe y golpe.
    public void Attack()
    {
        if (isDashing) return; // No se ataca mientras hace dash.
        
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return; // No hay ratón conectado.
        }

        // Si mantienes el botón y ya pasó el tiempo de espera, atacamos.
        if (mouse.leftButton.isPressed && Time.time >= nextAttackTime)
        {
            animator.SetTrigger("Attack");
            nextAttackTime = Time.time + attackRate;    // Calculamos cuándo se podrá atacar otra vez.
            if (AudioManager.instance != null)
                AudioManager.instance.PlayAudio(AudioManager.instance.hit);
        }
    }

    // Mueve al jugador de lado y voltea su dibujo para que mire hacia donde camina (si puede moverse y no hace dash).
    private void FlipCharacter()
    {
        if (canMove && !isDashing)
        {
            // Le damos velocidad de lado sin cambiar la velocidad de arriba/abajo.
            rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
            
            // Volteamos al personaje para que mire hacia donde se mueve.
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
