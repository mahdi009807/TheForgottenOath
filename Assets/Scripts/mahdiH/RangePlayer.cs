using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class RangePlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    private float input = 0f;
    public bool facingRight = true;

    [Header("Jump Settings")]
    public float jumpForce = 6f;
    public int maxJumpCount = 2;
    private int currentJumpCount = 0;
    private bool isGrounded = false;


    [Header("Components")]
    public Animator animator;
    public Rigidbody2D rb;
    public Transform groundCheck; // Assign in Inspector
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    
    [Header("Arrow Settings")]
    public Transform firePoint;
    public GameObject arrowPrefab;
    public float maxChargeTime = 2f;
    public float minLaunchForce = 5f;
    public float maxLaunchForce = 20f;

    private float currentChargeTime = 0f;
    private bool isCharging = false;
    // public PolygonCollider2D StandCollider;
    // public PolygonCollider2D IsAimingCollider;
    
    
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;
    
    [Header("Knockback")]
    public float knockbackForceX = 7f;
    public float knockbackForceY = 4f;
    public float knockbackDuration = 0.3f;
    private bool isKnockedBack = false;
    
    [Header("Wall Slide")]
    public Transform leftWallCheck;
    public Transform rightWallCheck;
    public float wallCheckDistance = 0.3f;
    public float wallSlideSpeed = 1.5f;
    public LayerMask wallLayer;
    public Vector2 wallJumpForce = new Vector2(10f, 12f); // x = افقی به بیرون، y = به بالا

    

    private bool isTouchingWall;
    [SerializeField]private bool isWallSliding;
    private int wallSlideSide; // -1 = left, 1 = right







    private PlayerControler RangeControler;

    private void Awake()
    {
        RangeControler = new PlayerControler();
        
        currentHealth = maxHealth;
        
        // StandCollider.enabled = true;
        // IsAimingCollider.enabled = false;

        // ورودی حرکت
        RangeControler.Range.Move.performed += ctx => input = ctx.ReadValue<float>();
        RangeControler.Range.Move.canceled += ctx => input = 0f;
        RangeControler.Range.Jump.performed += ctx => Jump();
        RangeControler.Range.Aim.started += ctx => StartCharging();
        RangeControler.Range.Aim.canceled += ctx => ReleaseArrow();
        

    }

    private void Update()
    {
        if (isDead || isKnockedBack) return;

        // تغییر جهت بازیکن
        if (facingRight && input < 0)
        {
            facingRight = false;
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
        else if (!facingRight && input > 0)
        {
            facingRight = true;
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        // بررسی تماس با زمین
        bool previousGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!previousGrounded && isGrounded)
        {
            currentJumpCount = 0;
            animator.SetBool("Jump", false);
        }

        // انیمیشن دویدن
        animator.SetFloat("Run", Mathf.Abs(input) > 0.1f ? 1 : 0);

        // شارژ تیر در حال Aim
        if (isCharging)
        {
            currentChargeTime += Time.deltaTime;
            currentChargeTime = Mathf.Clamp(currentChargeTime, 0f, maxChargeTime);
        }
        
        CheckWallSlide();

    }



    private void FixedUpdate()
    {
        if (isDead || isKnockedBack) return;
        
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }



        float speedMultiplier = isCharging ? 0.5f : 1f;
        transform.position += new Vector3(input, 0f, 0f) * moveSpeed * speedMultiplier * Time.fixedDeltaTime;
    }
    
    
    private void OnEnable()
    {
        RangeControler.Enable();
    }

    private void OnDisable()
    {
        RangeControler.Disable();
    }


    private void Jump()
    {
        if (isWallSliding)
        {
            // جهت پرش: اگر بازیکن به دیوار سمت چپ چسبیده، به سمت راست بپره و برعکس
            int direction = wallSlideSide == -1 ? 1 : -1;

            rb.linearVelocity = new Vector2(wallJumpForce.x * direction, wallJumpForce.y);

            // تنظیم جهت دید
            facingRight = direction == 1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);

            animator.SetBool("Jump", true);

            // قطع حالت wall slide
            isWallSliding = false;
            animator.SetBool("WallSlideLeft", false);
            animator.SetBool("WallSlideRight", false);

            return;
        }

        // پرش معمولی
        if (currentJumpCount < maxJumpCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("Jump", true);
            currentJumpCount++;
        }
    }

    
    private void CheckWallSlide()
    {
        bool leftHit = Physics2D.Raycast(leftWallCheck.position, Vector2.left, wallCheckDistance, wallLayer);
        bool rightHit = Physics2D.Raycast(rightWallCheck.position, Vector2.right, wallCheckDistance, wallLayer);

        isTouchingWall = leftHit || rightHit;
        wallSlideSide = leftHit ? -1 : (rightHit ? 1 : 0);

        isWallSliding = isTouchingWall && !isGrounded && rb.linearVelocity.y < 0;

        if (isWallSliding)
        {
            if (wallSlideSide == 1) animator.SetBool("WallSlideRight" , true);
            else animator.SetBool("WallSlideLeft", true);
        }
        else
        {
            animator.SetBool("WallSlideLeft", false);
            animator.SetBool("WallSlideRight", false);
        }
    }

    
    
    private void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0f;
        animator.SetBool("IsAiming", true);
        // StandCollider.enabled = false;
        // IsAimingCollider.enabled = true;
    }

    private void ReleaseArrow()
    {
        if (!isCharging) return;

        isCharging = false;
        animator.SetBool("IsAiming", false);
        // StandCollider.enabled = true;   
        // IsAimingCollider.enabled = false;

        float launchForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, currentChargeTime / maxChargeTime);

        Vector2 spawnPos = firePoint.position + (facingRight ? Vector3.right : Vector3.left) * 0.5f;
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity); // از firePoint.rotation استفاده نکن

        // جهت و چرخش
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        arrow.GetComponent<RangePlayerArrow>().Launch(direction, launchForce);
    }

    
    
    public void TakeDamage(int damage, Transform attacker)
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

        // Reset Y velocity and apply upward knock
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




    
    private void Die()
    {
        isDead = true;
        input = 0f; // متوقف کردن حرکت
        animator.SetBool("Dead", true);
        RangeControler.Disable(); // غیرفعال کردن کنترل‌ها
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        Destroy(gameObject, 2f); // حذف بعد از 2 ثانیه
    }

    
    public bool IsDead() => isDead;


    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}



