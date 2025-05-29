using System.Collections;
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    [Header("Movement")]
    public float distanceAttack = 10f;
    public float chaseSpeed = 6f;
    public float moveSpeed = 3f;

    public Transform leftboundry, rightboundry;
    public bool facingRight;

    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public MeleePlayer meleePlayer;

    [Header("Health")]
    private float currentHealth;
    private float maxHealth = 100f;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    private float attackTimer = 0f;
    public LayerMask playerLayer;

    private float waitingTime;
    private float patrolingTime;

    private bool isChasing = false;

    private void Start()
    {
        currentHealth = maxHealth;
        leftboundry.parent = null;
        rightboundry.parent = null;

        patrolingTime = Random.Range(2f, 3.5f);
        waitingTime = Random.Range(1f, 2f);
        facingRight = true;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, meleePlayer.transform.position);
        
        attackTimer -= Time.deltaTime;

        if (IsPlayerInSight() && distanceToPlayer < distanceAttack)
        {
            isChasing = true;
            if (distanceToPlayer <= attackRange)
            {
                Attack();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            isChasing = false;
            if (patrolingTime > 0)
            {
                Patrol();
                patrolingTime -= Time.deltaTime;
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // توقف در حالت انتظار
                waitingTime -= Time.deltaTime;
                if (waitingTime <= 0)
                {
                    patrolingTime = Random.Range(2f, 3.5f);
                    waitingTime = Random.Range(1f, 2f);
                }
            }
        }
    }

    private bool IsPlayerInSight()
    {
        Vector2 directionToPlayer = meleePlayer.transform.position - transform.position;
        return (facingRight && directionToPlayer.x > 0) || (!facingRight && directionToPlayer.x < 0);
    }

    private void ChasePlayer()
    {
        Vector2 direction = (meleePlayer.transform.position - transform.position).normalized;

        if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight))
        {
            Flip();
        }

        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);
    }

    private void Patrol()
    {
        float velocityX = facingRight ? moveSpeed : -moveSpeed;
        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);

        if (facingRight && transform.position.x > rightboundry.position.x)
        {
            Flip();
        }
        else if (!facingRight && transform.position.x < leftboundry.position.x)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.eulerAngles = new Vector3(0, facingRight ? 0 : 180, 0);
    }

    
    
    private void Attack()
    {
        if (attackTimer > 0f) return;

        // اجرای حمله
        Debug.Log("Enemy Attacks!");

        // بررسی برخورد با بازیکن
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);
        if (hitPlayer != null)
        {
            Debug.Log("Player hit!");
            hitPlayer.GetComponent<MeleePlayer>().TakeDamage(10); // اگر متد مربوطه وجود دارد
        }

        attackTimer = attackCooldown;
    }

    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log(currentHealth);
        if (currentHealth <= 0)
        {
            Debug.Log("Enemy died");
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
