using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 적(Enemy) 기본 동작을 담당하는 베이스 클래스
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyBase : MonoBehaviour
{
    // unreachablePlayerTimeLimit은 chaseMemoryTime과 항상 동일하게 사용
    private float unreachablePlayerTimer = 0f;
    [Header("Cliff Check")]
    [SerializeField] private bool preventCliffFall = true;
    [SerializeField] private float groundCheckDistance = 1.0f;
    [SerializeField] private LayerMask groundLayer;

    // 이동 전 발밑에 땅이 있는지 체크
    private bool IsGroundAhead(Vector3 direction)
    {
        if (!preventCliffFall) return true;
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        if (col == null)
        {
            return true;
        }
        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y) + (Vector2)direction.normalized * 0.3f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
#if UNITY_EDITOR
        Debug.DrawLine(origin, origin + Vector2.down * groundCheckDistance, hit ? Color.green : Color.red, 0.1f);
#endif
        return hit;
    }
    [Header("Patrol Settings")]
    [SerializeField] private bool enablePatrol = false;
    [SerializeField] private List<Vector3> patrolPoints = new List<Vector3>();
    [SerializeField] private float patrolWaitTime = 0.5f;
    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;
    [Header("Chase Memory Time (sec)")]
    [SerializeField] private float chaseMemoryTime = 2f;
    private float chaseTimer = 0f;
    [Header("Chase Options")]
    [SerializeField] private bool followY = false; // Y축도 따라갈지 여부
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask playerLayer = 1 << 0; // Default 레이어(기본값 0)

    private Transform playerTarget;
    private bool isPlayerDetected = false;
    [Header("Enemy Settings")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int attackPower = 10;
    public int AttackPower => attackPower;
    [Header("HP Settings")]
    [SerializeField] protected float maxHP = 30f;
    [Header("추격 속도 설정")]
    [SerializeField] private float chaseSpeed = 3f;

    protected Health health;

    protected virtual void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.Initialize(maxHP); // 리플렉션 대신 공개 메서드 사용
            health.Died += OnDied;
        }
    }

    protected virtual void Update()
    {
        DetectPlayer();
        if (isPlayerDetected && playerTarget != null)
        {
            chaseTimer = chaseMemoryTime;
            FollowPlayer();
        }
        else if (chaseTimer > 0f && playerTarget != null)
        {
            chaseTimer -= Time.deltaTime;
            FollowPlayer();
        }
        else
        {
            playerTarget = null;
            if (enablePatrol && patrolPoints.Count > 1)
            {
                Patrol();
            }
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Count == 0) return;
        Vector3 targetPoint = patrolPoints[currentPatrolIndex];
        float step = moveSpeed * Time.deltaTime;
        Vector3 moveDir = (targetPoint - transform.position).normalized;
        if (IsGroundAhead(moveDir))
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, step);
        }

        if (Vector3.Distance(transform.position, targetPoint) < 0.05f)
        {
            patrolWaitTimer += Time.deltaTime;
            if (patrolWaitTimer >= patrolWaitTime)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
                patrolWaitTimer = 0f;
            }
        }
        else
        {
            patrolWaitTimer = 0f;
        }
    }
    private void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if (hit != null && hit.CompareTag("Player"))
        {
            playerTarget = hit.transform;
            isPlayerDetected = true;
        }
        else
        {
            isPlayerDetected = false;
        }
    }

    private void FollowPlayer()
    {
        // X축만 또는 X,Y축 모두 따라가기
        float targetY = followY ? playerTarget.position.y : transform.position.y;
        Vector3 targetPos = new Vector3(playerTarget.position.x, targetY, transform.position.z);
        float step = chaseSpeed * Time.deltaTime;
        Vector3 moveDir = (targetPos - transform.position).normalized;
        if (IsGroundAhead(moveDir))
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            unreachablePlayerTimer = 0f;
        }
        else
        {
            unreachablePlayerTimer += Time.deltaTime;
            if (unreachablePlayerTimer >= chaseMemoryTime)
            {
                // 추적 포기, 패트롤로 복귀
                isPlayerDetected = false;
                unreachablePlayerTimer = 0f;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 플레이어 탐지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 절벽 방지용 레이캐스트 시각화 (실제 레이캐스트와 동일하게 콜라이더 기준)
        if (preventCliffFall)
        {
            CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
            if (col != null)
            {
                Vector2 baseOrigin = new Vector2(col.bounds.center.x, col.bounds.min.y);
                Vector3[] dirs = { Vector3.right, Vector3.left };
                foreach (var dir in dirs)
                {
                    Vector2 origin = baseOrigin + (Vector2)dir.normalized * 0.3f;
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(origin, origin + Vector2.down * groundCheckDistance);
                }
            }
        }

        // 패트롤 포인트 및 경로
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                Gizmos.DrawSphere(patrolPoints[i], 0.2f);
                int next = (i + 1) % patrolPoints.Count;
                Gizmos.DrawLine(patrolPoints[i], patrolPoints[next]);
            }
        }
    }

    /// <summary>
    /// 플레이어와 충돌 시 데미지(2D 기준)
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other.gameObject);
    }

    private void TryDamagePlayer(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            var playerHealth = obj.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackPower, this.gameObject);
            }
        }
    }

    /// <summary>
    /// 공격(플레이어 등 타겟에 데미지)
    /// </summary>
    public virtual void Attack(GameObject target)
    {
        var targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackPower, this.gameObject);
        }
    }

    /// <summary>
    /// 피격 처리
    /// </summary>
    public virtual void TakeDamage(float amount)
    {
        health.TakeDamage(amount);
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    protected virtual void OnDied()
    {
        // 사망 애니메이션/이펙트 등 추가 가능
        Destroy(gameObject);
    }
}
