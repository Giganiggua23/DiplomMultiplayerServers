using System.Collections;
using System.Net.Http;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    [Header("Firebase настройки")]
    [SerializeField]
    private string firebaseDatabaseURL =
        "https://unityserverlist-xxxxx-default-rtdb.europe-west1.firebasedatabase.app";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // ─── Запись сервера ───────────────────────────────────────────────
    public void UploadServerInfo(string ip, int port)
    {
        StartCoroutine(UploadCoroutine(ip, port));
    }

    private IEnumerator UploadCoroutine(string ip, int port)
    {
        string serverID = GetServerID(ip, port);
        string json = BuildServerJson(ip, port, "online");
        string url = $"{firebaseDatabaseURL}/servers/{serverID}.json";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        using UnityWebRequest req = new UnityWebRequest(url, "PUT");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            Debug.Log($"[Firebase] ✅ Сервер записан: {serverID}");
        else
            Debug.LogError($"[Firebase] ❌ Ошибка записи: {req.error}");
    }

    // ─── Удаление сервера (СИНХРОННО, вызывается при выключении) ─────
    public void DeleteServerSync(string ip, int port)
    {
        string serverID = GetServerID(ip, port);
        string url = $"{firebaseDatabaseURL}/servers/{serverID}.json";

        try
        {
            // Используем HttpClient синхронно — успевает до закрытия приложения
            using var client = new HttpClient();
            var result = client.DeleteAsync(url).GetAwaiter().GetResult();

            if (result.IsSuccessStatusCode)
                Debug.Log($"[Firebase] ✅ Сервер удалён из базы: {serverID}");
            else
                Debug.LogWarning($"[Firebase] ⚠️ Не удалось удалить: {result.StatusCode}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Firebase] ❌ Ошибка удаления: {e.Message}");
        }
    }

    // ─── Вспомогательные методы ───────────────────────────────────────
    private string GetServerID(string ip, int port)
        => $"{ip}_{port}".Replace(".", "-");

    private string BuildServerJson(string ip, int port, string status)
        => $"{{" +
           $"\"ip\":\"{ip}\"," +
           $"\"port\":{port}," +
           $"\"status\":\"{status}\"," +
           $"\"lastSeen\":\"{System.DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\"" +
           $"}}";
}