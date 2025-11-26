using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float range = 1f;
    public LayerMask interactLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, range, interactLayer);
            if (hit != null) hit.GetComponent<IInteractable>()?.Interact();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}