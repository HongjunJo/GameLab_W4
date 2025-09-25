// Assets/Script/UI/HealthVignetteController.cs

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // URP를 사용하는 경우. HDRP는 네임스페이스가 다를 수 있습니다.

/// <summary>
/// 플레이어의 체력에 따라 포스트 프로세싱 볼륨의 Vignette 강도를 조절합니다.
/// </summary>
public class HealthVignetteController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("효과를 적용할 글로벌 볼륨")]
    [SerializeField] private Volume postProcessVolume;
    [Tooltip("참조할 플레이어의 Health 컴포넌트. 비워두면 'Player' 태그로 찾습니다.")]
    [SerializeField] private Health playerHealth;

    [Header("Vignette Settings")]
    [Tooltip("효과가 나타나기 시작하는 체력 비율 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    [SerializeField] private float healthThreshold = 0.6f; // 60%

    [Tooltip("체력이 Threshold일 때의 Vignette 최소 강도")]
    [Range(0f, 1f)]
    [SerializeField] private float minIntensity = 0f;

    [Tooltip("체력이 0일 때의 Vignette 최대 강도")]
    [Range(0f, 1f)]
    [SerializeField] private float maxIntensity = 0.6f;
    
    [Tooltip("체력이 Threshold일 때의 Vignette 최소 부드러움")]
    [Range(0.01f, 1f)]
    [SerializeField] private float minSmoothness = 0.5f;

    [Tooltip("체력이 0일 때의 Vignette 최대 부드러움")]
    [Range(0.01f, 1f)]
    [SerializeField] private float maxSmoothness = 1f;
    
    [Tooltip("Vignette 색상")]
    [SerializeField] private Color vignetteColor = Color.red;

    [Header("Fade Settings")]
    [Tooltip("Vignette 강도가 변할 때의 부드러운 전환 속도")]
    [SerializeField] private float fadeSpeed = 2.0f;
    
    private Vignette vignette; // 제어할 Vignette 오버라이드
    private float currentIntensity;
    private float targetIntensity;
    private float currentSmoothness;
    private float targetSmoothness;

    private void Start()
    {
        // 플레이어 Health 컴포넌트 찾기
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }

        // 볼륨 프로필에서 Vignette 컴포넌트 가져오기
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            // ❗️핵심: 프로파일을 복제하여 새로운 인스턴스를 만듭니다.
            // 이렇게 해야 원본 에셋을 건드리지 않고 런타임에서 안전하게 수정할 수 있습니다.
            postProcessVolume.profile = Instantiate(postProcessVolume.profile);

            if (!postProcessVolume.profile.TryGet(out vignette))
            {
                Debug.LogError("Vignette 컴포넌트를 프로파일에서 찾을 수 없습니다. Global Volume에 Vignette Override가 추가되었는지 확인하세요.");
                this.enabled = false;
                return;
            }

            // 스크립트에서 제어할 수 있도록 오버라이드 상태로 설정
            vignette.color.Override(vignetteColor);
            vignette.intensity.Override(0f); // 시작은 0으로
            vignette.smoothness.Override(minSmoothness);
        }

        // Health의 HP 변경 이벤트 구독
        if (playerHealth != null)
        {
            playerHealth.HPChanged += OnHealthChanged;
            // 게임 시작 시 초기 상태 설정
            OnHealthChanged(playerHealth.CurrentHP, playerHealth.MaxHP);
        }
        else
        {
            Debug.LogError("Player Health 컴포넌트를 찾을 수 없습니다!");
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // 현재 강도를 목표 강도로 부드럽게 전환합니다.
        if (!Mathf.Approximately(currentIntensity, targetIntensity) || !Mathf.Approximately(currentSmoothness, targetSmoothness))
        {
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * fadeSpeed);
            currentSmoothness = Mathf.Lerp(currentSmoothness, targetSmoothness, Time.deltaTime * fadeSpeed);

            // 부동 소수점 오류를 방지하기 위해 아주 작은 값은 0으로 처리합니다.
            if (currentIntensity < 0.001f)
            {
                currentIntensity = 0f;
            }
            
            vignette.intensity.Override(currentIntensity);
            vignette.smoothness.Override(currentSmoothness);

            // ✨ 핵심: 효과의 활성 상태를 직접 제어하여 렌더링을 강제로 갱신합니다.
            // 강도가 0보다 클 때만 활성화하여 불필요한 연산을 줄이는 최적화 효과도 있습니다.
            vignette.active = currentIntensity > 0;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerHealth != null)
        {
            playerHealth.HPChanged -= OnHealthChanged;
        }
        // 스크립트 비활성화 시 효과 초기화
        if (vignette != null)
        {
            // OnDestroy에서는 즉시 0으로 설정
            vignette.intensity.Override(0f); 
            vignette.smoothness.Override(minSmoothness);
        }
    }

    /// <summary>
    /// 플레이어 체력이 변경될 때 호출되는 메서드
    /// </summary>
    private void OnHealthChanged(float currentHP, float maxHP)
    {
        if (vignette == null) return;

        float healthPercent = currentHP / maxHP;
        float newTargetIntensity = minIntensity;
        float newTargetSmoothness = minSmoothness;

        // 체력이 설정된 임계값(Threshold) 이하로 떨어졌을 때만 효과를 계산합니다.
        if (healthPercent < healthThreshold)
        {
            // (healthThreshold ~ 0%) 구간을 (0 ~ 1) 범위로 정규화합니다.
            // 체력이 낮을수록 1에 가까워집니다.
            float effectRange = 1f - (healthPercent / healthThreshold);

            // 정규화된 값을 사용하여 0에서 최대 강도 사이의 값을 계산합니다.
            newTargetIntensity = Mathf.Lerp(minIntensity, maxIntensity, effectRange);
            newTargetSmoothness = Mathf.Lerp(minSmoothness, maxSmoothness, effectRange);
        }
        // 목표 강도를 업데이트합니다. 실제 적용은 Update에서 처리됩니다.
        targetIntensity = newTargetIntensity;
        targetSmoothness = newTargetSmoothness;
    }
}
