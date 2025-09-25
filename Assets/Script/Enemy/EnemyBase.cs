using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 적(Enemy) 기본 동작을 담당하는 베이스 클래스
/// </summary>
// ...existing code...
// ...existing code...
// ...existing code...
 [RequireComponent(typeof(Health))]
 public class EnemyBase : MonoBehaviour
 {
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
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, step);

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
    float step = moveSpeed * Time.deltaTime;
    transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
    }

    private void OnDrawGizmosSelected()
    {
        // 플레이어 탐지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

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
