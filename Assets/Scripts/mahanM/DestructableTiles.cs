 using UnityEngine;
 using UnityEngine.Tilemaps;

 public class DestroctableTiles : MonoBehaviour
{
    public Tilemap destructableTilemap;

    public void Start()
    {
        destructableTilemap = GetComponent<Tilemap>();
    }

    public void onCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
             
        }
    }
}
