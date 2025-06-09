using UnityEngine;

public class PlatformRotation : MonoBehaviour
{
    [Tooltip("Time between rotations in seconds")]
    public float rotationInterval = 5f;
    
    [Tooltip("How long each rotation takes in seconds")]
    public float rotationDuration = 1f;

    private float timer;
    private bool isRotating;

    void Update()
    {
        timer += Time.deltaTime;

        // Start rotation when interval is reached
        if (timer >= rotationInterval && !isRotating)
        {
            StartCoroutine(RotatePlatform());
            timer = 0f;
        }
    }

    System.Collections.IEnumerator RotatePlatform()
    {
        isRotating = true;
        
        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, 180);

        while (elapsed < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(
                startRotation, 
                endRotation, 
                elapsed / rotationDuration
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure we reach exactly 180 degrees
        transform.rotation = endRotation;
        isRotating = false;
    }
}