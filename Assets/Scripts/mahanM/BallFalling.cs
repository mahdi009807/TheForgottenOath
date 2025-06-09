using UnityEngine;
using System.Collections.Generic; // Needed for List<>

public class BallFalling : MonoBehaviour
{
    [Tooltip("Players that can trigger the ball to fall")]
    public GameObject Ball;
    
    [Tooltip("Distance at which the ball will fall")]
    // public float triggerDistance = 3f;

    // private Rigidbody ballRigidbody;
    // private bool hasFallen = false;

    void Start()
    {
        // ballRigidbody = Ball.GetComponent<Rigidbody>();
        // ballRigidbody.useGravity = false; // Start with gravity disabled
    }

    // void Update()
    // {
    //     if (hasFallen) return;
    //
    //     foreach (GameObject player in players)
    //     {
    //         if (player != null && 
    //             Vector3.Distance(transform.position, player.transform.position) <= triggerDistance)
    //         {
    //             Fall();
    //             break; // No need to check other players once we fall
    //         }
    //     }
    // }

    // void Fall()
    // {
    //     ballRigidbody.useGravity = true;
    //     hasFallen = true;
    // }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Knight"))
        {
            // ballRigidbody.useGravity = true;
            Ball.SetActive(true);
            Destroy(Ball, 6);
        }
    }
}