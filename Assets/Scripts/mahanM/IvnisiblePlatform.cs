using UnityEngine;

public class InvisiblePlatform : MonoBehaviour
{
    [Tooltip("Time in seconds between visibility toggles")]
    public float toggleTime = 2f; // Only variable you need to set
    public GameObject Platform1;
    public GameObject Platform2;

    // private Collider platformCollider;
    // private MeshRenderer meshRenderer;
    private float timer;
    private bool isVisible = true;

    void Start()
    {
        // // Automatically get required components
        // platformCollider = GetComponent<Collider>();
        // meshRenderer = GetComponent<MeshRenderer>();

        timer = toggleTime; // Start countdown immediately
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (isVisible)
            {
                Platform1.SetActive(false);
                Platform2.SetActive(true);
                isVisible = false;
            }
            else
            {
                Platform1.SetActive(true);
                Platform2.SetActive(false);
                isVisible = true;
            }
            timer = toggleTime; // Reset timer
        }
    }

    // void ToggleVisibility()
    // {
    //     isVisible = !isVisible;
    //     platformCollider.enabled = isVisible;
    //     meshRenderer.enabled = isVisible;
    // }
}