    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.Serialization;

    public class MeleePlayer : MonoBehaviour
    {
        public Camera cam;                       //for camera to follow.

        public Transform AttackPoint;                     //for attack the enemies.
        public Transform DashAttackPoint;
        public float AttackRangr;
        public LayerMask EnemyLayers;
        public float attackingDuration;                    //for not attacking immediately.
        private bool isAttacking;

        public Rigidbody2D rb;                       //for jumping.
        public float JumpHeight;
        public int maxJumpCount;
        private int currentJumpCount; 
        
        public Animator animator;

        public float moveSpeed;                       //for moving.
        float input;

        private bool facingRight;        //for moving left and right

        private bool canDash;                      //for player dash.
        private bool isDashing;
        [SerializeField]private float dashingPower;
        [SerializeField]private float dashingDuration;
        private float dashingCooldown;
        public TrailRenderer tr;

        private bool canSlide;                           //for player sliding.
        private bool isSliding;                      
        public PolygonCollider2D slideCollider;
        public PolygonCollider2D standCollider;
        public PolygonCollider2D AttackCollider;
        public float slideDuration;

        private PlayerControler controls;
        
        private float currentHealth;
        private float maxHealth;
        
        
        private void Awake()
        {
            maxHealth = 100f;
            currentHealth = maxHealth;

            slideDuration = 0.4f;
            
            maxJumpCount = 1;
            currentJumpCount = 0;
            
            attackingDuration = 0.8f;
            AttackRangr = 0.1f;
            
            moveSpeed = 7f;
            facingRight = true;
            
            canDash = true;
            isDashing = false;
            dashingPower = 25f;
            dashingDuration = 0.4f;
            dashingCooldown = 1f;
            
            canSlide = true;
            isSliding = false;
            slideCollider.enabled = false;
            standCollider.enabled = true;
            slideDuration = 0f;
            
            controls = new PlayerControler();
            controls.Melee.Move.performed += ctx => input = ctx.ReadValue<float>();
            controls.Melee.Move.canceled += ctx => input = 0f;
            controls.Melee.Jump.performed += ctx => Jump();
            controls.Melee.Attack.performed += ctx =>
            {
                if (Keyboard.current.leftShiftKey.isPressed && isDashing) StartCoroutine(Dash_Attack());
                else Attack();
                
            };
            controls.Melee.Dash.performed += ctx =>
            {
                if (canDash) StartCoroutine(Dash());
            };
            controls.Melee.AirDash.performed += ctx =>
            {
                if (canDash && animator.GetBool("Jump"))
                {
                    animator.SetBool("Jump", false);
                    StartCoroutine(Dash());
                }
            };
            controls.Melee.Slide.performed += ctx =>
            {
                if (Mathf.Abs(input) > 0.1f && canSlide && !animator.GetBool("Jump") && !animator.GetBool("Attack"))
                {
                    StartCoroutine(slide());
                }
            };
        }

        
        
        
        
        void Update()
        {
            if (isDashing || isAttacking) return;

            if (input < 0 && facingRight)                    //for changing player pic. 180 degree.
            {
                facingRight = false;
                transform.eulerAngles = new Vector3(0, -180, 0);
            } 
            else if (input > 0 && !facingRight)
            {
                facingRight = true;
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
            animator.SetFloat("Run", Mathf.Abs(input) > 0.1f ? 1 : 0);   //for running animation.
        }
        
        
        
        
        
        private void FixedUpdate()
        {
            if (isDashing || isAttacking) return;
            transform.position += new Vector3(input , 0f , 0f) * moveSpeed * Time.fixedDeltaTime;       //for movement.
        }

        
        
        
        
        private void Attack()                                      //for attack.
        {
            if (animator.GetBool("Jump"))
            {
                animator.SetBool("Jump" , false);
            }
            isAttacking = true;
            animator.SetTrigger("Attack");
            StartCoroutine(AttackingDuration());
        }

        
        
        
        
        private IEnumerator AttackingDuration()
        {
            yield return new WaitForSeconds(attackingDuration);
            isAttacking = false;
        }

        
        
        
        
        private void PerformAttack()
        {
            Collider2D[] hitEnemy = Physics2D.OverlapCircleAll(AttackPoint.position , AttackRangr , EnemyLayers);
            foreach (Collider2D Enemy in hitEnemy)
            {
                Enemy.GetComponent<Enemy1>().TakeDamage(20);
            }
        }

        
        
        
        
        private void OnDrawGizmosSelected()
        {
            if(AttackPoint == null || DashAttackPoint == null) return;
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(AttackPoint.position , AttackRangr);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(DashAttackPoint.position , AttackRangr);
        }

        
        
        
        
        private IEnumerator Dash_Attack()                        //for dash attack.
        {
            if (isAttacking) yield break;
            
            canDash = false;
            isDashing = true;
            
            float originalGravity = rb.gravityScale;              //save the gravity and change it to 0. 
            rb.gravityScale = 0f;
            
            int facingRightInt = facingRight ? 1 : -1;                           //for dashing move.
            rb.linearVelocity = new Vector2(facingRightInt * dashingPower , 0f);
            
            animator.SetTrigger("Dash_Attack");                            //dashing-attack animation.
            
            tr.emitting = true;
            yield return new WaitForSeconds(dashingDuration);                          //how long animation takes.
            tr.emitting = false;
            
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);                        //after dashing make the x.position 0 so player will stop.
            rb.gravityScale = originalGravity;
            
            isDashing = false;
            yield return new WaitForSeconds(dashingCooldown);                         //respawn time for dashing-attack again.
            canDash = true;
        }
        
        
        
        
        private void PerformDashAttack()
        {
            Collider2D[] hitEnemy = Physics2D.OverlapCircleAll(AttackPoint.position , AttackRangr , EnemyLayers);
            foreach (Collider2D Enemy in hitEnemy)
            {
                Enemy.GetComponent<Enemy1>().TakeDamage(50);
            }
        }
        
        
        
        
        
        private void OnEnable()
        {
            controls.Enable();
        }

        
        
        
        
        private void OnDisable()
        {
            controls.Disable();
        }

        
        
        
        
        void Jump()                                      //for simple jump and double-jump.
        {
            if (isAttacking) return;
            
            if (currentJumpCount < maxJumpCount)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpHeight);
                animator.SetBool("Jump" , true);
                currentJumpCount++;
            }
        }

        
        
        
        
        private void OnCollisionEnter2D(Collision2D collision)            //for not jumping more than 2 times in air.  also for enemy contacts.
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                currentJumpCount = 0;
                animator.SetBool("Jump" , false);
                animator.SetFloat("Run", Mathf.Abs(input) > 0.1f ? 1 : 0);
            }
        }

        
        
        
        
        private IEnumerator Dash()                        //for dash.
        {
            if (isAttacking) yield break;
            
            canDash = false;
            isDashing = true;
            
            float originalGravity = rb.gravityScale;
            rb.gravityScale = 0f;
            
            int facingRightInt = facingRight ? 1 : -1;
            rb.linearVelocity = new Vector2(facingRightInt * dashingPower , 0f);
            
            animator.SetTrigger("Dash");
            
            tr.emitting = true;
            yield return new WaitForSeconds(dashingDuration);
            tr.emitting = false;
            
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); 
            rb.gravityScale = originalGravity;
            
            isDashing = false;
            yield return new WaitForSeconds(dashingCooldown);
            canDash = true;
        }

        
        
        
        
        private IEnumerator slide()                  //for sliding.
        {
            canSlide = false;
            isSliding = true;

            standCollider.enabled = false;
            slideCollider.enabled = true;
            
            rb.position -= new Vector2(0f, 0.2f); 

            float originVelocity = moveSpeed;
            moveSpeed *= 1.2f;

            animator.SetTrigger("Slide");
            yield return new WaitForSeconds(slideDuration);

            moveSpeed = originVelocity;
            
            rb.position += new Vector2(0f, 0.2f); 

            slideCollider.enabled = false;
            standCollider.enabled = true;
            
            canSlide = true;
            isSliding = false;
        }
        
        
        
        
        public void TakeDamage(float damage)
        {
            //animator.SetBool()...
            currentHealth -= damage;
            Debug.Log(currentHealth);
            if (currentHealth <= 0)
            {
                //animator.....
                Debug.Log("you died");
                OnDisable();
            }
        }
    }
