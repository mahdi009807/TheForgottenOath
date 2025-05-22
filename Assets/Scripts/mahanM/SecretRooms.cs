using UnityEngine;
using UnityEngine.Tilemaps;

public class SecretRooms : MonoBehaviour
{
    private BoundsInt area;
    private Tilemap tm;
    private BoxCollider2D cldr;

    void Start()
    {
        tm = GameObject.FindGameObjectWithTag("HiddenRooms").GetComponent<Tilemap>();
        cldr = GetComponent<BoxCollider2D>();
        
        Vector3Int position = Vector3Int.FloorToInt(cldr.bounds.min);
        Vector3Int size = Vector3Int.FloorToInt(cldr.bounds.size + new Vector3Int(0, 0, 1));
        area = new BoundsInt(position, size);
        
        foreach (Vector3Int point in area.allPositionsWithin)
        {
            tm.SetTileFlags(point, TileFlags.None);
            tm.SetColor(point, new Color(1f, 1f, 1f, 0f));
        }
    }

    void RevealRoom()
    {
        foreach (Vector3Int point in area.allPositionsWithin)
        {
            tm.SetTileFlags(point, TileFlags.None);
            tm.SetColor(point, new Color(1f, 1f, 1f, 1f));
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RevealRoom();
        }
    }
}