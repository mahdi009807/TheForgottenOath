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
    public int maxHealth = 3;

    [Header("References")]
    public Transform meleePlayer;
    public Transform rangePlayer;
    public Animator animator;

    private float nextShootTime;
    private int currentHealth;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
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



    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        Destroy(gameObject, 2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
