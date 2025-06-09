using UnityEngine;

public class FireFalling : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public float spawnInterval = 2f;
    
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= spawnInterval)
        {
            SpawnPrefab();
            timer = 0f;
        }
    }

    void SpawnPrefab()
    {
        if (prefabToSpawn != null)
        {
            GameObject spawnedPrefab = Instantiate(prefabToSpawn, transform.position, transform.rotation);
            // Add the SelfDestruct script to handle destruction
            spawnedPrefab.AddComponent<SelfDestruct>();
        }
    }
}

// New component for the prefab
public class SelfDestruct : MonoBehaviour
{
    private float destroyTimer = 5f;

    void Update()
    {
        // Count down and destroy after time
        destroyTimer -= Time.deltaTime;
        if (destroyTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy immediately on any collision
        Destroy(gameObject);
    }
}