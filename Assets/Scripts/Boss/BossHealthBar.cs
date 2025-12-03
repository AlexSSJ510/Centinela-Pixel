using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;
    public Image fillImage;
    public Text healthText;

    [Header("Boss Reference")]
    public BossController boss;

    [Header("Settings")]
    public Vector3 screenOffset = new Vector3(0, 50f, 0); // Offset en píxeles

    private Camera mainCamera;
    private RectTransform rectTransform;

    void Start()
    {
        // Este GameObject DEBE estar en un Canvas
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;

        // Buscar boss si no está asignado
        if (boss == null)
        {
            boss = FindObjectOfType<BossController>();
        }

        if (boss != null && healthSlider != null)
        {
            healthSlider.maxValue = boss.maxHealth;
            healthSlider.value = boss.maxHealth;
            UpdateHealthText();
        }

        // Ocultar inicialmente si no hay boss
        if (boss == null)
            gameObject.SetActive(false);
    }

    void Update()
    {
        if (boss == null || !boss.IsAlive())
        {
            gameObject.SetActive(false);
            return;
        }

        // 1. Seguir al boss en pantalla
        if (mainCamera != null)
        {
            Vector3 worldPosition = boss.transform.position + Vector3.up * 1.5f;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // Aplicar offset y asegurar que esté en pantalla
            screenPosition += screenOffset;
            screenPosition.x = Mathf.Clamp(screenPosition.x, 100f, Screen.width - 100f);
            screenPosition.y = Mathf.Clamp(screenPosition.y, 50f, Screen.height - 50f);

            rectTransform.position = screenPosition;
        }

        // 2. Actualizar salud
        if (healthSlider != null)
        {
            healthSlider.value = boss.GetCurrentHealth();
            UpdateHealthText();
        }
    }

    void UpdateHealthText()
    {
        if (healthText != null && healthSlider != null)
            healthText.text = $"{healthSlider.value}/{healthSlider.maxValue}";
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}