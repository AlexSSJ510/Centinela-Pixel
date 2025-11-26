using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    private PlayerInputActions input;
    private Vector2 rawInput;
    private Vector2 moveInput;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    public GameObject hitbox;

    public float speed = 4f;

    private void Awake()
    {
        input = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;
        input.Player.Attack.performed += OnAttack;
        input.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMove;
        input.Player.Attack.performed -= OnAttack;
        input.Player.Interact.performed -= OnInteract;
        input.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        rawInput = ctx.ReadValue<Vector2>();

        // --- Movimiento cardinal estilo Zelda ---
        if (Mathf.Abs(rawInput.x) > Mathf.Abs(rawInput.y))
        {
            moveInput = new Vector2(Mathf.Sign(rawInput.x), 0);
        }
        else if (Mathf.Abs(rawInput.y) > 0)
        {
            moveInput = new Vector2(0, Mathf.Sign(rawInput.y));
        }
        else
        {
            moveInput = Vector2.zero;
        }
    }

    private void Update()
    {
        // --- Parámetros del Animator estilo Zelda ---
        animator.SetFloat("Horizontal", moveInput.x);
        animator.SetFloat("Vertical", moveInput.y);

        // --- Flip para izquierda ---
        if (moveInput.x < 0)
            sr.flipX = true;
        else if (moveInput.x > 0)
            sr.flipX = false;

        Vector2 dir = moveInput;

        if (dir.x > 0) hitbox.transform.localPosition = new Vector3(0.5f, 0, 0);
        if (dir.x < 0) hitbox.transform.localPosition = new Vector3(-0.5f, 0, 0);
        if (dir.y > 0) hitbox.transform.localPosition = new Vector3(0, 0.5f, 0);
        if (dir.y < 0) hitbox.transform.localPosition = new Vector3(0, -0.5f, 0);
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }

    // ============================
    //             ATAQUE
    // ============================
    private void OnAttack(InputAction.CallbackContext ctx)
    {
        animator.SetTrigger("Attack");

        // Activar hitbox solo durante unos frames
        StartCoroutine(AttackWindow());
    }

    private IEnumerator AttackWindow()
    {
        hitbox.SetActive(true);
        yield return new WaitForSeconds(0.15f); // tiempo del frame de impacto
        hitbox.SetActive(false);
    }

    // ============================
    //          INTERACTUAR
    // ============================
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("Interact!");

        Vector2 dir = moveInput;
        if (dir == Vector2.zero)
            dir = Vector2.down;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 1f);

        if (hit.collider != null)
        {
            Debug.Log("Interacción con: " + hit.collider.name);
            hit.collider.GetComponent<IInteractable>()?.Interact();
        }
    }
}