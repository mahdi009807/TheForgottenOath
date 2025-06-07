using Unity.VisualScripting;
using UnityEngine;

public class Breakables : MonoBehaviour
{

    public GameObject[] Collectibles;
    
    public void Break()
    {
        int random = Random.Range(0, Collectibles.Length);
        Vector3 pos = transform.position + new Vector3(0f, 1f, 0f);
        Instantiate(Collectibles[random], pos, Quaternion.identity);
        Destroy(gameObject);
    }
}