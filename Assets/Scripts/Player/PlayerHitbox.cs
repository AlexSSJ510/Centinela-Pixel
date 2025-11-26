using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Buscar en el objeto mismo
        if (collision.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(damage);
            return;
        }

        // 2. Buscar en el padre
        IDamageable dmgParent = collision.GetComponentInParent<IDamageable>();
        if (dmgParent != null)
        {
            dmgParent.TakeDamage(damage);
        }
    }
}