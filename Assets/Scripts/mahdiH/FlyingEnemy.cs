using System.Collections;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Flight Settings")]
    public float patrolSpeed = 2f;
    public float dashForce = 15f;
    public float dashCooldown = 3f;
    public float returnDuration = 1.5f;
    public float detectionRange = 8f;
    public float damageDistance = 0.5f;

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
    private Transform targetPlayer;

    private void Start()
    {
        originalPosition = transform.position;
        currentHealth = maxHealth;
        rb.gravityScale = 0f; // Ensure floating
    }

    private void Update()
    {
        if (isDead || isDashing || isReturning || isOnCooldown) return;

        float meleeDist = Vector2.Distance(transform.position, meleePlayer.position);
        float rangeDist = Vector2.Distance(transform.position, rangePlayer.position);
        targetPlayer = meleeDist < rangeDist ? meleePlayer : rangePlayer;

        if (Vector2.Distance(transform.position, targetPlayer.position) <= detectionRange)
        {
            rb.linearVelocity = Vector2.zero; // Freeze in air before dashing
            StartCoroutine(AttackDash());
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        float offset = Mathf.Sin(Time.time * patrolSpeed) * 0.5f;
        transform.position = originalPosition + new Vector3(offset, 0f, 0f);
    }

    private IEnumerator AttackDash()
    {
        isDashing = true;

        Vector2 dashDirection = (targetPlayer.position - transform.position).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);

        float dashTime = 0.5f;
        float elapsed = 0f;

        while (elapsed < dashTime)
        {
            if (isDead) yield break;

            if (Vector2.Distance(transform.position, targetPlayer.position) <= damageDistance)
            {
                if (targetPlayer.TryGetComponent<MeleePlayer>(out MeleePlayer melee))
                    melee.TakeDamage(20 , transform);

                if (targetPlayer.TryGetComponent<RangePlayer>(out RangePlayer range))
                    range.TakeDamage(20 , transform);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

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
        animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject , 0.3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageDistance);
    }
}
