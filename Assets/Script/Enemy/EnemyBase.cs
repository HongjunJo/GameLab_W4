using UnityEngine;

/// <summary>
/// 적(Enemy) 기본 동작을 담당하는 베이스 클래스
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int attackPower = 10;
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
        // 기본 이동/AI 동작 구현 예정
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
                playerHealth.TakeDamage(attackPower);
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
            targetHealth.TakeDamage(attackPower);
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
