using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 4f;

    [Header("Attack")]
    public GameObject attackHitbox;
    public float attackDuration = 0.15f;

    [Header("Interaction")]
    public float interactRange = 1f;
    public Transform carryPoint;
    public LayerMask interactableLayer;

    // Componentes
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInputActions input;

    // Estado
    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.down;
    private bool isAttacking = false;
    private GameObject carriedObject = null;

    // Propiedades
    public bool IsCarrying => carriedObject != null;
    public Vector2 LastDirection => lastDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        input = new PlayerInputActions();
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

        // Movimiento cardinal (estilo Zelda)
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
        // Siempre pasar la última dirección (para idle)
        animator.SetFloat("DirX", lastDirection.x);
        animator.SetFloat("DirY", lastDirection.y);

        // Velocidad para walk/ idle
        animator.SetFloat("Speed", moveInput.magnitude);

        // Estado de carga
        animator.SetBool("IsCarrying", IsCarrying);
    }

    void OnAttack(InputAction.CallbackContext ctx)
    {
        if (isAttacking || IsCarrying) return;

        // IMPORTANTE: Setear dirección ANTES del trigger
        animator.SetFloat("AtkDirX", lastDirection.x);
        animator.SetFloat("AtkDirY", lastDirection.y);
        animator.SetTrigger("Attack");

        StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;

        if (attackHitbox != null)
        {
            // Posicionar hitbox en dirección
            attackHitbox.transform.localPosition = lastDirection * 0.5f;
            attackHitbox.SetActive(true);
        }

        yield return new WaitForSeconds(attackDuration);

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        isAttacking = false;
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
}