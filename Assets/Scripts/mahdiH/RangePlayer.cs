    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.Serialization;
    using UnityEngine.UI;
    using TMPro;
    using UnityEngine.SceneManagement;


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
        public Transform sprite;
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
        
        
        [Header("Health Settings")]
        public float maxHealth = 100;
        [SerializeField]private float currentHealth;
        private bool isDead = false;
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private Transform _healthBarTransform;
        [SerializeField] private Camera _camera;
        private int hearts = 3;
        [SerializeField] private TMP_Text heartsDisplay;
        [SerializeField] private Gradient colorGradient;
        
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

        
        [Header("Defend")]
        public bool isDefending = false;
        public float defendKnockbackMultiplier = 0.3f;
        
        [Header("Super Attack Settings")]
        public float mana = 0f;
        public float maxMana = 100f;
        public float superAttackCost = 100f;
        public float laserDuration = 1f;
        public float laserDamage = 50f;
        public float laserWidth = 0.2f;
        public Transform laserOrigin;
        public LineRenderer laserLine;
        public LayerMask laserHitMask;
        private bool isUsingSuper = false;
        public float laserDelay;
        [SerializeField] private Image _manaBarFill;


        private bool isTouchingWall;
        private bool isWallSliding;
        private int wallSlideSide; // -1 = left, 1 = right
        
        [Header("Respawn")]
        private Vector2 checkpointPos;




        private PlayerControler RangeControler;
        
        private void Start()
        {
            checkpointPos = transform.position;
            rb = GetComponent<Rigidbody2D>();
            currentHealth = maxHealth;
            // mana = maxMana;
            // _camera = Camera.main;
        }

        private void Awake()
        {
            PlayerRegistry.Archer = transform;
            
            RangeControler = new PlayerControler();
            
            currentHealth = maxHealth;
            
            RangeControler.Range.Move.performed += ctx =>
            {
                if (!isDefending) input = ctx.ReadValue<float>();
            };
            RangeControler.Range.Move.canceled += ctx => input = 0f;
            RangeControler.Range.Jump.performed += ctx => Jump();
            RangeControler.Range.Aim.started += ctx =>
            {
                if (!isDefending) StartCharging();
            };
            RangeControler.Range.Aim.canceled += ctx => ReleaseArrow();
            RangeControler.Range.Defend.started += ctx => StartDefending();
            RangeControler.Range.Defend.canceled += ctx => StopDefending();
            RangeControler.Range.SuperAttack.performed += ctx => TrySuperAttack();

            

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
            

            if (!isDefending) animator.SetBool("isDefending", false);
            else animator.SetBool("isDefending" , true);


        }
        
        private void LateUpdate()
        {
            _healthBarTransform.rotation = _camera.transform.rotation;
        }



        private void FixedUpdate()
        {
            if (isDead || isKnockedBack) return;
            
            if (isWallSliding)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
            }


            if (!isDefending)
            {
                float speedMultiplier = isCharging ? 0.2f : 1f;
                transform.position += new Vector3(input, 0f, 0f) * moveSpeed * speedMultiplier * Time.fixedDeltaTime;
            }
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
        }

        private void ReleaseArrow()
        {
            if (!isCharging) return;

            isCharging = false;
            animator.SetBool("IsAiming", false);

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

            float direction = Mathf.Sign(transform.position.x - attacker.position.x);
            float forward = facingRight ? -1f : 1f;

            bool hitFromFront = Mathf.Sign(direction) == Mathf.Sign(forward);

            if (isDefending && hitFromFront)
            {
                StartCoroutine(ApplyKnockback(direction, true));
                return;
            }

            currentHealth -= damage;
            Debug.Log(currentHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateHealthBar();
            animator.SetTrigger("Hit");
            StartCoroutine(ApplyKnockback(direction, false));

            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void UpdateHealthBar()
        {
            _healthBarFill.fillAmount = currentHealth / maxHealth;
            _healthBarFill.color = colorGradient.Evaluate(currentHealth / maxHealth);
        }
        
        private void UpdateManaBar()
        {
            _manaBarFill.fillAmount = mana / maxMana;
        }

        private void StartDefending()
        {
            isDefending = true;
            animator.speed = 1;
            animator.SetBool("isDefending", true);
        }

        private void StopDefending()
        {
            isDefending = false;
            animator.SetBool("isDefending", false);
        }



        
        private IEnumerator ApplyKnockback(float direction, bool isDefend)
        {
            isKnockedBack = true;

            float timer = 0f;
            float xForce = isDefend ? knockbackForceX * defendKnockbackMultiplier : knockbackForceX;
            float yForce = isDefend ? knockbackForceY * defendKnockbackMultiplier : knockbackForceY;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            transform.position += new Vector3(0f, yForce * Time.fixedDeltaTime, 0f);

            while (timer < knockbackDuration)
            {
                transform.position += new Vector3(direction * xForce * Time.deltaTime, 0f, 0f);
                timer += Time.deltaTime;
                yield return null;
            }

            isKnockedBack = false;
        }
        
        private void TrySuperAttack()
        {
            if (mana >= superAttackCost && !isUsingSuper && !isDead && !isKnockedBack)
            {
                StartCoroutine(ExecuteSuperAttack());
            }
        }

        private IEnumerator ExecuteSuperAttack()
        {
            isUsingSuper = true;
            input = 0f;
            isCharging = false;
            animator.SetTrigger("SuperAttack");

            yield return new WaitForSeconds(laserDelay);

            Vector2 direction = facingRight ? Vector2.right : Vector2.left;
            Vector2 origin = laserOrigin.position;

            // فقط اولین برخورد با زمین یا دیوار
            RaycastHit2D wallHit = Physics2D.Raycast(origin, direction, Mathf.Infinity, laserHitMask);
            Vector2 endPos = wallHit.collider != null ? wallHit.point : origin + direction * 100f;

            // نمایش لیزر
            laserLine.enabled = true;
            laserLine.startWidth = laserWidth;
            laserLine.endWidth = laserWidth;
            laserLine.SetPosition(0, origin);
            laserLine.SetPosition(1, endPos);

            // آسیب زدن به همه‌ی دشمن‌ها بین origin و endPos
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, Vector2.Distance(origin, endPos));
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.TryGetComponent<MeleeEnemy>(out MeleeEnemy enemy))
                {
                    enemy.TakeDamage(laserDamage, transform);
                }
            }

            mana -= superAttackCost;
            yield return new WaitForSeconds(laserDuration);
            laserLine.enabled = false;
            isUsingSuper = false;
        }

        
        public void AddMana(float amount)
        {
            mana = Mathf.Clamp(mana + amount, 0f, maxMana);
        }






        
        private void Die()
        {
            if (isDead) return;
    
            isDead = true;
            input = 0f;
            animator.SetBool("Dead", true);
            RangeControler.Disable();
    
            // Disable all player functionality without destroying
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
            GetComponent<Collider2D>().enabled = false;

            hearts--;
            heartsDisplay.text = hearts.ToString();
            // Destroy(gameObject);

            if (hearts < 0)
            {
                SceneManager.LoadScene("Game Over");
            }
            else
            {
                currentHealth = maxHealth;
                mana = 0;
                StartCoroutine(Respawn(1f));
            }
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Checkpoint"))
            {
                checkpointPos = transform.position;
            }

            if (collision.gameObject.CompareTag("Heart"))
            {
                if (hearts == 5)
                {
                    return;
                }
                hearts++;
            }

            if (collision.gameObject.CompareTag("Health"))
            {
                currentHealth += 17;
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                UpdateHealthBar();
            }

            if (collision.gameObject.CompareTag("Mana"))
            {
                AddMana(31);
                UpdateManaBar();
            }
            
            if (collision.gameObject.CompareTag("Power"))
            {
                float temp1 = minLaunchForce;
                float temp2 = maxLaunchForce;
                minLaunchForce *= 2;
                maxLaunchForce *= 2;
                StartCoroutine(Wait(5));
                minLaunchForce = temp1;
                maxLaunchForce = temp2;
            }

            if (collision.gameObject.CompareTag("Traps"))
            {
                TakeDamage(23, transform);
            }

            if (collision.gameObject.CompareTag("DeadlyTraps"))
            {
                Die();
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Projectile"))
            {
                TakeDamage(7, transform);
            }
        }

        private IEnumerator Wait(int seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
        
        private void OnGUI()
        {
            heartsDisplay.text = hearts.ToString();
        }
        
        IEnumerator Respawn(float duration)
        {
            // Make player completely invisible during respawn
            GetComponent<SpriteRenderer>().enabled = false;
    
            yield return new WaitForSeconds(duration);
    
            // Reset position first
            transform.position = checkpointPos;
    
            // Re-enable everything
            isDead = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
            GetComponent<Collider2D>().enabled = true;
            GetComponent<SpriteRenderer>().enabled = true;
    
            // Reset animator
            animator.SetBool("Dead", false);
            animator.Rebind();
            animator.Update(0f);
    
            // Re-enable controls
            RangeControler.Enable();
    
            // Reset health
            currentHealth = maxHealth;
            UpdateHealthBar();
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



