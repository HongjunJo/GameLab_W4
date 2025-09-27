// Assets/Script/Data/AbilityData.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 적용할 능력의 종류를 정의합니다.
/// </summary>
public enum AbilityType
{
    None,
    PlayerDoubleJumpOn, // 플레이어 더블 점프 증가
    PlayerDashOn,  // 플레이어 대시 활성화
    AttackPowerIncrease, // 공격력 증가
    O2Increase,          // 산소통 증가
    // ... 여기에 새로운 능력 종류를 추가할 수 있습니다.
}

/// <summary>
/// 자원의 종류(MineralData)와 필요 수량(amount)을 정의하는 데이터 구조체입니다.
/// 건물 레시피, 능력 구매 비용 등 다양한 곳에서 재사용됩니다.
/// </summary>
[System.Serializable]
public struct ResourceCost
{
    public MineralData mineral;
    public int amount;
}


/// <summary>
/// 하나의 능력을 정의하는 데이터 클래스. ScriptableObject로 만들어 에셋으로 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Ability", menuName = "Game Data/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("기본 정보")]
    public string abilityName; // 능력 이름

    [TextArea(3, 5)]
    public string description; // 능력 설명

    [Header("능력 효과")]
    public AbilityType type; // 이 능력이 어떤 종류인지
    public float value;      // 능력치에 적용할 값 (예: 속도 1.1배, 점프력 +2)
    public Sprite icon; // 상점 UI에 표시될 아이콘 (선택 사항)

    [Header("구매 비용")]
    public List<ResourceCost> costs; // 구매에 필요한 자원 목록

    [Header("상태 (런타임 전용)")]
    [HideInInspector] public bool isPurchased = false; // 구매 여부 (런타임 중에만 변경)

}
