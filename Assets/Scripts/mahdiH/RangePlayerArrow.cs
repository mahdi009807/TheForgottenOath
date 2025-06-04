using UnityEngine;

public class RangePlayerArrow : MonoBehaviour
{
    public float destroyDelay = 3f; // برای غیر دشمن
    private Rigidbody2D rb;
    private bool hasHit = false;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // مطمئن شو که در ابتدا هیچ انیمیشنی اجرا نمی‌شود
        if (animator != null)
        {
            animator.Play("Idle", 0); // نام انیمیشن پیش‌فرض که بدون لرزش است
        }
    }

    public void Launch(Vector2 direction, float force)
    {
        // جهت گرافیکی تیر
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        rb.linearVelocity = direction * force;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        hasHit = true;

        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        if (collision.CompareTag("Enemy"))
        {
            // تیر بچسبد به دشمن
            transform.parent = collision.transform;

            // محاسبه جهت عقب تیر (خلاف جهت جلو تیر)
            Vector2 backwardDirection = -transform.right; // چون در 2D راست، جلو تیر هست

            // مقدار کمی داخل بدن دشمن حرکت کن
            float inwardAmount = 0.1f; // مقدار جابجایی
            transform.position += (Vector3)(backwardDirection.normalized * inwardAmount);

            // آسیب به دشمن
            FlyingEnemy flying = collision.GetComponent<FlyingEnemy>();
            if (flying != null)
            {
                flying.TakeDamage(1); // یا مقدار مناسب مثلاً 10 یا 20
            }
            
            MeleeEnemy enemy = collision.GetComponent<MeleeEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }
            
            DogWolfEnemy dog = collision.GetComponent<DogWolfEnemy>();
            if (dog != null)
            {
                dog.TakeDamage(1); // یا مقدار دلخواه
            }


            // if (collision.CompareTag("Knight"))
            // {
            //     Physics2D.IgnoreCollision (collision.GetComponent<Collider2D>(), GetComponent<Collider2D>() , true);
            // }
        }
        else
        {
            // اگر غیر دشمن بود، فقط لرزش کن و حذف شو بعد از تاخیر
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Shake");
            }
            Destroy(gameObject, destroyDelay);
            return;
        }

        // لرزش تیر فقط یک بار هنگام برخورد
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger("Shake");
        }
    }

}