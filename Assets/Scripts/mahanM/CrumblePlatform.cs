using System.Collections;
using NUnit.Framework.Constraints;
using UnityEngine;

public class CrumblePlatform : MonoBehaviour
{
    public float shakeDuration = 0.5f;
    public float fallDelay = 0.25f;
    public float fallGravityScale = 2f;
    public float destroyDelay = 1f;
    public bool flashEffect = false;
    

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool hasActivated = false;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasActivated && collision.gameObject.CompareTag("Player"))
        {
            hasActivated = true;
            StartCoroutine(CrumbleSequence());
        }
    }

    private IEnumerator CrumbleSequence()
    {
        if (flashEffect)
            StartCoroutine(Flash());

        yield return Shake(shakeDuration);

        yield return new WaitForSeconds(fallDelay);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallGravityScale;
        boxCollider.isTrigger = true;
        
        Destroy(gameObject, destroyDelay);
    }

    private IEnumerator Shake(float duration)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.05f, 0.05f);
            float y = Random.Range(-0.05f, 0.05f);
            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    private IEnumerator Flash()
    {
        while (true)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.3f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
