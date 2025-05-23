using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    // PlayerMovement playerController;
    GameObject player;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        // playerController = player.GetComponent<PlayerMovement>();
    }
    
    void OnCollisionEnter2D(Collision2D col)
    {
        // if (col.collider.tag == "Player" && playerController.GetIsDashing())
        {
            Destroy(gameObject);
        }

    }
}
