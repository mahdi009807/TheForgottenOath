using UnityEngine;

public class DeadlyTrap : MonoBehaviour
{
    public MeleePlayer meleePlayer;
    public RangePlayer rangePlayer;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MeleePlayer>(out var melee))
        {
            melee.TakeDamage(100, transform);
        }
        else if (collision.TryGetComponent<RangePlayer>(out var range))
        {
            range.TakeDamage(100, transform);
        }
    }
}
