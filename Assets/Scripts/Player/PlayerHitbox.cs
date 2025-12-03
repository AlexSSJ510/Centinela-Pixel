using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public int damage = 1;
    public float activeTime = 0.15f;

    private Collider2D hitboxCollider;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    public void Activate()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = true;
            Invoke(nameof(Deactivate), activeTime);
        }
    }

    public void Deactivate()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Evitar dañarse a sí mismo
        if (collision.transform == transform.parent ||
            collision.transform == transform.root)
            return;

        // Buscar enemigos con IDamageable
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = collision.GetComponentInParent<IDamageable>();
        }

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }
}