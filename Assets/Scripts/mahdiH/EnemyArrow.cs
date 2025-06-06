using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    public float speed = 20f;
    private Vector2 direction;
    private int damage;
    private bool hasHit = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;

        // چرخش پیکان در جهت پرتاب
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Update()
    {
        if (!hasHit)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // برخورد با زمین
        if (other.CompareTag("Ground"))
        {
            hasHit = true;
            StopArrow();
            Destroy(gameObject, 2f);
        }
        // برخورد با بازیکن
        else if (other.TryGetComponent<MeleePlayer>(out MeleePlayer melee))
        {
            melee.TakeDamage(damage , transform);
            hasHit = true;
            StopArrow();
            Destroy(gameObject, 0.2f); // تاخیر جزئی در حذف برای افکت احتمالی
        }
        else if (other.TryGetComponent<RangePlayer>(out RangePlayer range))
        {
            range.TakeDamage(damage , transform);
            hasHit = true;
            StopArrow();
            Destroy(gameObject, 0.2f);
        }
    }

    private void StopArrow()
    {
        direction = Vector2.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
    }
}