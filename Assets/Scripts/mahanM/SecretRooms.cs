using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SecretRooms : MonoBehaviour

{
    BoundsInt area;
    Tilemap tm;
    BoxCollider2D cldr;

    private SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        //area = new BoundsInt(new Vector3Int(95,11,0), new Vector3Int(6,5,1));
        tm = GameObject.FindGameObjectWithTag("HiddenRooms").GetComponent<Tilemap>();
        cldr = GetComponent<BoxCollider2D>();
        Vector3Int position = Vector3Int.FloorToInt(cldr.bounds.min);
        Vector3Int size = Vector3Int.FloorToInt(cldr.bounds.size + new Vector3Int(0,0,1));
        area = new BoundsInt(position, size);

        foreach (Vector3Int point in area.allPositionsWithin)
        {
            tm.SetTileFlags(point, TileFlags.None);
            tm.SetColor(point, new Color(255f, 255f, 255f, 0f));
            //tm.SetTileFlags(point, TileFlags.LockColor);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    void RevealRoom()
    {
        foreach (Vector3Int point in area.allPositionsWithin)
        {
            sr.color = new Color(255, 255, 255, 0);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Melee_Player"))
        {
            RevealRoom();
        }
    }
    void HideRoom()
    {
        foreach (Vector3Int point in area.allPositionsWithin)
        {
            tm.SetTileFlags(point, TileFlags.None);
            tm.SetColor(point, new Color(255f, 255f, 255f, 0f));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HideRoom();
        }
    }
}