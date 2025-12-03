using UnityEngine;

public class SymbolSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private string correctSymbolId;
    [SerializeField] private GameObject slotVisual;
    [SerializeField] private GameObject currentSymbol;

    [Header("Visual Feedback")]
    [SerializeField] private Material emptyMaterial;
    [SerializeField] private Material correctMaterial;
    [SerializeField] private Material incorrectMaterial;
    [SerializeField] private GameObject highlightEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;

    // Estado
    private bool isFilled = false;
    private bool isCorrect = false;
    private SymbolPuzzle parentPuzzle;
    private AudioSource audioSource;

    // Propiedades
    public string CorrectSymbolId => correctSymbolId;
    public bool IsFilled => isFilled;
    public bool IsCorrect => isCorrect;
    public GameObject CurrentSymbol => currentSymbol;

    void Start()
    {
        parentPuzzle = GetComponentInParent<SymbolPuzzle>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        UpdateVisuals();
    }

    public bool TryPlaceSymbol(GameObject symbolObject)
    {
        if (isFilled || parentPuzzle == null || !parentPuzzle.IsActive())
            return false;

        Symbol symbol = symbolObject.GetComponent<Symbol>();
        if (symbol == null)
            return false;

        // Colocar símbolo
        currentSymbol = symbolObject;
        symbolObject.transform.position = transform.position;
        symbolObject.transform.SetParent(transform);

        // Verificar si es correcto
        isCorrect = symbol.symbolId == correctSymbolId;
        isFilled = true;

        // Feedback visual
        UpdateVisuals();

        // SFX
        PlayPlacementSound();

        // Notificar al puzzle padre
        if (parentPuzzle != null)
        {
            parentPuzzle.OnSymbolPlaced(this, isCorrect);
        }

        return true;
    }

    public void ClearSlot()
    {
        if (currentSymbol != null)
        {
            // Mover símbolo a posición original o destruirlo
            DraggableSymbol draggable = currentSymbol.GetComponent<DraggableSymbol>();
            if (draggable != null)
            {
                draggable.ReturnToOriginalPosition();
            }
            else
            {
                Destroy(currentSymbol);
            }
        }

        currentSymbol = null;
        isFilled = false;
        isCorrect = false;

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (slotVisual != null)
        {
            Renderer renderer = slotVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (!isFilled)
                    renderer.material = emptyMaterial;
                else if (isCorrect)
                    renderer.material = correctMaterial;
                else
                    renderer.material = incorrectMaterial;
            }
        }

        // Efecto de highlight
        if (highlightEffect != null)
            highlightEffect.SetActive(!isFilled);
    }

    void PlayPlacementSound()
    {
        if (audioSource == null) return;

        audioSource.PlayOneShot(isCorrect ? correctSound : incorrectSound);
    }

    public void SetHighlight(bool highlight)
    {
        if (highlightEffect != null && !isFilled)
            highlightEffect.SetActive(highlight);
    }

    void OnDrawGizmos()
    {
        // Dibujar gizmo para identificar slots
        Gizmos.color = isCorrect ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);

        // Mostrar ID correcto
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, 
                                $"Slot: {correctSymbolId}");
#endif
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Si un símbolo se arrastra sobre el slot, highlight
        if (!isFilled && other.CompareTag("Symbol"))
        {
            SetHighlight(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!isFilled && other.CompareTag("Symbol"))
        {
            SetHighlight(false);
        }
    }
}