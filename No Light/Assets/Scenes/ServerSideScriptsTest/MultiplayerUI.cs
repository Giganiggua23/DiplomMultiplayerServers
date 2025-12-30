using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MultiplayerUI : MonoBehaviour
{
    public Button connectButton;

    // Укажи тут IP сервера и порт, если нужно
    [SerializeField] private string serverIP = "95.79.84.78";

    void Start()
    {
        connectButton.onClick.AddListener(ConnectToServer);
    }

    void ConnectToServer()
    {
        Debug.Log($"Подключение к серверу: {serverIP}");

        NetworkManager.singleton.networkAddress = serverIP;
        NetworkManager.singleton.StartClient();

        // Можно скрыть кнопку после нажатия, если нужно
        connectButton.gameObject.SetActive(false);
    }
}
