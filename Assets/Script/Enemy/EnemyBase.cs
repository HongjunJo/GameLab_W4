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
            // maxHP 값 동기화
            var hpField = typeof(Health).GetField("maxHP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hpField != null)
            {
                hpField.SetValue(health, maxHP);
                health.FullHeal();
            }
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
        // "Player" 태그 또는 Health 컴포넌트로 판별
        if (obj.CompareTag("Player") || obj.GetComponent<Health>() != null)
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
