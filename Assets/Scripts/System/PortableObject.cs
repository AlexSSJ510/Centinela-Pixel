using UnityEngine;

public class PortableObject : MonoBehaviour, IPortable, IInteractable
{
    [Header("Portable Settings")]
    public bool canBePickedUp = true;
    public bool breakOnThrow = false;
    public float breakForceThreshold = 15f;

    private Rigidbody2D rb;
    private bool isBeingCarried = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Implementación de IPortable
    public bool CanBePickedUp() => canBePickedUp && !isBeingCarried;

    public void OnPickedUp()
    {
        isBeingCarried = true;
        // Opcional: cambiar layer para no interferir
        gameObject.layer = LayerMask.NameToLayer("IgnoreRaycast");
    }

    public void OnDropped()
    {
        isBeingCarried = false;
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public void OnThrown(Vector2 throwForce)
    {
        isBeingCarried = false;
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        if (breakOnThrow && throwForce.magnitude > breakForceThreshold)
        {
            BreakObject();
        }
    }

    private void BreakObject()
    {
        Debug.Log($"{name} se rompió!");
        // Aquí podrías instanciar un efecto o cambiar sprite
        Destroy(gameObject);
    }

    // Implementación de IInteractable (para puzzles)
    public void Interact()
    {
        Debug.Log($"Interactuando con {name}");
        // Lógica específica del puzzle
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Detectar si choca con algo importante para puzzles
        if (collision.gameObject.CompareTag("PressurePlate") ||
            collision.gameObject.CompareTag("PuzzleSocket"))
        {
            collision.gameObject.GetComponent<IInteractable>()?.Interact();
        }
    }
}