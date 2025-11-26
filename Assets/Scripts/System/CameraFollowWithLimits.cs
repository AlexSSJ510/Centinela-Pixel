using UnityEngine;

public class CameraFollowWithLimits : MonoBehaviour
{
    public Transform target;        // El jugador
    public float smoothSpeed = 0.15f;

    [Header("Limites de la cámara")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // posición deseada siguiendo al jugador
        Vector3 desiredPos = new Vector3(target.position.x, target.position.y, transform.position.z);

        // limitar la posición
        float clampX = Mathf.Clamp(desiredPos.x, minX, maxX);
        float clampY = Mathf.Clamp(desiredPos.y, minY, maxY);

        Vector3 limitedPos = new Vector3(clampX, clampY, desiredPos.z);

        // movimiento suave
        transform.position = Vector3.SmoothDamp(transform.position, limitedPos, ref velocity, smoothSpeed);
    }
}