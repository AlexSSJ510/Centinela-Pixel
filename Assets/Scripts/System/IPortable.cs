using UnityEngine;

public interface IPortable
{
    bool CanBePickedUp();
    void OnPickedUp();
    void OnDropped();
    void OnThrown(Vector2 throwForce);
}