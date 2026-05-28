using UnityEngine;
using Mirror;

public class DedicatedServerLauncher : MonoBehaviour
{
    void Start()
    {
#if UNITY_SERVER
        Debug.Log("Запуск сервера...");
        NetworkManager.singleton.StartServer();
#endif
    }
}
