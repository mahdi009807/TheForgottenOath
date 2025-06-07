using UnityEngine;

public class FireShooting : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] [Range(0, 180)] private float throwAngle = 45f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float shootInterval = 1f;

    private float timer;

    private void Start()
    {
        timer = shootInterval;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        
        if (timer <= 0f)
        {
            timer = shootInterval;
            ThrowProjectile();
        }
    }

    private void ThrowProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        
        // Calculate direction
        float angleRad = throwAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        
        // Apply force
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.AddForce(direction * throwForce, ForceMode2D.Impulse);
        
        // Add collision detection component
        ProjectileCollisionHandler handler = projectile.AddComponent<ProjectileCollisionHandler>();
        handler.Initialize(rb);
    }
}

// New component for projectile collision and lifetime management
public class ProjectileCollisionHandler : MonoBehaviour
{
    private Rigidbody2D rb;
    private float lifetime = 5f;

    public void Initialize(Rigidbody2D rigidbody)
    {
        rb = rigidbody;
    }

    private void Update()
    {
        // Countdown lifetime
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy on any collision
        Destroy(gameObject);
    }
}