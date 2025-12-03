using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 4f;

    [Header("Attack")]
    public GameObject attackHitbox;
    public float attackDuration = 0.15f;

    [Header("Health")]
    public int maxHealth = 10;
    public float invulnerabilityTime = 1f;
    public Slider healthSlider;

    [Header("Interaction")]
    public float interactRange = 1f;
    public Transform carryPoint;
    public LayerMask interactableLayer;

    // Componentes
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerInputActions input;

    // Estado
    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.down;
    private bool isAttacking = false;
    private bool isInvulnerable = false;
    private GameObject carriedObject = null;

    // Salud
    private int currentHealth;

    // Propiedades
    public bool IsCarrying => carriedObject != null;
    public Vector2 LastDirection => lastDirection;
    public bool IsInvulnerable => isInvulnerable;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public System.Action<int> OnHealthChanged;
    public System.Action OnPlayerDeath;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        input = new PlayerInputActions();

        // Inicializar salud
        currentHealth = maxHealth;
    }

    void Start()
    {
        if (attackHitbox != null)
        {
            // Desactivar collider al inicio
            Collider2D col = attackHitbox.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
                Debug.Log("Collider desactivado al inicio");
            }

            // Asegurar que el GameObject SÍ esté activo
            attackHitbox.SetActive(true);
        }
        // Inicializar salud UI
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void OnEnable()
    {
        input.Player.Enable();
        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;
        input.Player.Attack.performed += OnAttack;
        input.Player.Interact.performed += OnInteract;
    }

    void OnDisable()
    {
        input.Player.Disable();
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 raw = ctx.ReadValue<Vector2>();

        if (raw.magnitude > 0.1f)
        {
            if (Mathf.Abs(raw.x) > Mathf.Abs(raw.y))
                moveInput = new Vector2(Mathf.Sign(raw.x), 0);
            else
                moveInput = new Vector2(0, Mathf.Sign(raw.y));

            lastDirection = moveInput;
        }
        else
        {
            moveInput = Vector2.zero;
        }
    }

    void Update()
    {
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!isAttacking)
        {
            rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
        }
    }

    void UpdateAnimator()
    {
        animator.SetFloat("DirX", lastDirection.x);
        animator.SetFloat("DirY", lastDirection.y);
        animator.SetFloat("Speed", moveInput.magnitude);
        animator.SetBool("IsCarrying", IsCarrying);
    }

    void OnAttack(InputAction.CallbackContext ctx)
    {
        if (isAttacking || IsCarrying) return;

        animator.SetFloat("AtkDirX", lastDirection.x);
        animator.SetFloat("AtkDirY", lastDirection.y);
        animator.SetTrigger("Attack");

        StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        if (isAttacking || IsCarrying)
            yield break;

        isAttacking = true;

        // Activar animación
        animator.SetFloat("AtkDirX", lastDirection.x);
        animator.SetFloat("AtkDirY", lastDirection.y);
        animator.SetTrigger("Attack");

         attackHitbox.transform.localPosition = lastDirection * 0.5f;

        if (lastDirection.x != 0)
        {
            // Horizontal
            attackHitbox.transform.localRotation = Quaternion.identity;
            attackHitbox.transform.localScale = new Vector3(Mathf.Sign(lastDirection.x), 1, 1);
        }
        else if (lastDirection.y > 0)
        {
            // Arriba
            attackHitbox.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else if (lastDirection.y < 0)
        {
            attackHitbox.transform.localRotation = Quaternion.Euler(0, 0, -90);
        }

        Collider2D attackCollider = attackHitbox.GetComponent<Collider2D>();
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
            Debug.Log("Collider ACTIVADO");
        }

        yield return new WaitForSeconds(attackDuration);

        if (attackCollider != null)
        {
            attackCollider.enabled = false;
            Debug.Log("Collider DESACTIVADO");
        }

        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthUI(); // <-- Actualizar UI

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(Invulnerability());
        }
    }

    IEnumerator Invulnerability()
    {
        isInvulnerable = true;
        float timer = 0f;

        while (timer < invulnerabilityTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    void Die()
    {
        // Desactivar controles
        enabled = false;
        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        // Animación de muerte
        animator.SetTrigger("Die");

        Debug.Log("Player murió - Game Over");

        // Reiniciar escena
        Invoke(nameof(Respawn), 2f);
    }

    private void Respawn()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (IsCarrying)
            DropCarriedObject();
        else
            TryPickup();
    }

    void TryPickup()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, lastDirection, interactRange, interactableLayer);
        if (hit.collider != null)
        {
            var portable = hit.collider.GetComponent<IPortable>();
            if (portable != null && portable.CanBePickedUp())
            {
                PickUpObject(hit.collider.gameObject);
            }
        }
    }

    void PickUpObject(GameObject obj)
    {
        carriedObject = obj;
        if (obj.TryGetComponent<Rigidbody2D>(out var rbObj))
            rbObj.simulated = false;

        obj.transform.SetParent(carryPoint);
        obj.transform.localPosition = Vector3.zero;
        obj.GetComponent<IPortable>()?.OnPickedUp();
    }

    void DropCarriedObject()
    {
        if (carriedObject == null) return;

        if (carriedObject.TryGetComponent<Rigidbody2D>(out var rbObj))
            rbObj.simulated = true;

        carriedObject.transform.SetParent(null);
        carriedObject.GetComponent<IPortable>()?.OnDropped();
        carriedObject = null;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
    }

    public bool IsAlive() => currentHealth > 0;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}