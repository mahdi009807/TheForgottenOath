using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleePlayer : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float deceleration = 15f;  
    public float moveInput;
    private float currentVelocityX = 0f;
    private bool facingRight = true;
    private bool isRunning;
    private bool wasRunningLastFrame;

    [Header("Jumping")]
    public float jumpForce = 12f;
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;
    private bool wasGroundedLastFrame;
    private bool jumpPressed;

    [Header("References")]
    private Animator animator;
    public Rigidbody2D rb;
    // public PolygonCollider2D StandCollider;
    // public PolygonCollider2D LandingCollider;
    
    [Header("Combat")]
    public Transform firePoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public float attackDamage = 25f;
    
    [Header("Attack Timing")]
    public float attackDelay = 0.3f; // زمان تقریبی فعال شدن ضربه
    public float attackCooldown = 0.6f; // فاصله بین دو حمله
    private bool isAttacking = false;

    
    [Header("Health")]
    [SerializeField]private float maxHealth = 100f;
    [SerializeField]private float currecntHealth;
    [SerializeField]private bool isDead = false;
    
    [Header("Knockback")]
    public float knockbackForceX = 8f;
    public float knockbackForceY = 5f;
    public float knockbackDuration = 0.3f;
    private bool isKnockedBack = false;
    
    [Header("Wall Jump")]
    public Transform leftWallCheck;
    public Transform rightWallCheck;
    public float wallCheckDistance = 0.3f;
    public float wallSlideSpeed = 1f;
    public Vector2 wallJumpForce = new Vector2(10f, 12f);
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool wallJumping;
    private float wallJumpDirection;
    public LayerMask wallLayer;
    private bool wasWallSliding = false;
    private int wallSlideSide = 0; // -1: چپ، 1: راست
    
    
    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public TrailRenderer trailRenderer;
    public LayerMask dashCollisionMask;
    private bool isDashing = false;
    private bool canDash = true;
    
    [Header("AfterImage")]
    public GameObject afterImagePrefab;
    public float afterImageInterval = 0.05f; // هر چند ثانیه یک afterimage
    

    // Input System
    private PlayerControler controls;

    private void Awake()
    {
        currecntHealth = maxHealth;
        
        // StandCollider.enabled = true;
        // LandingCollider.enabled = false;
        
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        controls = new PlayerControler();

        controls.Melee.Move.performed += ctx => moveInput = ctx.ReadValue<float>();
        controls.Melee.Move.canceled += ctx => moveInput = 0f;

        controls.Melee.Jump.performed += ctx => jumpPressed = true;
        
        controls.Melee.Dash.performed += ctx => TryDash();
        

        controls.Melee.Attack.performed += ctx =>
        {
            if (CombatManager.instance != null)
            {
                CombatManager.instance.Attack(ctx);
            }
        };
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        if (isDead || isKnockedBack || isAttacking) return;
        
        if (!isWallSliding)
        {
            if (moveInput > 0 && !facingRight) Flip();
            else if (moveInput < 0 && facingRight) Flip();
        }


        // وضعیت دویدن (CapsLock + حرکت)
        isRunning = Keyboard.current.capsLockKey.isPressed && Mathf.Abs(moveInput) > 0.1f;
        float targetSpeed = (isRunning ? runSpeed : walkSpeed) * moveInput;

        // کاهش تدریجی سرعت وقتی input قطع شده
        if (Mathf.Abs(moveInput) < 0.1f)
        {
            currentVelocityX = Mathf.MoveTowards(currentVelocityX, 0, deceleration * Time.deltaTime);
        }
        else
        {
            currentVelocityX = targetSpeed;
        }
        
        // انیمیشن‌ها
        animator.SetFloat("Speed", Mathf.Abs(currentVelocityX) > 0.1 ? 1 : 0);
        animator.SetBool("IsRunning", isRunning);

        // کنترل ترایگرهای RunStart و RunEnd فقط وقتی وضعیت تغییر می‌کند
        if (isRunning && !wasRunningLastFrame && animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            animator.SetTrigger("RunStart");
        }
        else if (!isRunning && wasRunningLastFrame && animator.GetCurrentAnimatorStateInfo(0).IsName("RunLoop"))
        {
            animator.SetTrigger("RunEnd");
        }
        wasRunningLastFrame = isRunning;

        // تشخیص زمین
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // تشخیص سقوط (انیمیشن Fall)
        if (!isGrounded && rb.linearVelocity.y < -0.1f && !isWallSliding && !isDashing)
        {
            animator.SetTrigger("Fall");
        }

        // انیمیشن فرود
        if (!wasGroundedLastFrame && isGrounded)
        {
            StartCoroutine(Landing());
            
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        wasGroundedLastFrame = isGrounded;
        
        
        // کنترل اجرای حمله بدون Animation Event
        if (!isAttacking && CombatManager.instance.HasPendingInput())
        {
            TryAttack();
        }
        
        CheckWallSlide();


    }


    private void FixedUpdate()
    {
        if (isDead || isKnockedBack || isAttacking) return;

        // اعمال سرعت روی Rigidbody2D (حرکت افقی)
        transform.position += new Vector3(currentVelocityX * Time.fixedDeltaTime, 0, 0);

        // پرش از زمین
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }

        // پرش از دیوار
        if (jumpPressed && isWallSliding)
        {
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpForce.x, wallJumpForce.y);
            wallJumping = true;
            animator.SetTrigger("Jump"); // یا WallJump
        }

        // 🧱 کاهش سرعت سقوط در حین WallSlide
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(0, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }

        jumpPressed = false;
    }



    private void Flip()
    {
        facingRight = !facingRight;
        // transform.eulerAngles = new Vector3(0f, facingRight ? 0f : 180f, 0f);
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
    
    public bool IsRunning() => isRunning;
    

    private IEnumerator Landing()
    {
        animator.SetTrigger("Land");
        yield return new WaitForSeconds(0.3f);
    }
    
    private void CheckWallSlide()
    {
        // بررسی برخورد با دیوار چپ و راست
        bool leftHit = Physics2D.Raycast(leftWallCheck.position, Vector2.left, wallCheckDistance, wallLayer);
        bool rightHit = Physics2D.Raycast(rightWallCheck.position, Vector2.right, wallCheckDistance, wallLayer);

        isTouchingWall = leftHit || rightHit;
        wallJumpDirection = rightHit ? -1 : (leftHit ? 1 : 0);
        wallSlideSide = leftHit ? -1 : (rightHit ? 1 : 0);

        isWallSliding = isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && Mathf.Abs(moveInput) > 0.01f;

        if (isWallSliding)
        {
            if (!wasWallSliding)
            {
                if (wallSlideSide == -1)
                {
                    animator.SetTrigger("StartWallSlideLeft");
                }
                else if (wallSlideSide == 1)
                {
                    animator.SetTrigger("StartWallSlideRight");
                }
            }

            // فعال‌سازی فقط یک Loop بر اساس جهت
            animator.SetBool("WallSlideLeftLoop", wallSlideSide == -1);
            animator.SetBool("WallSlideRightLoop", wallSlideSide == 1);
        }
        else
        {
            animator.SetBool("WallSlideLeftLoop", false);
            animator.SetBool("WallSlideRightLoop", false);
        }

        wasWallSliding = isWallSliding;
    }




    
    public void DealDamage()
    {
        // شلیک یک دایره از firePoint برای برخورد با دشمنان
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<MeleeEnemy>(out MeleeEnemy e))
            {
                e.TakeDamage(attackDamage);
            }
            
            if (enemy.TryGetComponent<FlyingEnemy>(out FlyingEnemy flying))
            {
                flying.TakeDamage((int)attackDamage); // اگر نیاز بود می‌توانی تبدیل نوع را تغییر بدهی
            }
            
            if (enemy.TryGetComponent<DogWolfEnemy>(out DogWolfEnemy dog))
            {
                dog.TakeDamage((int)attackDamage);
            }


        }
    }
    
    private void TryAttack()
    {
        if (!isAttacking && CombatManager.instance.HasPendingInput())
        {
            CombatManager.instance.DisableInput();
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        int nextAttackNumber = Mathf.Clamp(CombatManager.instance.attackInputCount, 1, 4); // ما فقط Attack1 تا Attack4 داریم
        string triggerName = "Attack" + nextAttackNumber;
        animator.SetTrigger(triggerName);
        
        yield return new WaitForSeconds(attackDelay);
        DealDamage(); // دیگر نیازی به Animation Event نیست

        // صبر کن تا پایان حمله
        yield return new WaitForSeconds(attackCooldown - attackDelay);

        CombatManager.instance.ConsumeInput();
        isAttacking = false;
        CombatManager.instance.EnableInput();

        // حمله بعدی در صورت وجود
        TryAttack();
    }


    
    public void TakeDamage(float damage)
    {
        if (isDead || isAttacking) return;

        currecntHealth -= damage;
        animator.SetTrigger("Hurt");

        // اجرای ضربه‌ی برگشتی (Knockback)
        StartCoroutine(ApplyKnockback());

        if (currecntHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }
    
    private IEnumerator ApplyKnockback()
    {
        isKnockedBack = true;

        float direction = facingRight ? -1f : 1f;

        float timer = 0f;

        Vector3 knockbackVelocity = new Vector3(direction * knockbackForceX, knockbackForceY, 0);

        // پرش اولیه به سمت عقب (فقط یکبار، فقط محور Y)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // صفر کردن سرعت Y برای کنترل بیشتر
        transform.position += new Vector3(0f, knockbackForceY * Time.fixedDeltaTime, 0f);

        while (timer < knockbackDuration)
        {
            transform.position += new Vector3(direction * knockbackForceX * Time.deltaTime, 0f, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;
    }
    




    private IEnumerator Die()
    {
        isDead = true;
        animator.SetBool("Death", true);
        
        // منتظر بمان تا انیمیشن وارد وضعیت "Death" بشود
        // while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
        yield return new WaitForSeconds(3f);

        // اکنون منتظر بمان تا طول انیمیشن مرگ طی شود
        // yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // بعد از انیمیشن مرگ:
        controls.Disable();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        Destroy(gameObject);
    }
    
    
    private void TryDash()
    {
        if (isDashing || !canDash || isDead || isKnockedBack || isAttacking) return;
        StartCoroutine(DashRoutine());
    }
    
    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;
        trailRenderer.emitting = true;

        float dashDirection = facingRight ? 1f : -1f;
        float elapsedTime = 0f;

        int originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Dashing");

        // اجرای StartDash
        animator.SetTrigger("StartDash");

        yield return new WaitForSeconds(0.05f); // می‌تونی این رو با طول StartDash تنظیم کنی

        // اجرای Loop تا پایان dash
        animator.SetBool("DashLoop", true);

        float afterImageTimer = 0f;

        while (elapsedTime < dashDuration)
        {
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            elapsedTime += Time.deltaTime;
            afterImageTimer += Time.deltaTime;
            if (afterImageTimer >= afterImageInterval)
            {
                CreateAfterImage();
                afterImageTimer = 0f;
            }
            yield return null;
        }

        // پایان dash
        animator.SetBool("DashLoop", false);
        animator.SetTrigger("DashEnd");

        trailRenderer.emitting = false;
        gameObject.layer = originalLayer;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        // اگر روی زمین هست، سر خوردن کوچیک اجرا کن
        if (isGrounded)
        {
            float slideSpeed = 4f;         // سرعت سر خوردن
            float slideDuration = 0.15f;   // مدت سر خوردن

            float slideTime = 0f;
            while (slideTime < slideDuration)
            {
                rb.linearVelocity = new Vector2(dashDirection * slideSpeed, rb.linearVelocity.y);
                slideTime += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    
    private void CreateAfterImage()
    {
        GameObject clone = Instantiate(afterImagePrefab);
        Sprite currentSprite = GetComponent<SpriteRenderer>().sprite;
        Vector3 scale = transform.localScale; // اندازه واقعی بازیکن

        clone.GetComponent<AfterImage>().Setup(
            currentSprite,
            transform.position,
            scale
        );
    }








    
    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(firePoint.position, attackRange);
    }

}