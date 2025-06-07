using UnityEngine;
using Unity.Netcode;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;

    public void Shoot(Vector2 targetPos)
    {
        ShootServerRpc(targetPos);
    }

    [ServerRpc]
    private void ShootServerRpc(Vector2 targetPos)
    {
        SpawnBullet(targetPos);
    }

    public void SpawnBullet(Vector2 targetPos)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Vector2 direction = (targetPos - (Vector2)firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle + 180);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = direction * bulletSpeed;
        bullet.GetComponent<Bullet>().shooter = gameObject;
        bullet.GetComponent<NetworkObject>().Spawn();
    }
}
