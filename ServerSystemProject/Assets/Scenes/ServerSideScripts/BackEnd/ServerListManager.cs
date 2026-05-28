using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Структура одного сервера
[System.Serializable]
public class ServerEntry
{
    public string ip;
    public int port;
    public string status;
    public string lastSeen;
}

public class ServerListManager : MonoBehaviour
{
    public static ServerListManager Instance;

    [Header("Firebase")]
    [SerializeField]
    private string firebaseDatabaseURL =
        "https://unityserverlist-xxxxx-default-rtdb.europe-west1.firebasedatabase.app";

    [Header("Настройки обновления")]
    [SerializeField] private float refreshInterval = 10f; // секунд

    // Список серверов — сюда будет читать UI
    public List<ServerEntry> ServerList { get; private set; } = new();

    // Событие — вызывается когда список обновился
    public event System.Action OnServerListUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Сразу загружаем и запускаем автообновление
        StartCoroutine(AutoRefreshLoop());
    }

    // ─── Публичный метод для кнопки "Обновить" ───────────────────────
    public void RefreshServerList()
    {
        StartCoroutine(FetchServerList());
    }

    // ─── Автообновление каждые N секунд ──────────────────────────────
    private IEnumerator AutoRefreshLoop()
    {
        while (true)
        {
            yield return FetchServerList();
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    // ─── Загрузка данных из Firebase ─────────────────────────────────
    private IEnumerator FetchServerList()
    {
        string url = $"{firebaseDatabaseURL}/servers.json";

        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[ServerList] ❌ Ошибка получения: {req.error}");
            yield break;
        }

        string json = req.downloadHandler.text;
        Debug.Log($"[ServerList] Ответ Firebase: {json}");

        // Firebase возвращает null если база пустая
        if (json == "null" || string.IsNullOrEmpty(json))
        {
            ServerList.Clear();
            OnServerListUpdated?.Invoke();
            yield break;
        }

        ServerList = ParseServerList(json);
        Debug.Log($"[ServerList] ✅ Серверов найдено: {ServerList.Count}");

        OnServerListUpdated?.Invoke();
    }

    // ─── Парсинг JSON вручную (без Newtonsoft) ────────────────────────
    private List<ServerEntry> ParseServerList(string json)
    {
        var result = new List<ServerEntry>();

        // Firebase возвращает:
        // {"key1":{"ip":"...","port":7778,...},"key2":{"ip":"...","port":7243,...}}
        // Нам нужно вытащить каждый внутренний объект {...}

        // Пропускаем первый символ { внешнего объекта
        // и ищем все вложенные объекты начиная с глубины 1
        int depth = 0;
        int objStart = -1;

        for (int i = 0; i < json.Length; i++)
        {
            if (json[i] == '{')
            {
                depth++;
                // Вложенные объекты начинаются с depth == 2
                if (depth == 2)
                    objStart = i;
            }
            else if (json[i] == '}')
            {
                // Закрываем вложенный объект на depth == 2
                if (depth == 2 && objStart >= 0)
                {
                    string entryJson = json.Substring(objStart, i - objStart + 1);
                    Debug.Log($"[ServerList] Парсим запись: {entryJson}");

                    try
                    {
                        var entry = JsonUtility.FromJson<ServerEntry>(entryJson);
                        if (entry != null && !string.IsNullOrEmpty(entry.ip) && entry.port > 0)
                        {
                            result.Add(entry);
                            Debug.Log($"[ServerList] ✅ Добавлен сервер: {entry.ip}:{entry.port}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[ServerList] Ошибка парсинга записи: {e.Message}");
                    }

                    objStart = -1;
                }
                depth--;
            }
        }

        return result;
    }
}