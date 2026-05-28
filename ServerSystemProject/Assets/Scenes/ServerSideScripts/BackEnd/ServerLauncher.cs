using System;
using System.Collections;
using System.Net;
using UnityEngine;
using Mirror;
using kcp2k; // для KcpTransport ( for KcpTransport from Mirror, basically port access )

public class ServerLauncher : MonoBehaviour
{
    [Header("Ссылки")]
    public MyNetworkManager networkManager;
    public KcpTransport kcpTransport;

    // Публичные поля - для отправки порта и веншнего айпи сервера в Firebase
    // Public fields - for sending the server's port and external IP address to Firebase.
    [HideInInspector] public int serverPort = 0;
    [HideInInspector] public string externalIP = "";

    private void Start()
    {
        // Только в серверном билде
        // only for sever builds
#if UNITY_SERVER || UNITY_EDITOR
        int port = ParsePortArgument();

        if (port <= 0)
        {
            Debug.Log("[ServerLauncher] Порт не указан. Сервер не будет запущен.");

            // Debug.LogError("[ServerLauncher] Порт не указан! Используй: ./Server.exe --port 7778");
            // Application.Quit(1);
            // return;

            return;
        }

        serverPort = port;
        Debug.Log($"[ServerLauncher] Порт из аргументов: {serverPort}");

        // 1. Назначаем порт в KcpTransport
        kcpTransport.Port = (ushort)serverPort;
        Debug.Log($"[ServerLauncher] KcpTransport.Port установлен в {kcpTransport.Port}");

        // 2. Запускаем получение внешнего IP, потом стартуем сервер
        StartCoroutine(FetchExternalIPAndStartServer());
#endif
    }

    private int ParsePortArgument()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--port" && int.TryParse(args[i + 1], out int port))
            {
                return port;
            }
        }
        return -1;
    }

    private IEnumerator FetchExternalIPAndStartServer()
    {
        Debug.Log("[ServerLauncher] Получаем внешний IP...");

        // Запрашиваем внешний IP через публичный сервис
        UnityEngine.Networking.UnityWebRequest req =
            UnityEngine.Networking.UnityWebRequest.Get("https://api.ipify.org");

        yield return req.SendWebRequest();

        if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            externalIP = req.downloadHandler.text.Trim();
            Debug.Log($"[ServerLauncher] Внешний IP: {externalIP}");
        }
        else
        {
            Debug.LogWarning($"[ServerLauncher] Не удалось получить IP: {req.error}. Используем fallback.");
            externalIP = GetLocalIPFallback();
        }

        // 3. Записываем IP в NetworkManager (networkAddress используется клиентом, 
        //    но для сервера полезно хранить)
        networkManager.networkAddress = externalIP;

        Debug.Log($"[ServerLauncher] Запускаем Mirror-сервер на порту {serverPort}...");

        // 4. Запускаем сервер — аналог кнопки "Server Only" в NetworkManagerHUD
        networkManager.StartServer();

        Debug.Log($"[ServerLauncher] ✅ Сервер запущен! IP: {externalIP}, Port: {serverPort}");

        // 5. Данные готовы — их можно отправлять в Firebase (следующий пункт)
        OnServerDataReady(externalIP, serverPort);
    }

    private string GetLocalIPFallback()
    {
        // Запасной вариант — локальный IP
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return "127.0.0.1";
    }

    private void OnServerDataReady(string ip, int port)
    {
        Debug.Log($"[ServerLauncher] Данные готовы → IP: {ip}, Port: {port}");

        // Отправляем в Firebase
        if (FirebaseManager.Instance != null)
            FirebaseManager.Instance.UploadServerInfo(ip, port);
        else
            Debug.LogError("[ServerLauncher] FirebaseManager не найден на сцене!");
    }

    // При остановке сервера — помечаем оффлайн
    private void OnApplicationQuit()
    {
        if (FirebaseManager.Instance != null && externalIP != "" && serverPort > 0)
        {
            Debug.Log("[ServerLauncher] Выключение — удаляем сервер из Firebase...");
            // Синхронный вызов — успевает до закрытия
            FirebaseManager.Instance.DeleteServerSync(externalIP, serverPort);
        }
    }
}