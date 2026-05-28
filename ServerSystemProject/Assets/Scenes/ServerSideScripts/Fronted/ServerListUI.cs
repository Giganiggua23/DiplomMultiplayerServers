using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class ServerListUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private Transform serverListContainer; // ScrollView Content
    [SerializeField] private GameObject serverButtonPrefab;  // Префаб кнопки сервера
    [SerializeField] private Button refreshButton;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Ссылка на NetworkManager")]
    [SerializeField] private MyNetworkManager networkManager;
    [SerializeField] private kcp2k.KcpTransport kcpTransport;

    private List<GameObject> _spawnedButtons = new();

    private void Start()
    {
        // Подписываемся на обновление списка
        if (ServerListManager.Instance != null)
            ServerListManager.Instance.OnServerListUpdated += RebuildUI;

        refreshButton.onClick.AddListener(() =>
        {
            statusText.text = "Обновление...";
            ServerListManager.Instance?.RefreshServerList();
        });

        statusText.text = "Загрузка серверов...";
    }

    private void OnDestroy()
    {
        if (ServerListManager.Instance != null)
            ServerListManager.Instance.OnServerListUpdated -= RebuildUI;
    }

    // ─── Перестраиваем список кнопок ─────────────────────────────────
    private void RebuildUI()
    {
        foreach (var btn in _spawnedButtons)
            Destroy(btn);
        _spawnedButtons.Clear();

        var servers = ServerListManager.Instance.ServerList;

        // Добавь эти логи
        Debug.Log($"[UI] RebuildUI вызван, серверов: {servers.Count}");
        Debug.Log($"[UI] Container: {serverListContainer}");
        Debug.Log($"[UI] Prefab: {serverButtonPrefab}");

        if (servers.Count == 0)
        {
            Debug.Log("[UI] Список пустой");
            return;
        }

        foreach (var server in servers)
        {
            Debug.Log($"[UI] Спавним кнопку для {server.ip}:{server.port}");
            GameObject btnObj = Instantiate(serverButtonPrefab, serverListContainer);
            _spawnedButtons.Add(btnObj);

            var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = $"{server.ip}:{server.port}";
            else
                Debug.LogWarning("[UI] TextMeshProUGUI не найден на префабе!");

            var btn = btnObj.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogWarning("[UI] Button компонент не найден на префабе!");
                continue;
            }

            var capturedServer = server;
            btn.onClick.AddListener(() => ConnectToServer(capturedServer));
        }
    }

    // ─── Подключение к выбранному серверу ────────────────────────────
    private void ConnectToServer(ServerEntry server)
    {
        Debug.Log($"[ServerListUI] Подключаемся к {server.ip}:{server.port}");

        // Устанавливаем адрес и порт
        networkManager.networkAddress = server.ip;
        kcpTransport.Port = (ushort)server.port;

        // Запускаем клиент
        networkManager.StartClient();
    }
}