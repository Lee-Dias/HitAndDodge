using Unity.Netcode;
using UnityEngine;

public class PlayerShootingClientSidePrediction : NetworkBehaviour
{
    [SerializeField]
    private GameObject bulletPrefabLocal;   // sem NetworkObject
    [SerializeField]
    private GameObject bulletPrefabNetwork; // com NetworkObject + NetworkTransform
    [SerializeField]
    private Transform firePoint;
    [SerializeField]
    private float bulletSpeed = 10f;

    public void Shoot(Vector2 targetPos)
    {

        SpawnLocalPrediction(targetPos);  
    }
    private void SpawnLocalPrediction(Vector2 targetPos)
    {
        float spawnOffset = -1f;
        Vector2 direction = (targetPos - (Vector2)firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Vector3 spawnPos = firePoint.position + (Vector3)(direction * spawnOffset);
        GameObject bullet = Instantiate(bulletPrefabLocal, spawnPos, Quaternion.Euler(0f, 0f, angle + 180));
        bullet.GetComponent<BulletPrediction>().shooter = gameObject;
        bullet.GetComponent<BulletPrediction>().Initialize(direction, bulletSpeed);

        ShootServerRpc(spawnPos, bullet.transform.rotation);
        Destroy(bullet, 3f);
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 position, Quaternion rotation)
    {
        SpawnNetworkBullet(position, rotation);
    }

    private void SpawnNetworkBullet(Vector3 position, Quaternion rotation)
    {
        
        Vector2 direction = rotation * Vector2.left;

        GameObject bullet = Instantiate(bulletPrefabNetwork, firePoint.position, rotation);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Initialize(direction, bulletSpeed);
        bulletScript.shooter = gameObject;
        bullet.GetComponent<NetworkObject>().Spawn();
        
    }
}
