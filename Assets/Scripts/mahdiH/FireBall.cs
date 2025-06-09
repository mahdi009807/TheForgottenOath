using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float speed = 10f;
    private Vector2 direction;
    private int damage;
    private bool hasHit = false;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;

        // چرخش با scale (فقط افقی)
        if (direction.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void Update()
    {
        if (!hasHit)
        {
            transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        hasHit = true;

        // برخورد با بازیکن
        if (other.TryGetComponent<MeleePlayer>(out var melee))
        {
            melee.TakeDamage(damage, transform);
        }
        else if (other.TryGetComponent<RangePlayer>(out var range))
        {
            range.TakeDamage(damage, transform);
        }

        // انیمیشن برخورد
        animator.SetTrigger("Touch");

        // بعد از اجرای انیمیشن حذف شود
        Destroy(gameObject, 0.3f);
    }
}