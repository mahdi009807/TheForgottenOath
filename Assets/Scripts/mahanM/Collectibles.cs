using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntEvent : UnityEvent<int> {}

public class Collectibles : MonoBehaviour
{
    public IntEvent onCollect;

    [SerializeField] private int amount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MeleePlayer>(out _) || collision.TryGetComponent<RangePlayer>(out _))
        {
            onCollect?.Invoke(amount);
            Destroy(gameObject);
        }
    }
}