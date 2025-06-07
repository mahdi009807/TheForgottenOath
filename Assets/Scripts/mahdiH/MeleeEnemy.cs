using System.Collections;
using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float chaseSpeed = 6f;
    public Transform leftPoint, rightPoint;

    [Header("Idle Timing")]
    public float patrolDuration = 4f;
    public float idleDuration = 1.5f;

    [Header("Detection Settings")]
    public float detectionRange = 8f;
    public float attackRange = 2.5f;
    public float fieldOfViewAngle = 150f;

    [Header("Players")]
    public Transform meleePlayer;
    public Transform rangePlayer;

    private Transform closestPlayer;

    [Header("Attack Settings")]
    public float attackCooldown = 0.5f;
    private float attackTimer;
    private bool isPerformingAttack;

    [Header("Health Settings")]
    public float maxHealth = 100;
    public float currentHealth;
    private bool isDead;
    private bool isHurt;

    [Header("Knockback Settings")]
    public float knockbackForceX = 8f;
    public float knockbackForceY = 5f;
    public float knockbackDuration = 0.3f;
    private bool isKnockedBack = false;

    [Header("Components")]
    public SpriteRenderer sprite;
    public Animator animator;
    public Rigidbody2D rb;

    [Header("Attack Area")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public LayerMask playerLayer;

    private bool facingRight = true;
    private bool isIdle;
    private bool isChasing;
    private bool isAttacking;
    private float patrolTimer;
    private float idleTimer;
    private bool isWaitingAfterAttack = false;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    

    private void Start()
    {
        leftPoint.parent = null;
        rightPoint.parent = null;
        currentHealth = maxHealth;
        patrolTimer = patrolDuration;
    }

    private void Update()
    {
        if (isDead || isHurt || isPerformingAttack) return;

        if (isIdle)
        {
            idleTimer -= Time.deltaTime;
            animator.SetBool("Idle", true);
            animator.SetFloat("Run", 0);
            if (idleTimer <= 0f)
            {
                isIdle = false;
                patrolTimer = patrolDuration;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isHurt || isKnockedBack) return;

        if (isPerformingAttack)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // تعیین نزدیک‌ترین بازیکن (مقاوم در برابر null شدن بازیکنان مرده)
        if (meleePlayer != null && rangePlayer != null)
        {
            float meleeDist = Vector2.Distance(transform.position, meleePlayer.position);
            float rangeDist = Vector2.Distance(transform.position, rangePlayer.position);
            closestPlayer = meleeDist < rangeDist ? meleePlayer : rangePlayer;
        }
        else if (meleePlayer != null)
        {
            closestPlayer = meleePlayer;
        }
        else if (rangePlayer != null)
        {
            closestPlayer = rangePlayer;
        }
        else
        {
            closestPlayer = null;
        }

        // اگر هیچ بازیکنی نبود فقط گشت‌زنی کند
        if (closestPlayer == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, closestPlayer.position);
        bool inSight = IsPlayerInSight(closestPlayer);

        if (distanceToPlayer <= attackRange && attackTimer <= 0f && inSight)
        {
            StartCoroutine(PerformAttack());
        }
        else if (distanceToPlayer <= detectionRange && inSight)
        {
            isChasing = true;
            isIdle = false;

            if (distanceToPlayer <= attackRange)
            {
                StartCoroutine(PerformAttack());
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            isChasing = false;
            Patrol();
        }

        attackTimer -= Time.fixedDeltaTime;
    }


    private void Patrol()
    {
        float dir = facingRight ? 1f : -1f;
        transform.position += Vector3.right * dir * moveSpeed * Time.deltaTime;

        animator.SetFloat("Run", 0);
        animator.SetBool("Idle", false);
        sprite.flipX = !facingRight;

        if (facingRight && transform.position.x >= rightPoint.position.x)
            facingRight = false;
        else if (!facingRight && transform.position.x <= leftPoint.position.x)
            facingRight = true;

        UpdateAttackPoint();
    }

    private void ChasePlayer()
    {
        Vector3 targetPos = new Vector3(closestPlayer.position.x, transform.position.y);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);

        animator.SetFloat("Run", 1);
        animator.SetBool("Idle", false);

        facingRight = closestPlayer.position.x > transform.position.x;
        sprite.flipX = !facingRight;

        UpdateAttackPoint();
    }

    private void UpdateAttackPoint()
    {
        Vector3 pos = attackPoint.localPosition;
        pos.x = Mathf.Abs(pos.x) * (facingRight ? 1 : -1);
        attackPoint.localPosition = pos;
    }

    private IEnumerator PerformAttack()
    {
        isPerformingAttack = true;
        isChasing = false;
        isIdle = false;

        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger("Attack");
        animator.SetBool("Idle", false);
        animator.SetFloat("Run", 0);

        facingRight = closestPlayer.position.x > transform.position.x;
        sprite.flipX = !facingRight;
        UpdateAttackPoint();

        yield return new WaitForSeconds(0.5f);
        DoAttackHit();

        float remainingCooldown = attackCooldown - 0.5f;
        if (remainingCooldown > 0)
            yield return new WaitForSeconds(remainingCooldown);

        isPerformingAttack = false;
        attackTimer = attackCooldown;
    }

    public void DoAttackHit()
    {
        if (isDead) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (Collider2D hit in hits)
        {
            MeleePlayer melee = hit.GetComponent<MeleePlayer>();
            if (melee != null)
            {
                melee.TakeDamage(25, transform);
                continue;
            }

            RangePlayer range = hit.GetComponent<RangePlayer>();
            if (range != null)
            {
                range.TakeDamage(25, transform);
            }
        }
    }

    private bool IsPlayerInSight(Transform target)
    {
        Vector2 dirToPlayer = (target.position - transform.position).normalized;
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(forward, dirToPlayer);
        return angle < fieldOfViewAngle * 0.5f;
    }

    public void TakeDamage(float damage, Transform attacker)
    {
        if (isDead || isKnockedBack) return;

        currentHealth -= damage;
        animator.SetTrigger("Hit");

        float direction = Mathf.Sign(transform.position.x - attacker.position.x);
        StartCoroutine(ApplyKnockback(direction));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator ApplyKnockback(float direction)
    {
        isKnockedBack = true;
        float timer = 0f;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        transform.position += new Vector3(0f, knockbackForceY * Time.fixedDeltaTime, 0f);

        while (timer < knockbackDuration)
        {
            transform.position += new Vector3(direction * knockbackForceX * Time.deltaTime, 0f, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.collider.TryGetComponent<MeleePlayer>(out MeleePlayer melee))
        {
            melee.TakeDamage(20, transform);
        }

        if (collision.collider.TryGetComponent<RangePlayer>(out RangePlayer range))
        {
            range.TakeDamage(20, transform);
        }
    }

    private void Die()
    {
        isDead = true;

        animator.SetBool("Idle", false);
        animator.SetFloat("Run", 0);
        animator.SetBool("Dead", true);

        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        foreach (Transform child in transform)
        {
            RangePlayerArrow arrow = child.GetComponent<RangePlayerArrow>();
            if (arrow != null)
                Destroy(arrow.gameObject, 0.3f);
        }

        this.enabled = false;
        Destroy(gameObject, 2f);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        Gizmos.color = Color.yellow;
        Vector3 rightDir = Quaternion.Euler(0, 0, fieldOfViewAngle / 2) * (facingRight ? Vector2.right : Vector2.left);
        Vector3 leftDir = Quaternion.Euler(0, 0, -fieldOfViewAngle / 2) * (facingRight ? Vector2.right : Vector2.left);

        Gizmos.DrawRay(transform.position, rightDir * detectionRange);
        Gizmos.DrawRay(transform.position, leftDir * detectionRange);
    }
}
