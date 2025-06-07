using Unity.VisualScripting;
using UnityEngine;

public class EnterCheckpoint : MonoBehaviour
{
    public GameObject firstImage;
    public GameObject secondImage;
    public GameObject thirdImage;
    // private bool enter = false;
    // private bool exit = false;
    

    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (!enter)
    //     {
    //         firstImage.SetActive(true);
    //         secondImage.SetActive(false);
    //         thirdImage.SetActive(true);
    //         enter = true;
    //     }
    //     else
    //     {
    //         firstImage.SetActive(true);
    //         secondImage.SetActive(false);
    //         thirdImage.SetActive(true);
    //     }
    // }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            firstImage.SetActive(true);
            secondImage.SetActive(false);
            thirdImage.SetActive(true);
            gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
            // if (!enter)
            // {
            //     firstImage.SetActive(false);
            //     secondImage.SetActive(true);
            //     thirdImage.SetActive(false);
            // }
            // else
            // {
            //     firstImage.SetActive(true);
            //     secondImage.SetActive(false);
            //     thirdImage.SetActive(true);
            // }
            
        }
    }
    
}
