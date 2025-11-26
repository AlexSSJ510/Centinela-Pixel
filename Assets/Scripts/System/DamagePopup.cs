using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float lifetime = 0.6f;
    public float floatSpeed = 1f;

    private float timer;

    private TextMeshPro tmp;
    private Color textColor;

    private void Awake()
    {
        // Busca TMP en este objeto o en cualquier hijo
        tmp = GetComponent<TextMeshPro>();

        if (tmp == null)
            tmp = GetComponentInChildren<TextMeshPro>(true);

        if (tmp == null)
        {
            Debug.LogError("? DamagePopup: No se encontró un TextMeshPro en el prefab.");
            return;
        }

        textColor = tmp.color;
        timer = lifetime;
    }

    public void Setup(int damage)
    {
        if (tmp != null)
            tmp.text = damage.ToString();
    }

    private void Update()
    {
        if (tmp == null) return;

        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            textColor.a -= Time.deltaTime * 2f;
            tmp.color = textColor;

            if (textColor.a <= 0)
                Destroy(gameObject);
        }
    }
}