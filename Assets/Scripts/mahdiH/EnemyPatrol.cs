using System.Collections;
using UnityEngine;

public class EnemyPatroller : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform leftPoint;
    public Transform rightPoint;
    public float walkSpeed = 2f;
    public float idleDuration = 2f;
    private Transform currentTarget;

    [Header("Chase & Attack Settings")]
    public float detectionRange = 10f;
    public float chaseSpeed = 4f;
    public float attackCooldown = 2f;
    public Transform attackRangeCheck;
    public float attackRadius = 2f;
    public int damage = 10;

    [Header("Health Settings")]
    public int maxHealth = 50;
    private int currentHealth;
    private bool isDead = false;

    [Header("References")]
    public Transform meleePlayer;
    public Transform rangePlayer;
    public Animator animator;

    private Transform targetPlayer;
    private bool canAttack = true;
    private bool isChasing = false;
    private bool isIdle = false;
    
    public GameObject[] Collectibles;

    void Start()
    {
        leftPoint.parent = null;
        rightPoint.parent = null;
        currentHealth = maxHealth;
        currentTarget = rightPoint;
        StartCoroutine(PatrolRoutine());
    }

    void Update()
    {
        if (isDead) return;

        float distToMelee = Vector2.Distance(transform.position, meleePlayer.position);
        float distToRange = Vector2.Distance(transform.position, rangePlayer.position);

        bool meleeInRange = distToMelee <= detectionRange;
        bool rangeInRange = distToRange <= detectionRange;

        if (meleeInRange || rangeInRange)
        {
            isChasing = true;
            targetPlayer = distToMelee < distToRange ? meleePlayer : rangePlayer;
        }
        else
        {
            isChasing = false;
            targetPlayer = null;
        }

        if (isChasing)
        {
            StopAllCoroutines();
            ChasePlayer();
        }

        if (isChasing && Vector2.Distance(transform.position, targetPlayer.position) <= attackRadius)
        {
            TryAttack();
        }
    }

    private IEnumerator PatrolRoutine()
    {
        while (!isDead)
        {
            animator.SetBool("Run", false);
            animator.SetBool("Walk", true);

            while (Vector2.Distance(transform.position, currentTarget.position) > 0.1f)
            {
                Vector2 dir = (currentTarget.position - transform.position).normalized;
                transform.position += new Vector3(dir.x, 0f, 0f) * walkSpeed * Time.deltaTime;
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (dir.x > 0 ? 1 : -1);
                transform.localScale = scale;

                yield return null;
            }

            animator.SetBool("Walk", false);
            animator.SetTrigger("Idle");
            yield return new WaitForSeconds(idleDuration);

            currentTarget = currentTarget == leftPoint ? rightPoint : leftPoint;
        }
    }

    private void ChasePlayer()
    {
        if (targetPlayer == null || isDead) return;

        Vector2 dir = (targetPlayer.position - transform.position).normalized;
        transform.position += new Vector3(dir.x, 0f, 0f) * chaseSpeed * Time.deltaTime;
        transform.localScale = new Vector3(dir.x > 0 ? 1 : -1, 1, 1);
        animator.SetBool("Run", true);
        animator.SetBool("Walk", false);
    }

    private void TryAttack()
    {
        if (!canAttack) return;

        canAttack = false;
        animator.SetTrigger("Attack");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackRangeCheck.position, attackRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent<MeleePlayer>(out var melee))
                    melee.TakeDamage(damage , transform);
                else if (hit.TryGetComponent<RangePlayer>(out var range))
                    range.TakeDamage(damage , transform);
            }
        }

        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;

        if (!isChasing && !isDead)
            StartCoroutine(PatrolRoutine());
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        animator.SetTrigger("TakeDamage");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        int random1 = Random.Range(0, Collectibles.Length);
        int random2 = Random.Range(0, Collectibles.Length);
        Vector3 pos = transform.position + new Vector3(0f, 1f, 0f);
        Instantiate(Collectibles[random1], pos, Quaternion.identity);
        Instantiate(Collectibles[random2], pos, Quaternion.identity);
        
        isDead = true;
        animator.SetTrigger("Death");
        StartCoroutine(SinkAndDestroy());
    }

    private IEnumerator SinkAndDestroy()
    {
        float sinkSpeed = 0.5f;
        while (transform.position.y > -10f)
        {
            transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (attackRangeCheck != null)
            Gizmos.DrawWireSphere(attackRangeCheck.position, attackRadius);
    }
}
