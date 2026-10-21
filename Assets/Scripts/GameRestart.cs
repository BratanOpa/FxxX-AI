using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestart : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;

    public void ShowWinPanel()
    {
        if (winPanel == null)
        {
            Debug.LogError("Koppla Win Panel till GameRestart i Inspector.", this);
            return;
        }

        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
