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
    private Coroutine autoHealCoroutine; // 코루틴 참조를 저장할 변수
    private DangerGaugeSystem dangerGaugeSystem; // DangerGaugeSystem 참조

    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0f;

    public delegate void OnHPChanged(float current, float max);
    public event OnHPChanged HPChanged;
    public delegate void OnDied();
    public event OnDied Died;

    private void Awake()
    {
        // 플레이어의 DangerGaugeSystem 컴포넌트를 찾아 저장합니다.
        dangerGaugeSystem = GetComponent<DangerGaugeSystem>();
    }

    private void Start()
    {
        // currentHP가 maxHP보다 높은 상태로 시작하는 것을 방지
        if (currentHP > maxHP || currentHP <= 0)
        {
            currentHP = maxHP;
        }
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
            // Died 이벤트를 호출하는 대신, DangerGaugeSystem의 사망 처리 메서드를 직접 호출합니다.
            if (dangerGaugeSystem != null)
            {
                dangerGaugeSystem.KillPlayer("HP 0"); // "HP 0"은 사망 원인 로그
            }

            // 기존 이벤트 구독자를 위해 이벤트도 호출해줍니다. (선택사항)
            Died?.Invoke(); 
        }

        // 자동 회복 로직 개선
        if (enableAutoHeal && currentHP > 0f)
        {
            // 기존 자동 회복 코루틴이 있다면 중지
            if (autoHealCoroutine != null)
            {
                StopCoroutine(autoHealCoroutine);
            }
            // 새로운 코루틴 시작
            autoHealCoroutine = StartCoroutine(AutoHealCoroutine());
        }
    }

    private System.Collections.IEnumerator AutoHealCoroutine()
    {
        // 데미지 후 일정 시간 대기
        yield return new WaitForSeconds(autoHealDelay);

        // HP가 최대가 될 때까지 주기적으로 회복
        while (currentHP < maxHP && !IsDead && Time.time >= lastDamageTime + autoHealDelay)
        {
            Heal(autoHealAmount);
            yield return new WaitForSeconds(autoHealInterval);
        }

        // 코루틴이 정상적으로 끝나면 참조를 null로 설정
        autoHealCoroutine = null;
    }

    /// <summary>
    /// HP를 최대치로 설정합니다.
    /// </summary>
    public void FullHeal()
    {
        currentHP = maxHP;
        HPChanged?.Invoke(currentHP, maxHP);

        // 자동 회복이 활성화되어 있고, 코루틴이 실행 중이 아닐 때 다시 시작하도록 보완
        if (enableAutoHeal)
        {
            if (autoHealCoroutine != null)
            {
                StopCoroutine(autoHealCoroutine);
            }
            // 데미지를 입은 상태가 아니므로, 즉시 회복 로직을 시작할 수 있습니다.
            autoHealCoroutine = StartCoroutine(AutoHealCoroutine());
        }
    }

    /// <summary>
    /// 무적 상태를 설정합니다.
    /// </summary>
    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }

    /// <summary>
    /// 외부에서 최대 체력을 설정하고 현재 체력을 초기화합니다.
    /// </summary>
    /// <param name="newMaxHP">새로운 최대 체력</param>
    public void Initialize(float newMaxHP)
    {
        maxHP = newMaxHP;
        currentHP = maxHP;
        HPChanged?.Invoke(currentHP, maxHP);
    }
}
