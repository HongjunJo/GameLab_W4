using UnityEngine;

public class CharacterDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Ability Unlocks")]
    public bool canDash = false;

    private Rigidbody2D rb;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float cooldownTimer = 0f;
    private Vector2 dashDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;
        if (!isDashing && canDash && Input.GetKeyDown(dashKey) && cooldownTimer <= 0f)
        {
            StartDash();
        }
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;
        dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (dashDirection == Vector2.zero)
            dashDirection = transform.right; // 기본적으로 바라보는 방향
        dashDirection.Normalize();
        rb.linearVelocity = dashDirection * (dashDistance / dashDuration);
    }

    private void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;
    }
}
