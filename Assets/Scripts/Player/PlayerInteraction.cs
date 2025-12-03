using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Vector2 direction = playerController.LastDirection;
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            interactionRange,
            interactableLayer
        );

        if (hit.collider != null)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Mostrar rango de interacción
        Gizmos.color = Color.yellow;
        Vector2 direction = Application.isPlaying ?
            GetComponent<PlayerController>().LastDirection : Vector2.down;
        Gizmos.DrawRay(transform.position, direction * interactionRange);
    }
}