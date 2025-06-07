using UnityEngine;

public class BulletPrediction : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 3f;
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
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == shooter) return;

        Health health = collision.GetComponent<Health>();
        if (health != null)
            Destroy(gameObject);
    }
}
