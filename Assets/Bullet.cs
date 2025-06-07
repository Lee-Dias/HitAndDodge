using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    [SerializeField]
    private float lifeTime = 3f;
    [SerializeField]
    private int damage = 20;
    [HideInInspector]
    public GameObject shooter;

    private NetworkObject networkObject;



    private void Start()
    {
        Debug.Log($"Bullet spawned. IsServer: {IsServer}, IsOwner: {IsOwner}, IsClient: {IsClient}");
        // Destroy bullet after a few seconds
        Destroy(gameObject, lifeTime);
        networkObject = GetComponent<NetworkObject>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (!IsServer) return;

        if (collision.gameObject == shooter)
        {
            Debug.Log("Bullet ignored collision with shooter.");
            return;
        }


        PlayerController playerController = collision.GetComponent<PlayerController>();
        playerController.BlinkClientRpc();
        playerController.Blink();
        // Check if we hit a player (must have Health component)
        Health health = collision.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            NetworkObject.Despawn(); // Despawn bullet over the network
        }


        



    }
}
