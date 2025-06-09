using UnityEngine;
using UnityEngine.Events;

public class DoorTrigger2D : MonoBehaviour
{
    public UnityEvent onPlayerEnter;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player")) {
            onPlayerEnter.Invoke();
        }
    }
}