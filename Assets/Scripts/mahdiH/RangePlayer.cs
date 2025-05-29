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
    private bool facingRight = true;

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
    
    
    [Header("Ulty Settings")]
    public float maxMana = 100f;
    public float currentMana = 100f;
    public GameObject ultyArrowPrefab; // Prefab تیر خاص
    public float ultyManaCost = 100f;   // میزان مصرف مانا
    private bool isTryingUlty = false;



    private PlayerControler RangeControler;

    private void Awake()
    {
        RangeControler = new PlayerControler();

        // ورودی حرکت
        RangeControler.Range.Move.performed += ctx => input = ctx.ReadValue<float>();
        RangeControler.Range.Move.canceled += ctx => input = 0f;
        RangeControler.Range.Jump.performed += ctx => Jump();
        RangeControler.Range.Aim.started += ctx => StartCharging();
        RangeControler.Range.Aim.canceled += ctx => ReleaseArrow();
        RangeControler.Range.Ulty.started += ctx => isTryingUlty = true;
        RangeControler.Range.Ulty.canceled += ctx => isTryingUlty = false;


    }

    private void Update()
    {
        // تغییر جهت بازیکن
        if (facingRight && input < 0)
        {
            facingRight = false;
            transform.eulerAngles = new Vector3(0f, -180f, 0f);
        }
        else if (!facingRight && input > 0)
        {
            facingRight = true;
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
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
    }



    private void FixedUpdate()
    {
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
        if (currentJumpCount < maxJumpCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("Jump" , true);
            currentJumpCount++;
            Debug.Log(animator.GetBool("Jump"));
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

        if (isTryingUlty)
        {
            Vector2 direction = facingRight ? Vector2.right : Vector2.left;
            animator.SetTrigger("Ulty");
        }
        else
        {

            float launchForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, currentChargeTime / maxChargeTime);

            Vector2 spawnPos = firePoint.position + (facingRight ? Vector3.right : Vector3.left) * 0.5f;
            GameObject arrow = Instantiate(arrowPrefab, spawnPos, firePoint.rotation);

            Vector2 direction = facingRight ? Vector2.right : Vector2.left;
            arrow.GetComponent<RangePlayerArrow>().Launch(direction, launchForce);
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
