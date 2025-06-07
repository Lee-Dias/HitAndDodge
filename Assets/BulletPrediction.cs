using UnityEngine;

public class BulletPrediction : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 3f;
    [HideInInspector]
    public GameObject shooter;

    private Rigidbody2D rb;



    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == shooter) return;

        Health health = collision.GetComponent<Health>();
        if (health != null)
            Destroy(gameObject);
        
    }
}
