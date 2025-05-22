using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [Header("Chunk Settings")]
    [SerializeField] private int chunkWidth = 10;
    [SerializeField] private float generateAheadDistance = 20f;
    [SerializeField] private int chunksPerGroup = 3;
    [SerializeField] private int initialGroups = 1;

    [Header("References")]
    [SerializeField] private GameObject startChunk;
    [SerializeField] private GameObject checkpointChunk;
    [SerializeField] private GameObject[] chunkPrefabs;
    [SerializeField] private Transform player;

    private List<GameObject> activeChunks = new List<GameObject>();
    private float chunkWorldWidth;
    private int currentGroup = 0;
    private bool finalChunkSpawned = false;
    private List<int> availableChunkIndices = new List<int>();

    void Start()
    {
        if (chunkPrefabs.Length < chunksPerGroup)
        {
            Debug.LogError("Not enough chunk prefabs assigned!");
            return;
        }

        chunkWorldWidth = chunkWidth;

        // Spawn start chunk
        SpawnChunk(startChunk, 0);
        
        // Initialize available chunks (first 3 for first group)
        ResetAvailableChunks(0, chunksPerGroup);
        
        // Spawn initial group
        SpawnGroup();
    }

    void Update()
    {
        if (finalChunkSpawned) return;

        float playerPosX = player.position.x;
        float furthestChunkEndX = activeChunks.Count * chunkWorldWidth;

        if (playerPosX + generateAheadDistance > furthestChunkEndX)
        {
            if (availableChunkIndices.Count == 0)
            {
                currentGroup++;
                
                if (currentGroup * chunksPerGroup < chunkPrefabs.Length)
                {
                    // Spawn checkpoint
                    SpawnCheckpoint();
                    
                    // Reset available chunks for next group
                    ResetAvailableChunks(currentGroup * chunksPerGroup, chunksPerGroup);
                    
                    // Spawn next group
                    SpawnGroup();
                }
                else
                {
                    // Spawn final checkpoint and chunk
                    SpawnCheckpoint();
                    SpawnFinalChunk();
                    finalChunkSpawned = true;
                }
            }
            else
            {
                SpawnGroup();
            }
        }
    }

    void ResetAvailableChunks(int startIndex, int count)
    {
        availableChunkIndices.Clear();
        int endIndex = Mathf.Min(startIndex + count, chunkPrefabs.Length);
        
        for (int i = startIndex; i < endIndex; i++)
        {
            availableChunkIndices.Add(i);
        }
    }

    void SpawnGroup()
    {
        int chunksToSpawn = Mathf.Min(availableChunkIndices.Count, chunksPerGroup);
        
        for (int i = 0; i < chunksToSpawn; i++)
        {
            // Get random index from available chunks
            int randomListIndex = Random.Range(0, availableChunkIndices.Count);
            int chunkIndex = availableChunkIndices[randomListIndex];
            availableChunkIndices.RemoveAt(randomListIndex);
            
            SpawnChunk(chunkPrefabs[chunkIndex], activeChunks.Count);
        }
    }

    void SpawnCheckpoint()
    {
        SpawnChunk(checkpointChunk, activeChunks.Count);
    }

    void SpawnFinalChunk()
    {
        SpawnChunk(chunkPrefabs[chunkPrefabs.Length - 1], activeChunks.Count);
    }

    void SpawnChunk(GameObject chunkPrefab, int positionIndex)
    {
        GameObject chunk = Instantiate(chunkPrefab, transform);
        chunk.transform.position = new Vector3(positionIndex * chunkWorldWidth, 0, 0);
        activeChunks.Add(chunk);
    }
}