using UnityEngine;

public class ExitCheckpoint : MonoBehaviour
{
    public GameObject firstImage;
    public GameObject secondImage;
    public GameObject thirdImage;
    

    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         firstImage.SetActive(false);
    //         secondImage.SetActive(true);
    //         thirdImage.SetActive(false);
    //     }
    // }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            firstImage.SetActive(false);
            secondImage.SetActive(true);
            thirdImage.SetActive(false);
            gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
        }
    }
}
