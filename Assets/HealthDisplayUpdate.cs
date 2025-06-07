using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class HealthDisplayUpdate : NetworkBehaviour
{
    [SerializeField]
    private Image fill;
    [SerializeField]
    private Health health;

    private int maxHealth;

    private void Start()
    {
        if (IsOwner)
        {
            fill.color = Color.green;
        }
        maxHealth = health.MaxHealth;
    }

    [ClientRpc]
    public void UpdateHealthClientRpc(int newHealth)
    {
        float p = Mathf.Clamp01((float)newHealth / (float)maxHealth);
        fill.transform.localScale = new Vector3(p, 1f, 1f);
    }

    public void UpdateHealth(int newHealth)
    {
        float p = Mathf.Clamp01((float)newHealth / (float)maxHealth);
        fill.transform.localScale = new Vector3(p, 1f, 1f);
    }
}
