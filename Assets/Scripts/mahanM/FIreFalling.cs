using UnityEngine;

public class FireFalling : MonoBehaviour
{
    public GameObject prefabToSpawn;  // Drag your prefab here in Inspector
    public float spawnInterval = 2f; // Time between spawns (in seconds)
    
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= spawnInterval)
        {
            SpawnPrefab();
            timer = 0f; // Reset timer
        }
    }

    void SpawnPrefab()
    {
        if (prefabToSpawn != null)
        {
            // Spawn at this object's position and rotation
            Instantiate(prefabToSpawn, transform.position, transform.rotation);
        }
    }
}