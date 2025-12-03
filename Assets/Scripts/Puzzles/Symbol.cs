using UnityEngine;

public class Symbol : MonoBehaviour
{
    public string symbolId;
    public string symbolName;

    void Start()
    {
        // Asegurarse de tener un collider
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }
}