using UnityEngine;

public class Chest : MonoBehaviour
{
    
    public GameObject[] Collectibles;
    SpriteRenderer spriteRenderer;
    public Sprite active;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void Break()
    {
        int random1 = Random.Range(0, Collectibles.Length);
        int random2 = Random.Range(0, Collectibles.Length);
        Vector3 pos = transform.position + new Vector3(0f, 1f, 0f);
        Instantiate(Collectibles[random1], pos, Quaternion.identity);
        Instantiate(Collectibles[random2], pos, Quaternion.identity);
        spriteRenderer.sprite = active;
        GetComponent<Collider2D>().enabled = false;
        // Destroy(gameObject, 5);
    }
}
