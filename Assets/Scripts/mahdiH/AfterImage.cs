using UnityEngine;

public class AfterImage : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color color;
    public float fadeSpeed = 2f;

    public void Setup(Sprite sprite, Vector3 position, Vector3 scale)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        transform.position = position;
        transform.localScale = scale; // دقیقاً همون scale بازیکن

        spriteRenderer.sprite = sprite;
        color = new Color(1f, 1f, 1f, 0.6f);
        spriteRenderer.color = color;
    }


    private void Update()
    {
        color.a -= fadeSpeed * Time.deltaTime;
        if (color.a <= 0f)
        {
            Destroy(gameObject);
        }
        else
        {
            spriteRenderer.color = color;
        }
    }
}

