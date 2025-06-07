using UnityEngine;
using Unity.Netcode;

public class Health : NetworkBehaviour
{
    [SerializeField]
    private int maxHealth = 100;

    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    public int CHealth => currentHealth.Value;
    public int MaxHealth => maxHealth;

    private void Start()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        if (!IsServer) return;

        currentHealth.Value -= amount;

        if (currentHealth.Value <= 0)
        {
            Die();
        }
        HealthDisplayUpdate healthDU = GetComponent<HealthDisplayUpdate>();
        healthDU.UpdateHealthClientRpc(currentHealth.Value);
    }

    private void Die()
    {
        NetworkObject.Despawn();
    }
}
