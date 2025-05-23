using UnityEngine;

public class PlatformRotation : MonoBehaviour
{
    [SerializeField] private float flipInterval = 5f;
    private float timer;
    private Vector3 pivotPoint; // Rotation pivot point

    private void Start()
    {
        timer = flipInterval;
        // Set pivot point to the platform's center
        pivotPoint = GetComponent<Collider2D>().bounds.center;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        
        if (timer <= 0f)
        {
            FlipPlatform();
            timer = flipInterval;
        }
    }

    private void FlipPlatform()
    {
        // Rotate around the center point
        transform.RotateAround(pivotPoint, Vector3.forward, 180);
        
        // Optional: Add visual/sound effects
        OnFlip();
    }

    private void OnFlip()
    {
        // Add your effects here (particles, sounds, etc.)
    }

    // Visualize the pivot point in editor
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pivotPoint, 0.1f);
        }
    }
}
