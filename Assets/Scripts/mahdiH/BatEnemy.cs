using System.Collections;
using UnityEngine;

public class BatEnemy : MonoBehaviour
{
    [Header("Flight Settings")]
    public float dashForce = 15f;
    public float dashCooldown = 3f;
    public float returnDuration = 1.5f;
    public float detectionRange = 8f;     // برای حمله
    public float wakeUpRange = 12f;       // 🔸 برای بیدار شدن

    [Header("Health")]
    public int maxHealth = 1;
    private int currentHealth;
    private bool isDead = false;

    [Header("References")]
    public Transform meleePlayer;
    public Transform rangePlayer;
    public Rigidbody2D rb;
    public Animator animator;

    private Vector3 originalPosition;
    private bool isDashing = false;
    private bool isReturning = false;
    private bool isOnCooldown = false;
    private bool hasWokenUp = false;
    
    public GameObject[] Collectibles;

    private Transform targetPlayer;

    private void Start()
    {
        
        meleePlayer = PlayerRegistry.Knight;
        rangePlayer = PlayerRegistry.Archer;
        
        originalPosition = transform.position;
        currentHealth = maxHealth;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        if (isDead || isDashing || isReturning || isOnCooldown) return;

        float meleeDist = Vector2.Distance(transform.position, meleePlayer.position);
        float rangeDist = Vector2.Distance(transform.position, rangePlayer.position);
        targetPlayer = meleeDist < rangeDist ? meleePlayer : rangePlayer;

        float playerDistance = Vector2.Distance(transform.position, targetPlayer.position);

        // 🔸 فقط یک بار بیدار شود
        if (!hasWokenUp && playerDistance <= wakeUpRange)
        {
            hasWokenUp = true;
            animator.SetTrigger("WakeUp");
            return;
        }

        if (!hasWokenUp)
        {
            animator.SetTrigger("Sleep");
            return;
        }

        // 🔸 حمله فقط در محدوده حمله
        if (playerDistance <= detectionRange)
        {
            FaceTarget();
            rb.linearVelocity = Vector2.zero;
            StartCoroutine(PrepareAndDash());
        }
        else
        {
            animator.SetTrigger("Idle");
        }
    }

    private void FaceTarget()
    {
        if (targetPlayer == null) return;

        Vector3 scale = transform.localScale;
        scale.x = targetPlayer.position.x > transform.position.x ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private IEnumerator PrepareAndDash()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(0.5f);

        if (Vector2.Distance(transform.position, targetPlayer.position) > detectionRange)
        {
            animator.SetTrigger("Idle");
            isOnCooldown = false;
            yield break;
        }

        StartCoroutine(AttackDash());
    }

    private IEnumerator AttackDash()
    {
        isDashing = true;
        FaceTarget();

        Vector2 dashDirection = (targetPlayer.position - transform.position).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);

        animator.SetTrigger(dashDirection.x > 0 ? "RightAttack" : "LeftAttack");

        yield return new WaitForSeconds(0.5f);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        StartCoroutine(ReturnToStart());
    }

    private IEnumerator ReturnToStart()
    {
        isReturning = true;
        Vector3 startPos = transform.position;
        float t = 0f;

        while (t < returnDuration)
        {
            transform.position = Vector3.Lerp(startPos, originalPosition, t / returnDuration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        isReturning = false;
        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(dashCooldown);
        isOnCooldown = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        int random = Random.Range(0, Collectibles.Length);
        Vector3 pos = transform.position + new Vector3(0f, 1f, 0f);
        Instantiate(Collectibles[random], pos, Quaternion.identity);
        
        isDead = true;
        animator.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 0.3f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDashing || isDead) return;

        if (collision.collider.TryGetComponent<MeleePlayer>(out var melee))
            melee.TakeDamage(20, transform);

        else if (collision.collider.TryGetComponent<RangePlayer>(out var range))
            range.TakeDamage(20, transform);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wakeUpRange); // 🔹 محدوده بیدار شدن

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // 🔸 محدوده حمله
    }
}
