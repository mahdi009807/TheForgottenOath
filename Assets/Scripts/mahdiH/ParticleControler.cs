using UnityEngine;

public class ParticleControler : MonoBehaviour
{
    [SerializeField] private ParticleSystem movementParticle;
    [SerializeField] private ParticleSystem FallingParticle;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private MeleePlayer player;

    private float counter;
    private bool IsOnGround;

    private void Update()
    {
        counter += Time.deltaTime;
        
        // اگر سرعت خیلی کم است یا ورودی نداریم => برگرد
        if (!IsOnGround || Mathf.Abs(player.moveInput) < 0.1f || Mathf.Abs(rb.linearVelocity.x) < 0.1f)
        {
            if (movementParticle.isPlaying)
                movementParticle.Stop();

            return;
        }

        // سرعت تولید بر اساس دویدن یا راه رفتن
        float dustFormationPeriod = player.IsRunning() ? 0.2f : 0.5f;

        if (counter > dustFormationPeriod)
        {
            movementParticle.Play();
            counter = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            FallingParticle.Play();
            IsOnGround = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            IsOnGround = false;
    }
}