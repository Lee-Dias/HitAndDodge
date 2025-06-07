using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private SpriteRenderer playerSpriteRenderer;
    [SerializeField]
    private float shotCooldown = 2f;

    private float shotTime;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 mousePos;
    private bool canMove = true;

    private int lastMoveDirection = 1;

    private PlayerShootingClientSidePrediction playerShootingClientSidePrediction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerShootingClientSidePrediction = GetComponent<PlayerShootingClientSidePrediction>();
        shotTime = shotCooldown;
    }

    void Update()
    {
        if (!IsLocalPlayer || !canMove) return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.x != 0)
        {
            lastMoveDirection = movement.x > 0 ? 1 : -1;
            transform.rotation = lastMoveDirection == 1 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
        }

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && shotCooldown <= shotTime)
        {
            shotTime = 0;
            canMove = false;

            // Flip player to face shooting direction
            Vector2 fireDirection = mousePos - (Vector2)transform.position;
            if ((fireDirection.x > 0 && transform.right.x < 0) || (fireDirection.x < 0 && transform.right.x > 0))
            {
                FlipPlayer();
            }

            animator.SetTrigger("Shoot"); 
            //faz com que os outros players vejam a animação
            TriggerShootAnimationServerRpc();
        }
        shotTime += Time.deltaTime;
    }
    [ServerRpc]
    private void TriggerShootAnimationServerRpc()
    {
        TriggerShootAnimationClientRpc();
    }

    [ClientRpc]
    private void TriggerShootAnimationClientRpc()
    {
        if (!IsLocalPlayer) // Evita chamar duas vezes no local
            animator.SetTrigger("Shoot");
    }

    void FixedUpdate()
    {
        if (!IsLocalPlayer || !canMove) return;
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void DoShoot() // Animation event calls this
    {
        if (!IsLocalPlayer) return;

        playerShootingClientSidePrediction.Shoot(mousePos);
    

        canMove = true;
    }

    private void FlipPlayer()
    {
        if (transform.right.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            lastMoveDirection = -1;
        }
        else
        {
            transform.rotation = Quaternion.identity;
            lastMoveDirection = 1;
        }
    }
    [ClientRpc]
    public void BlinkClientRpc()
    {
        StartCoroutine(BlinkCoroutine());
    }
    
    private IEnumerator BlinkCoroutine()
    {
        Color originalColor = playerSpriteRenderer.color;
        Color blinkColor = Color.red;

        float blinkDuration = 2f;
        float blinkInterval = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < blinkDuration)
        {
            playerSpriteRenderer.color = blinkColor;
            yield return new WaitForSeconds(blinkInterval / 2f);

            playerSpriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkInterval / 2f);

            elapsedTime += blinkInterval;
        }

        playerSpriteRenderer.color = originalColor;
    }
}
