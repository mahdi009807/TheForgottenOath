using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Breakables : MonoBehaviour
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
        int random = Random.Range(0, Collectibles.Length);
        Vector3 pos = transform.position + new Vector3(0f, 1f, 0f);
        Instantiate(Collectibles[random], pos, Quaternion.identity);
        spriteRenderer.sprite = active;
        GetComponent<Collider2D>().enabled = false;
        Destroy(this, 5);
    }
}