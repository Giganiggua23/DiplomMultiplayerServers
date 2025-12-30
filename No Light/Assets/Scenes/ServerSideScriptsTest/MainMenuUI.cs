using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject multiplayerPanel;

    public void OpenMultiplayer()
    {
        mainMenuPanel.SetActive(false);
        multiplayerPanel.SetActive(true);
    }

    public void BackToMenu()
    {
        multiplayerPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
