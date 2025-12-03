using UnityEngine;

public class ActivateBoss : MonoBehaviour
{
    public BossController boss;
    public GameObject bossHealthBar;

    void Start()
    {
        // Desactivar inicialmente
        if (boss != null)
            boss.enabled = false;

        if (bossHealthBar != null)
            bossHealthBar.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Activar boss
            if (boss != null)
            {
                boss.enabled = true;
                Debug.Log("¡Boss activado!");
            }

            // Mostrar health bar
            if (bossHealthBar != null)
                bossHealthBar.SetActive(true);

            // Desactivar trigger
            GetComponent<Collider2D>().enabled = false;
        }
    }
}