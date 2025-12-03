using UnityEngine;
using System.Collections;

public class CorruptedStatue : MonoBehaviour, IInteractable
{
    [Header("Statue Settings")]
    [SerializeField] private bool isCorrupted = true;
    [SerializeField] private GameObject corruptedVisual;
    [SerializeField] private GameObject purifiedVisual;

    [Header("Purification Settings")]
    [SerializeField] private GameObject purificationEffect;
    [SerializeField] private AudioClip purificationSound;
    [SerializeField] private float purificationTime = 2f;

    [Header("Puzzle Connection")]
    [SerializeField] private SymbolPuzzle[] connectedPuzzles;
    [SerializeField] private DoorController[] doorsToUnlock;

    [Header("Reward")]
    [SerializeField] private GameObject rewardPrefab;
    [SerializeField] private Transform rewardSpawnPoint;

    // Estado
    private bool isPurifying = false;
    private bool hasBeenPurified = false;
    private AudioSource audioSource;

    // Eventos
    public System.Action OnStatuePurified;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (corruptedVisual != null)
            corruptedVisual.SetActive(isCorrupted && !hasBeenPurified);

        if (purifiedVisual != null)
            purifiedVisual.SetActive(!isCorrupted || hasBeenPurified);
    }

    // Llamado por el BossController cuando muere
    public void Purify()
    {
        if (hasBeenPurified || isPurifying) return;

        StartCoroutine(PurificationProcess());
    }

    IEnumerator PurificationProcess()
    {
        isPurifying = true;

        Debug.Log("Purificando estatua...");

        // Efecto visual
        if (purificationEffect != null)
        {
            GameObject effect = Instantiate(purificationEffect, transform.position, Quaternion.identity);
            Destroy(effect, purificationTime);
        }

        // SFX
        if (purificationSound != null)
            audioSource.PlayOneShot(purificationSound);

        // Animación de transición
        float timer = 0f;
        while (timer < purificationTime)
        {
            // Puedes agregar efectos de transición aquí
            // Ej: cambiar color gradualmente, escalar, etc.
            timer += Time.deltaTime;
            yield return null;
        }

        // Cambiar estado
        isCorrupted = false;
        hasBeenPurified = true;

        UpdateVisuals();

        // Activar puzzles conectados
        ActivateConnectedPuzzles();

        // Abrir puertas
        UnlockDoors();

        // Spawnear recompensa
        SpawnReward();

        // Evento
        OnStatuePurified?.Invoke();

        isPurifying = false;

        Debug.Log("Estatua purificada!");
    }

    void ActivateConnectedPuzzles()
    {
        foreach (SymbolPuzzle puzzle in connectedPuzzles)
        {
            if (puzzle != null)
                puzzle.ActivatePuzzle();
        }
    }

    void UnlockDoors()
    {
        foreach (DoorController door in doorsToUnlock)
        {
            if (door != null)
                door.Unlock();
        }
    }

    void SpawnReward()
    {
        if (rewardPrefab != null && rewardSpawnPoint != null)
        {
            GameObject reward = Instantiate(rewardPrefab, rewardSpawnPoint.position, Quaternion.identity);

            // Si es un corazón, hacerlo flotar
            if (reward.CompareTag("HeartContainer"))
            {
                StartCoroutine(FloatReward(reward));
            }
        }
    }

    IEnumerator FloatReward(GameObject reward)
    {
        float floatHeight = 0.5f;
        float floatSpeed = 2f;
        Vector3 startPos = reward.transform.position;
        float timer = 0f;

        while (reward != null)
        {
            float yOffset = Mathf.Sin(timer * floatSpeed) * floatHeight;
            reward.transform.position = startPos + new Vector3(0, yOffset, 0);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    // Implementación de IInteractable
    public void Interact()
    {
        if (hasBeenPurified)
        {
            // La estatua ya está purificada, mostrar mensaje
            ShowMessage("La estatua ya está purificada. El valle está a salvo.");
            return;
        }

        if (isCorrupted)
        {
            // Mostrar mensaje de que necesita ser purificada
            ShowMessage("Esta estatua está corrompida. Derrota al Señor de la Corrupción para purificarla.");
        }
    }

    void ShowMessage(string message)
    {
        // Implementar sistema de diálogo UI
        Debug.Log(message);

        // Si tienes un UIManager:
        // UIManager.Instance.ShowDialogue(message, 2f);
    }

    // Getters
    public bool IsPurified() => hasBeenPurified;
    public bool IsCorrupted() => isCorrupted && !hasBeenPurified;

    // Para debugging
    void OnDrawGizmos()
    {
        // Mostrar conexiones con puzzles
        Gizmos.color = Color.cyan;
        foreach (SymbolPuzzle puzzle in connectedPuzzles)
        {
            if (puzzle != null)
                Gizmos.DrawLine(transform.position, puzzle.transform.position);
        }

        // Mostrar conexiones con puertas
        Gizmos.color = Color.green;
        foreach (DoorController door in doorsToUnlock)
        {
            if (door != null)
                Gizmos.DrawLine(transform.position, door.transform.position);
        }
    }
}