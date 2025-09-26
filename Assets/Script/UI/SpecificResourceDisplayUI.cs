using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 지정된 각 자원의 수량을 별개의 UI 요소에 표시하는 스크립트입니다.
/// 자원이 1개 이상일 때만 해당 UI 요소를 보여줍니다.
/// </summary>
public class SpecificResourceDisplayUI : MonoBehaviour
{
    [System.Serializable]
    public struct ResourceDisplayBinding
    {
        [Tooltip("수량을 추적할 광물 데이터 (ScriptableObject)")]
        public MineralData mineral;
        [Tooltip("수량을 표시할 TextMeshProUGUI 컴포넌트")]
        public TextMeshProUGUI amountText;
        [Tooltip("자원이 1개 이상일 때만 활성화할 게임 오브젝트 (아이콘, 텍스트 등을 포함하는 부모 오브젝트)")]
        public GameObject displayContainer;
    }

    [Header("자원 UI 연결")]
    [SerializeField]
    private List<ResourceDisplayBinding> resourceDisplays;

    private void OnEnable()
    {
        // UI가 활성화될 때 자원 정보를 즉시 업데이트하고, 이벤트 리스너를 등록합니다.
        GameEvents.OnResourceChanged += UpdateAllDisplays;
        if (ResourceManager.Instance != null)
        {
            UpdateAllDisplays(ResourceManager.Instance.GetAllResources());
        }
    }

    private void OnDisable()
    {
        // UI가 비활성화될 때 이벤트 리스너를 해제합니다.
        GameEvents.OnResourceChanged -= UpdateAllDisplays;
    }

    /// <summary>
    /// ResourceManager의 모든 자원 정보를 받아 연결된 모든 UI를 업데이트합니다.
    /// </summary>
    private void UpdateAllDisplays(Dictionary<MineralData, int> allResources)
    {
        if (ResourceManager.Instance == null) return;

        // 연결된 모든 자원 표시에 대해 반복
        foreach (var display in resourceDisplays)
        {
            if (display.mineral == null || display.amountText == null || display.displayContainer == null) continue;

            // ResourceManager에서 해당 자원의 현재 수량을 가져옵니다.
            int amount = ResourceManager.Instance.GetResourceAmount(display.mineral);

            // 자원의 수량과 관계없이 항상 UI를 활성화하고 텍스트를 업데이트합니다.
            display.displayContainer.SetActive(true); // 항상 활성화
            display.amountText.text = $"x {amount}";
        }
    }
}