using Unity.Netcode;
using UnityEngine;

public class PlayerShootingClientSidePrediction : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefabLocal;   // sem NetworkObject
    [SerializeField] private GameObject bulletPrefabNetwork; // com NetworkObject + NetworkTransform
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;

    // Chamada pelo cliente local
    public void Shoot(Vector2 targetPos)
    {
        if (!IsLocalPlayer) return;

        SpawnLocalPrediction(targetPos);   // Predição local imediata
        ShootServerRpc(targetPos);         // Pedido para o servidor fazer o spawn oficial
    }

    private void SpawnLocalPrediction(Vector2 targetPos)
    {
        GameObject bullet = Instantiate(bulletPrefabLocal, firePoint.position, Quaternion.identity);

        Vector2 direction = (targetPos - (Vector2)firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle + 180);
        bullet.GetComponent<BulletPrediction>().shooter = gameObject;
        bullet.GetComponent<Rigidbody2D>().linearVelocity = direction * bulletSpeed;

        Destroy(bullet, 3f); // auto-destruir localmente
    }

    [ServerRpc]
    private void ShootServerRpc(Vector2 targetPos)
    {
        SpawnNetworkBullet(targetPos);
    }
    private void SpawnNetworkBullet(Vector2 targetPos)
    {
        GameObject bullet = Instantiate(bulletPrefabNetwork, firePoint.position, Quaternion.identity);

        Vector2 direction = (targetPos - (Vector2)firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle + 180);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = direction * bulletSpeed;

        bullet.GetComponent<Bullet>().shooter = gameObject;
        bullet.GetComponent<NetworkObject>().Spawn();
    }
}
