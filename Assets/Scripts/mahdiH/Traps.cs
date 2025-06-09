using UnityEngine;

public class Traps : MonoBehaviour
{
    public MeleePlayer meleePlayer;
    public RangePlayer rangePlayer;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MeleePlayer>(out var melee))
        {
            melee.TakeDamage(23, transform);
        }
        else if (collision.TryGetComponent<RangePlayer>(out var range))
        {
            range.TakeDamage(23, transform);
        }
    }

}
