using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 월드스페이스 UI에서 단일 자원의 아이콘과 수량을 표시하는 컴포넌트입니다.
/// </summary>
public class ResourceDisplayItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    /// <summary>
    /// 지정된 자원 데이터와 수량으로 UI 아이템을 설정합니다.
    /// </summary>
    /// <param name="mineralData">표시할 자원의 MineralData ScriptableObject</param>
    /// <param name="amount">표시할 자원의 수량</param>
    public void SetItem(MineralData mineralData, int amount)
    {
        if (mineralData == null) return;

        iconImage.sprite = mineralData.icon;
        amountText.text = $"x{amount}";
    }
}