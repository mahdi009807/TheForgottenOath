using UnityEngine;
using UnityEngine.SceneManagement;
public class FinishPoint : MonoBehaviour
{
    // public GameObject Manager;
    private bool knightEnter = false;
    private bool archerEnter = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Knight")
        {
            // knightEnter = true;
            // if (archerEnter)
            // {
            // Manager.SetActive(false);
            Destroy(GameObject.FindWithTag("ChunkManager"));
                UnlockedNewLevel();
                SceneController.instance.NextLevel();
            // }
        }

        if (collision.gameObject.tag == "Archer")
        {
            // archerEnter = true;
            // if (knightEnter)
            // {
                UnlockedNewLevel();
                SceneController.instance.NextLevel();
                // Manager.SetActive(false);
            // }
        }
    }

    void UnlockedNewLevel()
    {
        if (SceneManager.GetActiveScene().buildIndex >= PlayerPrefs.GetInt("ReachedIndex"))
        {
            PlayerPrefs.SetInt("ReachedIndex", SceneManager.GetActiveScene().buildIndex + 1);
            PlayerPrefs.SetInt("UnlockedLevel", PlayerPrefs.GetInt("UnlockedLevel", 1) + 1);
            PlayerPrefs.Save();
            
        }
    }
}
