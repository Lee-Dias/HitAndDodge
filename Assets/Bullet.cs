using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    [SerializeField]
    private float lifeTime = 3f;
    [SerializeField]
    private int damage = 20;
    [SerializeField]
    private float speed = 10f;
    [HideInInspector]
    public GameObject shooter;

    private Vector2 direction;

    public void Initialize(Vector2 dir, float bulletSpeed)
    {
        direction = dir.normalized;
        speed = bulletSpeed;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (IsServer)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || collision.gameObject == shooter) return;

        Health health = collision.GetComponent<Health>();
        if (health != null)
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            playerController.BlinkClientRpc();
            health.TakeDamage(damage);
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
