using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class MyNetworkManager : NetworkManager
{
    private Transform[] spawnPoints;

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        spawnPoints = FindObjectsByType<CustomSpawnPoint>(
            FindObjectsSortMode.None)
            .Select(x => x.transform)
            .ToArray();

        Debug.Log($"[SPAWN] Найдено spawn points: {spawnPoints.Length}");
    }

    public override void OnClientConnect()
    {
        Debug.Log("CLIENT CONNECT");

        NetworkClient.Ready();

        if (!NetworkClient.localPlayer)
            NetworkClient.AddPlayer();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log($"SERVER ADD PLAYER: {conn.connectionId}");

        Transform start = GetStartPosition();

        Vector3 pos = start ? start.position : Vector3.zero;
        Quaternion rot = start ? start.rotation : Quaternion.identity;

        Debug.Log($"[SPAWN] Позиция спавна: {pos}");

        GameObject player = Instantiate(playerPrefab, pos, rot);

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override Transform GetStartPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        Debug.LogWarning("[SPAWN] Spawn points не найдены!");

        return null;
    }
}