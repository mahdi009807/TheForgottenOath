using System.Collections;
using UnityEngine;

public class DogWolfEnemy : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRange = 8f;
    public float verticalDetectionRange = 4f;
    public float chaseSpeed = 4f;
    public float returnSpeed = 2f;
    public float stopAfterHitDuration = 1.5f;
    public float knockbackForce = 3f;
    public float knockbackDuration = 0.3f;
    public int damage = 20;
    public int maxHealth = 3;
    private Coroutine returnCoroutine;
    // [SerializeField]private float attackingAnimationCooldown;
    // [SerializeField] private bool AttackingTime = false;


    [Header("References")]
    public Transform meleePlayer;
    public Transform rangePlayer;
    public Animator animator;

    private int currentHealth;
    private Vector3 startPosition;
    private Transform targetPlayer;
    private bool isChasing = false;
    private bool isReturning = false;
    private bool isStoppedAfterHit = false;
    private bool isKnockedBack = false;
    private bool isDead = false;
    private bool canAttack = true;
    
    public GameObject[] Collectibles;

    void Start()
    {
        meleePlayer = PlayerRegistry.Knight;
        rangePlayer = PlayerRegistry.Archer;

        currentHealth = maxHealth;
        startPosition = transform.position;
    }

    void Update()
    {
        if (isDead || isKnockedBack) return;
        
        // if (AttackingTime) return; 

        float distToMelee = Vector2.Distance(transform.position, meleePlayer.position);
        float distToRange = Vector2.Distance(transform.position, rangePlayer.position);
        float verticalDistMelee = Mathf.Abs(transform.position.y - meleePlayer.position.y);
        float verticalDistRange = Mathf.Abs(transform.position.y - rangePlayer.position.y);
        
        bool isMeleeAlive = meleePlayer != null && !meleePlayer.GetComponent<MeleePlayer>().IsDead();
        bool isRangeAlive = rangePlayer != null && !rangePlayer.GetComponent<RangePlayer>().IsDead();


        bool meleeInRange = isMeleeAlive && distToMelee <= detectionRange && verticalDistMelee <= verticalDetectionRange;
        bool rangeInRange = isRangeAlive && distToRange <= detectionRange && verticalDistRange <= verticalDetectionRange;


        if ((meleeInRange || rangeInRange) && !isDead)
        {
            if (meleeInRange && rangeInRange)
            {
                targetPlayer = distToMelee < distToRange ? meleePlayer : rangePlayer;
            }
            else if (meleeInRange)
            {
                targetPlayer = meleePlayer;
            }
            else if (rangeInRange)
            {
                targetPlayer = rangePlayer;
            }
            
            isChasing = true;
            isReturning = false;

            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
                returnCoroutine = null;
            }
        }

        else if (!meleeInRange && !rangeInRange && !isReturning && isChasing && !isStoppedAfterHit)
        {
            isChasing = false;
            returnCoroutine = StartCoroutine(ReturnToStart());
        }


        if (isChasing && !isStoppedAfterHit)
        {
            Vector2 direction = (targetPlayer.position - transform.position).normalized;
            transform.position += new Vector3(direction.x * chaseSpeed * Time.deltaTime, 0f, 0f);
            SetRun();

            if ((direction.x > 0 && transform.localScale.x < 0) || (direction.x < 0 && transform.localScale.x > 0))
            {
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }
        }
        else if (!isReturning)
        {
            SetIdle();
        }

        CheckForAttack();
        
        // اگر هیچ حالتی فعال نیست و دشمن هنوز به مکان اولیه برنگشته، برگرد به نقطه شروع
        if (!isChasing && !isReturning && !isStoppedAfterHit && Vector2.Distance(transform.position, startPosition) > 0.1f)
        {
            returnCoroutine = StartCoroutine(ReturnToStart());
        }

    }

    private void CheckForAttack()
    {
        if (isChasing && !isKnockedBack && !isDead && canAttack && targetPlayer != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, targetPlayer.position);

            if (distanceToTarget <= 1f)
            {
                animator.SetTrigger("Attack");

                // StartCoroutine(waitForAttacking());

                if (targetPlayer.TryGetComponent<MeleePlayer>(out MeleePlayer melee))
                    melee.TakeDamage(damage , transform);
                if (targetPlayer.TryGetComponent<RangePlayer>(out RangePlayer range))
                    range.TakeDamage(damage , transform);

                StartCoroutine(StopAfterHit());
            }
        }
    }

    // private IEnumerator waitForAttacking()
    // {
    //     AttackingTime = true;
    //     yield return new WaitForSeconds(attackingAnimationCooldown);
    //     AttackingTime = false;
    // }

    private IEnumerator StopAfterHit()
    {
        isStoppedAfterHit = true;
        canAttack = false;
        yield return new WaitForSeconds(stopAfterHitDuration);
        canAttack = true;
        isStoppedAfterHit = false;
    }

    private IEnumerator ReturnToStart()
    {
        isReturning = true;
        SetWalk();

        while (Vector2.Distance(transform.position, startPosition) > 1f)
        {
            Vector2 direction = (startPosition - transform.position).normalized;
            transform.position += new Vector3(direction.x * returnSpeed * Time.deltaTime, 0f, 0f);

            // Flip
            if ((direction.x > 0 && transform.localScale.x < 0) || (direction.x < 0 && transform.localScale.x > 0))
            {
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }

            yield return null;
        }

        // Debug.Log("yes");
        isReturning = false;
        SetIdle();
    }


    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        StartCoroutine(ApplyKnockback());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator ApplyKnockback()
    {
        isKnockedBack = true;

        float direction = transform.localScale.x > 0 ? -1f : 1f;
        float timer = 0f;

        while (timer < knockbackDuration)
        {
            transform.position += new Vector3(direction * knockbackForce * Time.deltaTime, 0f, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

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
        Destroy(gameObject, 2f);
    }

    private void SetIdle()
    {
        animator.SetBool("Run", false);
        animator.SetBool("Walk", false);
        animator.SetBool("Idle", true);
    }

    private void SetRun()
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Run", true);
        animator.SetBool("Walk", false);
    }

    private void SetWalk()
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Run", false);
        animator.SetBool("Walk", true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
