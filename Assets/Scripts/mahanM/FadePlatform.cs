using System.Collections;
using UnityEngine;

public class FadePlatform : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Coroutine fadeCoroutine;
    public float fadeDuration = 0.5f; // Duration of fade effect in seconds
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Start with platform invisible (fully transparent)
        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Stop any existing fade
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            
            // Fade in over 0.5 seconds
            fadeCoroutine = StartCoroutine(FadeTo(1f));
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Stop any existing fade
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            
            // Fade out over 0.5 seconds
            fadeCoroutine = StartCoroutine(FadeTo(0f));
        }
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = spriteRenderer.color.a;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            // Calculate new alpha value
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            
            // Update sprite color
            Color c = spriteRenderer.color;
            c.a = newAlpha;
            spriteRenderer.color = c;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final alpha is exact
        Color finalColor = spriteRenderer.color;
        finalColor.a = targetAlpha;
        spriteRenderer.color = finalColor;
    }
}