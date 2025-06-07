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
        Debug.Log($"{gameObject.name} took {amount} damage. HP = {currentHealth.Value}");

        if (currentHealth.Value <= 0)
        {
            Die();
        }
        HealthDisplayUpdate healthDU = GetComponent<HealthDisplayUpdate>();
        healthDU.UpdateHealthClientRpc(currentHealth.Value);
        healthDU.UpdateHealth(currentHealth.Value);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        NetworkObject.Despawn();
        // You could trigger a respawn, disable controls, or despawn the player object
    }
}
