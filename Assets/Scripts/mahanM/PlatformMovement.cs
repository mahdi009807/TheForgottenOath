using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    public Transform posA, posB;
    public float moveSpeed;
    private Vector3 targetPos;

    private void Start()
    {
        targetPos = posB.position;
    }
    private void Update()
    {
        if (Vector2.Distance(transform.position, posB.position) < 0.05f)
        {
            targetPos = posA.position;
        }

        if (Vector2.Distance(transform.position, posA.position) < 0.05f)
        {
            targetPos = posB.position;
        }
         transform.position = Vector3.MoveTowards( transform.position, targetPos, Time.deltaTime * moveSpeed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.transform.parent = this.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.transform.parent = null;
        }
    }
}
