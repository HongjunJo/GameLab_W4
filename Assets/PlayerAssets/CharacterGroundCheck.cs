using UnityEngine;

public class CharacterGroundCheck : MonoBehaviour
{

    private bool onGround;

    [Header("Ground Collider")]
    [SerializeField] private float groundLength = 1.0f;
    [SerializeField] private Vector3 colliderOffset;

    [Header("Layer Masks ")]
    [SerializeField] private LayerMask groundLayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        Vector3 leftRayOrigin = transform.position + new Vector3(-colliderOffset.x, colliderOffset.y, colliderOffset.z);
        Vector3 rightRayOrigin = transform.position + new Vector3(colliderOffset.x, colliderOffset.y, colliderOffset.z);

        onGround = Physics2D.Raycast(rightRayOrigin, Vector2.down, groundLength, groundLayer) || Physics2D.Raycast(leftRayOrigin, Vector2.down, groundLength, groundLayer);
    }

    public bool GetOnGround() { return onGround; }

    /// <summary>
    /// Scene 뷰에서 땅 감지 범위를 시각적으로 표시합니다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 position = transform.position;

        Vector3 leftRayOrigin = position + new Vector3(-colliderOffset.x, colliderOffset.y, colliderOffset.z);
        Vector3 rightRayOrigin = position + new Vector3(colliderOffset.x, colliderOffset.y, colliderOffset.z);

        // 두 개의 Raycast를 시각화합니다.
        Gizmos.DrawLine(rightRayOrigin, rightRayOrigin + Vector3.down * groundLength);
        Gizmos.DrawLine(leftRayOrigin, leftRayOrigin + Vector3.down * groundLength);
    }
}
