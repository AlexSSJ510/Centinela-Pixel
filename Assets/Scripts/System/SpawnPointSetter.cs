using UnityEngine;

public class SpawnPointSetter : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject spawn = GameObject.FindGameObjectWithTag("SpawnPoint");

        if (player != null && spawn != null)
        {
            player.transform.position = spawn.transform.position;
        }
    }
}