using System;
using System.Collections;
using UnityEngine;

public class RangePlayerArrow : MonoBehaviour
{
    public Rigidbody2D rb;
    public float lifeTime = 2f; // زمان قبل از نابودی تیر

    private void Start()
    {
        // خاموش کردن جاذبه تا تیر سقوط نکند
        rb.gravityScale = 0.3f;

        // نابودی تیر بعد از چند ثانیه
        Destroy(gameObject, lifeTime);
    }

    // پرتاب تیر بدون استفاده از AddForce
    public void Launch(Vector2 direction, float force)
    {
        rb.linearVelocity = direction.normalized * force;

        // تنظیم زاویه تیر بر اساس مسیر
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // برخورد تیر با دشمن یا محیط
        // if (collision.CompareTag("Enemy")) { ... }
        Destroy(gameObject);
    }
}