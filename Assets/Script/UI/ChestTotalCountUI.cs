using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 상자에 보관된 모든 자원의 총 개수를 텍스트로 표시하는 UI 스크립트입니다.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class ChestTotalCountUI : MonoBehaviour
{
    private TextMeshProUGUI _totalAmountText;

    private void Awake()
    {
        _totalAmountText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        // UI가 활성화될 때 자원 정보를 즉시 업데이트하고, 이벤트 리스너를 등록합니다.
        GameEvents.OnResourceChanged += UpdateTotalCount;
        if (ResourceManager.Instance != null)
        {
            UpdateTotalCount(ResourceManager.Instance.GetAllResources());
        }
    }

    private void OnDisable()
    {
        // UI가 비활성화될 때 이벤트 리스너를 해제합니다.
        GameEvents.OnResourceChanged -= UpdateTotalCount;
    }

    /// <summary>
    /// ResourceManager의 모든 자원 정보를 받아 총 개수를 UI에 업데이트합니다.
    /// </summary>
    private void UpdateTotalCount(Dictionary<MineralData, int> allResources)
    {
        if (ResourceManager.Instance == null || _totalAmountText == null) return;

        int totalCount = ResourceManager.Instance.GetTotalResourceCount();
        
        // 자원이 하나 이상 있을 때만 텍스트를 표시합니다.
        _totalAmountText.text = totalCount > 0 ? $"x {totalCount}" : "";
    }
}