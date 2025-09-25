using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어의 산소량(DangerGauge)을 월드스페이스 UI에 표시하고 플레이어를 따라다니게 합니다.
/// </summary>
public class PlayerOxygenUIController : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("산소량을 표시할 UI 슬라이더를 씬에서 직접 할당하세요.")]
    [SerializeField] private Slider oxygenBarSlider;

    private void Awake()
    {
        if (oxygenBarSlider == null)
        {
            Debug.LogError("인스펙터에서 Oxygen Bar Slider가 할당되지 않았습니다! 스크립트를 비활성화합니다.", this);
            enabled = false; // 스크립트의 추가 동작을 막습니다.
            return;
        }
        // UI를 항상 표시합니다.
        oxygenBarSlider.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        // 산소량(위험도)이 변경될 때마다 UI를 업데이트하도록 이벤트 구독
        GameEvents.OnDangerChanged += UpdateOxygenDisplay;
    }
    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 구독 해제
        GameEvents.OnDangerChanged -= UpdateOxygenDisplay;
    }
    /// <summary>
    /// GameEvents.OnDangerChanged 이벤트가 발생했을 때 호출될 메서드입니다.
    /// </summary>
    private void UpdateOxygenDisplay(float currentOxygen, float maxOxygen)
    {
        if (oxygenBarSlider == null) return;

        if (maxOxygen <= 0) return;

        // 항상 산소량 진행률을 업데이트합니다.
        float progress = currentOxygen / maxOxygen;
        oxygenBarSlider.value = progress;
    }
}
