using UnityEngine;

public class MovingLava : MonoBehaviour 
{
    public Transform targetObject;
    public float speed = 2f;
    public float stopDistance = 0.1f;
    public DoorTrigger2D door;
    
    private Vector2 startPos;
    private bool moving = false;
    private bool reached = false;

    void Start()
    {
        startPos = transform.position;
        
        if(door == null) {
            door = FindObjectOfType<DoorTrigger2D>();
            door.onPlayerEnter.AddListener(StartMove);
        }
    }

    void Update()
    {
        if(!reached && moving && targetObject) {
            Vector2 dir = (targetObject.position - transform.position).normalized;
            transform.position += (Vector3)(dir * speed * Time.deltaTime);
            
            if(Vector2.Distance(transform.position, targetObject.position) <= stopDistance) {
                reached = true;
                moving = false;
            }
        }
    }

    public void StartMove() => moving = !reached ? true : false;

    public void ResetPos() {
        transform.position = startPos;
        moving = false;
        reached = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Knight") && moving) {
            ResetPos();
        }
    }
}