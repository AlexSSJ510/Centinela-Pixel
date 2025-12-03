using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private bool isLocked = true;
    [SerializeField] private KeyType requiredKey;
    [SerializeField] private GameObject doorVisual;
    [SerializeField] private Collider2D doorCollider;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject openEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip lockedSound;

    private AudioSource audioSource;

    public enum KeyType
    {
        None,
        SmallKey,
        BossKey,
        PuzzleComplete
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Unlock()
    {
        isLocked = false;
        Debug.Log("Puerta desbloqueada: " + gameObject.name);
    }

    public void TryOpen()
    {
        if (!isLocked)
        {
            Open();
        }
        else
        {
            // Feedback de que está cerrada
            if (lockedSound != null)
                audioSource.PlayOneShot(lockedSound);

            Debug.Log("La puerta está cerrada");
        }
    }

    void Open()
    {
        // Animación
        if (animator != null)
            animator.SetTrigger("Open");

        // SFX
        if (openSound != null)
            audioSource.PlayOneShot(openSound);

        // Efecto visual
        if (openEffect != null)
            Instantiate(openEffect, transform.position, Quaternion.identity);

        // Desactivar colisión
        if (doorCollider != null)
            doorCollider.enabled = false;

        Debug.Log("Puerta abierta: " + gameObject.name);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isLocked)
        {
            Open();
        }
    }
}