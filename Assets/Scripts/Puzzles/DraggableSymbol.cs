using UnityEngine;

public class DraggableSymbol : MonoBehaviour, IPortable
{
    [Header("Drag Settings")]
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private float snapDistance = 0.3f;
    [SerializeField] private GameObject dragEffect;

    // Referencias
    private Symbol symbolComponent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isReturning = false;
    private SymbolSlot targetSlot;

    void Start()
    {
        symbolComponent = GetComponent<Symbol>();

        // Guardar posición original
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Asegurar collider
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }

    void Update()
    {
        // Si está siendo devuelto a su posición
        if (isReturning)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition,
                                            Time.deltaTime * returnSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation,
                                               Time.deltaTime * returnSpeed);

            // Si está suficientemente cerca, detener
            if (Vector3.Distance(transform.position, originalPosition) < 0.1f)
            {
                isReturning = false;
                transform.position = originalPosition;
                transform.rotation = originalRotation;
            }
        }
    }

    // IPortable Implementation
    public bool CanBePickedUp()
    {
        return !isReturning;
    }

    public void OnPickedUp()
    {
        // Detener retorno
        isReturning = false;

        // Efecto visual
        if (dragEffect != null)
            dragEffect.SetActive(true);
    }

    public void OnDropped()
    {
        // Efecto visual
        if (dragEffect != null)
            dragEffect.SetActive(false);

        // Buscar slot más cercano
        SymbolSlot nearestSlot = FindNearestSlot();

        if (nearestSlot != null && Vector3.Distance(transform.position,
                                                   nearestSlot.transform.position) < snapDistance)
        {
            // Intentar colocar en el slot
            if (nearestSlot.TryPlaceSymbol(gameObject))
            {
                // Éxito: quedarse en el slot
                targetSlot = nearestSlot;
                return;
            }
        }

        // Si no se colocó en un slot, volver a posición original
        ReturnToOriginalPosition();
    }

    public void OnThrown(Vector2 throwForce)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(throwForce, ForceMode2D.Impulse);
        }

        OnDropped();
    }

    SymbolSlot FindNearestSlot()
    {
        SymbolSlot[] allSlots = FindObjectsOfType<SymbolSlot>();
        SymbolSlot nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (SymbolSlot slot in allSlots)
        {
            if (!slot.IsFilled)
            {
                float distance = Vector3.Distance(transform.position, slot.transform.position);
                if (distance < nearestDistance && distance < snapDistance * 2)
                {
                    nearestDistance = distance;
                    nearest = slot;
                }
            }
        }

        return nearest;
    }

    public void ReturnToOriginalPosition()
    {
        // Iniciar retorno suave
        isReturning = true;

        // Si estaba en un slot, limpiarlo
        if (targetSlot != null)
        {
            targetSlot.ClearSlot();
            targetSlot = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Mostrar radio de snap
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
    }
}