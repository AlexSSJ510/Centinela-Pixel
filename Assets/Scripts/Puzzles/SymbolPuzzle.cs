using UnityEngine;
using System.Collections.Generic;

public class SymbolPuzzle : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private bool isComplete = false;

    [Header("Slots")]
    [SerializeField] private SymbolSlot[] symbolSlots;

    [Header("Symbols")]
    [SerializeField] private GameObject[] symbolPrefabs;
    [SerializeField] private Transform symbolsSpawnArea;
    [SerializeField] private float spawnRadius = 2f;

    [Header("Reward")]
    [SerializeField] private GameObject rewardObject;
    [SerializeField] private Transform rewardSpawnPoint;
    [SerializeField] private bool rewardGiven = false;

    [Header("Completion Effects")]
    [SerializeField] private GameObject completionEffect;
    [SerializeField] private AudioClip completionSound;
    [SerializeField] private DoorController[] doorsToOpen;

    private List<GameObject> spawnedSymbols = new List<GameObject>();
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Inicializar slots
        foreach (SymbolSlot slot in symbolSlots)
        {
            if (slot != null)
            {
                // Conectar slot con este puzzle
                // (se hace automáticamente por parent-child relationship)
            }
        }
    }

    public void ActivatePuzzle()
    {
        if (isActive || isComplete) return;

        isActive = true;
        SpawnSymbols();

        Debug.Log($"Puzzle '{gameObject.name}' activado!");
    }

    void SpawnSymbols()
    {
        // Limpiar símbolos anteriores
        ClearSpawnedSymbols();

        // Spawnear símbolos alrededor del área designada
        Vector3 spawnCenter = symbolsSpawnArea != null ?
            symbolsSpawnArea.position : transform.position;

        for (int i = 0; i < symbolPrefabs.Length; i++)
        {
            // Posición circular
            float angle = i * Mathf.PI * 2f / symbolPrefabs.Length;
            Vector3 spawnPos = spawnCenter + new Vector3(
                Mathf.Cos(angle) * spawnRadius,
                Mathf.Sin(angle) * spawnRadius,
                0
            );

            GameObject symbol = Instantiate(symbolPrefabs[i], spawnPos, Quaternion.identity);
            symbol.tag = "Symbol";

            // Inicializar componente DraggableSymbol
            DraggableSymbol draggable = symbol.GetComponent<DraggableSymbol>();
            if (draggable == null)
                draggable = symbol.AddComponent<DraggableSymbol>();

            spawnedSymbols.Add(symbol);
        }
    }

    public void OnSymbolPlaced(SymbolSlot slot, bool isCorrect)
    {
        if (!isActive || isComplete) return;

        Debug.Log($"Símbolo colocado en slot: {slot.CorrectSymbolId} - " +
                 $"Correcto: {isCorrect}");

        // Verificar si el puzzle está completo
        CheckPuzzleCompletion();
    }

    void CheckPuzzleCompletion()
    {
        foreach (SymbolSlot slot in symbolSlots)
        {
            if (!slot.IsFilled || !slot.IsCorrect)
                return;
        }

        // Todos los slots están correctamente llenos
        OnPuzzleComplete();
    }

    void OnPuzzleComplete()
    {
        isComplete = true;
        isActive = false;

        Debug.Log($"¡Puzzle '{gameObject.name}' completado!");

        // Efectos
        PlayCompletionEffects();

        // Recompensa
        GiveReward();

        // Abrir puertas
        OpenConnectedDoors();

        // Limpiar símbolos sobrantes
        ClearSpawnedSymbols();
    }

    void PlayCompletionEffects()
    {
        // VFX
        if (completionEffect != null)
        {
            GameObject effect = Instantiate(completionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // SFX
        if (completionSound != null)
            audioSource.PlayOneShot(completionSound);

        // Animación en slots
        foreach (SymbolSlot slot in symbolSlots)
        {
            // Podrías agregar efectos individuales aquí
        }
    }

    void GiveReward()
    {
        if (rewardGiven || rewardObject == null) return;

        Vector3 spawnPos = rewardSpawnPoint != null ?
            rewardSpawnPoint.position : transform.position + Vector3.up;

        GameObject reward = Instantiate(rewardObject, spawnPos, Quaternion.identity);

        // Si es un item especial, hacerlo flotar
        if (reward.CompareTag("HeartContainer") || reward.CompareTag("Key"))
        {
            StartCoroutine(FloatItem(reward));
        }

        rewardGiven = true;
    }

    System.Collections.IEnumerator FloatItem(GameObject item)
    {
        Vector3 startPos = item.transform.position;
        float floatHeight = 0.3f;
        float floatSpeed = 1.5f;
        float timer = 0f;

        while (item != null)
        {
            float yOffset = Mathf.Sin(timer * floatSpeed) * floatHeight;
            item.transform.position = startPos + new Vector3(0, yOffset, 0);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    void OpenConnectedDoors()
    {
        foreach (DoorController door in doorsToOpen)
        {
            if (door != null)
                door.Unlock();
        }
    }

    void ClearSpawnedSymbols()
    {
        foreach (GameObject symbol in spawnedSymbols)
        {
            if (symbol != null)
                Destroy(symbol);
        }
        spawnedSymbols.Clear();
    }

    public bool IsActive() => isActive;
    public bool IsComplete() => isComplete;

    void OnDrawGizmos()
    {
        // Área de spawn de símbolos
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Vector3 center = symbolsSpawnArea != null ?
            symbolsSpawnArea.position : transform.position;
        Gizmos.DrawWireSphere(center, spawnRadius);

        // Conexiones con puertas
        Gizmos.color = Color.blue;
        foreach (DoorController door in doorsToOpen)
        {
            if (door != null)
                Gizmos.DrawLine(transform.position, door.transform.position);
        }
    }
}