using UnityEngine;
using UnityEngine.Events;

public class Collectibles : MonoBehaviour
{
    // just for coins and diamonds (shop)
    public UnityEvent collisionEntered;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {            
            collisionEntered?.Invoke();
            Destroy(gameObject);

        }
    }
}
