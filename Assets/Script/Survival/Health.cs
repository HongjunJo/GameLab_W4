using UnityEngine;

/// <summary>
/// 플레이어의 HP(체력)를 관리하는 컴포넌트. (산소와 별개로 동작)
/// </summary>
public class Health : MonoBehaviour
{
    [Header("HP Settings")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP = 100f;
    [SerializeField] private bool isInvincible = false;

    [Header("Auto Heal Settings")]
    [SerializeField] private float autoHealDelay = 3f; // 데미지 후 회복 대기 시간(초)
    [SerializeField] private float autoHealAmount = 5f; // 1틱당 회복량
    [SerializeField] private float autoHealInterval = 0.5f; // 회복 주기(초)
    [SerializeField] private bool enableAutoHeal = true;

    private float lastDamageTime = -999f;
    private bool isAutoHealing = false;

    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0f;

    public delegate void OnHPChanged(float current, float max);
    public event OnHPChanged HPChanged;
    public delegate void OnDied();
    public event OnDied Died;

    private void Start()
    {
        currentHP = maxHP;
        HPChanged?.Invoke(currentHP, maxHP);
    }

    /// <summary>
    /// HP를 회복합니다.
    /// </summary>
    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        HPChanged?.Invoke(currentHP, maxHP);
    }

    /// <summary>
    /// HP에 데미지를 입힙니다.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (IsDead || isInvincible) return;
        currentHP -= amount;
        HPChanged?.Invoke(currentHP, maxHP);
        lastDamageTime = Time.time;
        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Died?.Invoke();
        }
        if (enableAutoHeal && !isAutoHealing && currentHP > 0f && currentHP < maxHP)
        {
            StartCoroutine(AutoHealCoroutine());
        }
    }
    private System.Collections.IEnumerator AutoHealCoroutine()
    {
        isAutoHealing = true;
        // 데미지 후 일정 시간 대기
        while (Time.time < lastDamageTime + autoHealDelay)
            yield return null;

        // HP가 최대가 될 때까지 주기적으로 회복
        while (currentHP < maxHP && !IsDead)
        {
            Heal(autoHealAmount);
            if (currentHP >= maxHP) break;
            yield return new WaitForSeconds(autoHealInterval);
            // 도중에 추가 데미지 받으면 대기부터 다시 시작
            if (Time.time < lastDamageTime + autoHealDelay)
            {
                while (Time.time < lastDamageTime + autoHealDelay)
                    yield return null;
            }
        }
        isAutoHealing = false;
    }

    /// <summary>
    /// HP를 최대치로 설정합니다.
    /// </summary>
    public void FullHeal()
    {
        currentHP = maxHP;
        HPChanged?.Invoke(currentHP, maxHP);
    }

    /// <summary>
    /// 무적 상태를 설정합니다.
    /// </summary>
    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }
}
