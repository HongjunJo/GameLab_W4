using UnityEngine;

/// <summary>
/// 플레이어의 HP(체력)를 관리하는 컴포넌트. (산소와 별개로 동작)
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Damage Effect Settings")]
    [SerializeField] private float invincibleTime = 1f;
    [SerializeField] private Color blinkColor = Color.red;
    [SerializeField] private int blinkCount = 6;
    [SerializeField] private float knockbackForce = 10f;
    [Header("Invincible Sprite Renderers")]
    [Tooltip("Change color for these SpriteRenderers during invincibility. If empty, uses first found child.")]
    [SerializeField] private SpriteRenderer[] invincibleSpriteRenderers;
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;
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
    if (invincibleSpriteRenderers == null || invincibleSpriteRenderers.Length == 0)
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // 플레이어의 DangerGaugeSystem 컴포넌트를 찾아 저장합니다.
        dangerGaugeSystem = GetComponent<DangerGaugeSystem>();
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
        TakeDamage(amount, null);
    }

    public void TakeDamage(float amount, GameObject attacker)
    {
        if (IsDead || isInvincible) return;
        currentHP -= amount;
        HPChanged?.Invoke(currentHP, maxHP);
        lastDamageTime = Time.time;

        // 넉백 적용 (공격자 기준, 2D 전체 방향)
        if (knockbackForce > 0f && attacker != null)
        {
            Vector2 dir = ((Vector2)transform.position - (Vector2)attacker.transform.position).normalized;
            Rigidbody2D rb = GetComponentInChildren<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }

        // 무적 및 깜빡임 효과
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkAndInvincibleCoroutine());

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            if (dangerGaugeSystem != null)
            {
                dangerGaugeSystem.KillPlayer("HP 0");
            }
            Died?.Invoke(); 
        }

        if (enableAutoHeal && currentHP > 0f)
        {
            if (autoHealCoroutine != null)
            {
                StopCoroutine(autoHealCoroutine);
            }
            autoHealCoroutine = StartCoroutine(AutoHealCoroutine());
        }
    }

    private System.Collections.IEnumerator BlinkAndInvincibleCoroutine()
    {
        isInvincible = true;
        if (invincibleSpriteRenderers != null && invincibleSpriteRenderers.Length > 0)
        {
            Color[] originalColors = new Color[invincibleSpriteRenderers.Length];
            for (int j = 0; j < invincibleSpriteRenderers.Length; j++)
                if (invincibleSpriteRenderers[j] != null)
                    originalColors[j] = invincibleSpriteRenderers[j].color;
            for (int i = 0; i < blinkCount; i++)
            {
                for (int j = 0; j < invincibleSpriteRenderers.Length; j++)
                    if (invincibleSpriteRenderers[j] != null)
                        invincibleSpriteRenderers[j].color = blinkColor;
                yield return new WaitForSeconds(invincibleTime / (blinkCount * 2f));
                for (int j = 0; j < invincibleSpriteRenderers.Length; j++)
                    if (invincibleSpriteRenderers[j] != null)
                        invincibleSpriteRenderers[j].color = originalColors[j];
                yield return new WaitForSeconds(invincibleTime / (blinkCount * 2f));
            }
        }
        else if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            for (int i = 0; i < blinkCount; i++)
            {
                spriteRenderer.color = blinkColor;
                yield return new WaitForSeconds(invincibleTime / (blinkCount * 2f));
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(invincibleTime / (blinkCount * 2f));
            }
        }
        else
        {
            yield return new WaitForSeconds(invincibleTime);
        }
        isInvincible = false;

        // 무적 끝난 직후, Enemy 레이어와 겹쳐 있으면 적에게서 다시 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            if (hit.gameObject != this.gameObject)
            {
                var enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    TakeDamage(enemy.AttackPower, hit.gameObject);
                    break; // 한 번만
                }
            }
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
