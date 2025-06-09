using System.Collections;
using UnityEngine;

public class EnemyArcher : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRange = 10f;
    public float shootInterval = 2f;
    public GameObject arrowPrefab;
    public Transform shootPoint;
    public int damage = 15;
    public int maxHealth = 50;

    [Header("References")]
    public Transform meleePlayer;
    public Transform rangePlayer;
    public Animator animator;
    
    [Header("Knockback Settings")]
    public float knockbackForceX = 4f;
    public float knockbackForceY = 3f;
    public float knockbackDuration = 0.2f;

    private bool isKnockedBack = false;
    private Rigidbody2D rb;


    private float nextShootTime;
    [SerializeField]private int currentHealth;
    private bool isDead = false;
    [SerializeField] private float DieDuration;
    
    public GameObject[] Collectibles;

    private void Start()
    {
        
        meleePlayer = PlayerRegistry.Knight;
        rangePlayer = PlayerRegistry.Archer;

        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        // چرخاندن به سمت چپ در شروع بازی
        if (transform.localScale.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }



    private void Update()
    {
        if (isDead) return;

        float distToMelee = Vector2.Distance(transform.position, meleePlayer.position);
        float distToRange = Vector2.Distance(transform.position, rangePlayer.position);

        bool meleeInRange = distToMelee <= detectionRange;
        bool rangeInRange = distToRange <= detectionRange;

        if ((meleeInRange || rangeInRange) && Time.time >= nextShootTime)
        {
            Transform target = distToMelee < distToRange ? meleePlayer : rangePlayer;
            ShootAt(target);
            nextShootTime = Time.time + shootInterval;
        }
    }

    private void ShootAt(Transform target)
    {
        animator.SetTrigger("Shoot");
        StartCoroutine(DelayedShoot(target));
    }

    private IEnumerator DelayedShoot(Transform target)
    {
        // مقدار این تأخیر را با زمان رها شدن کمان در انیمیشن تنظیم کن
        yield return new WaitForSeconds(0.8f);

        if (isDead) yield break;

        Vector2 direction = (target.position - shootPoint.position).normalized;

        // چرخش آرچر به سمت هدف
        if (direction.x > 0 && transform.localScale.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        else if (direction.x < 0 && transform.localScale.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        // Instantiate تیر بعد از تاخیر
        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
        arrow.GetComponent<EnemyArrow>().Initialize(direction, damage);
    }



    public void TakeDamage(int dmg, Transform attacker)
    {
        if (isDead || isKnockedBack) return;

        currentHealth -= dmg;
        animator.SetTrigger("Hurt");

        Vector2 direction = (transform.position - attacker.position).normalized;
        StartCoroutine(ApplyKnockback(direction));

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;

        Vector2 knockDir = new Vector2(direction.x, 1f).normalized;
        rb.linearVelocity = Vector2.zero; // reset velocity
        rb.AddForce(new Vector2(knockDir.x * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }



    private void Die()
    {
        int random1 = Random.Range(0, Collectibles.Length);
        int random2 = Random.Range(0, Collectibles.Length);
        Vector3 pos = transform.position + new Vector3(0f, 1f, 0f);
        Instantiate(Collectibles[random1], pos, Quaternion.identity);
        Instantiate(Collectibles[random2], pos, Quaternion.identity);
        
        isDead = true;
        animator.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // دیگه فیزیک روش اعمال نشه
        StartCoroutine(SinkIntoGround());
    }
    
    private IEnumerator SinkIntoGround()
    {
        yield return new WaitForSeconds(DieDuration);
        float duration = 1.5f; // مدت زمان فرو رفتن
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.down * 0.5f; // نصف واحد به پایین

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        Destroy(gameObject);
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
