using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private Transform[] spawnPoints;

    private int nextSpawnIndex = 0;

    private void Awake()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        int maxPlayers = 2;

        if (NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
        {
            response.Approved = false;
            response.CreatePlayerObject = false;
            response.Pending = false;
            Debug.LogWarning("Conexão recusada: número máximo de jogadores atingido.");
        }
        else
        {
            response.Approved = true;
            response.CreatePlayerObject = false; 
            response.Pending = false;
            Debug.Log("Conexão aprovada.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (nextSpawnIndex > spawnPoints.Length)
            nextSpawnIndex = spawnPoints.Length;

        Transform spawnPoint = spawnPoints[nextSpawnIndex];
        nextSpawnIndex++;

        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
}
