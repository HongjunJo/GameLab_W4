using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 플레이어의 임시 인벤토리를 관리합니다.
/// 탐험 지역에서 획득한 자원은 여기에 보관되며, 메인 섹터로 돌아가야 ResourceManager에 등록됩니다.
/// </summary>
public class TemporaryInventory : MonoBehaviour
{
    // MineralData를 키로, 획득한 양과 원래 ResourceSource를 값으로 저장
    // C# 7.0 ValueTuple을 사용합니다. (amount, sources)
    private readonly Dictionary<MineralData, (int amount, List<ResourceSource> sources)> tempResources = new Dictionary<MineralData, (int, List<ResourceSource>)>();
    public static event Action<Dictionary<MineralData, (int amount, List<ResourceSource> sources)>> OnTemporaryResourceChanged;

    [System.Serializable]
    public class TempResourceDisplay
    {
        public MineralData mineral;
        public int amount;
    }

    [Header("Debug - Temporary Resources")]
    [SerializeField] private List<TempResourceDisplay> debugTempResources = new List<TempResourceDisplay>();

    [Header("사망 설정")]
    [SerializeField] private GameObject deathBoxPrefab; // 사망 시 생성될 아이템 상자 프리팹

    /// <summary>
    /// 임시 인벤토리에 자원을 추가합니다.
    /// </summary>
    /// <param name="mineral">추가할 광물 데이터</param>
    /// <param name="amount">추가할 양</param>
    /// <param name="source">자원의 출처 (재생성을 위해)</param>
    public void AddResource(MineralData mineral, int amount, ResourceSource source)
    {
        if (tempResources.ContainsKey(mineral))
        {
            var entry = tempResources[mineral];
            entry.amount += amount;
            entry.sources.Add(source);
            tempResources[mineral] = entry;
        }
        else
        {
            tempResources[mineral] = (amount, new List<ResourceSource> { source });
        }

        Debug.Log($"[임시] {mineral.name} {amount}개 추가. 현재 임시 보유량: {tempResources[mineral].amount}");
        UpdateDebugDisplay();

        // 임시 인벤토리 변경 사항을 UI에 알립니다.
        OnTemporaryResourceChanged?.Invoke(new Dictionary<MineralData, (int, List<ResourceSource>)>(tempResources));
    }

    /// <summary>
    /// 임시 인벤토리의 모든 자원을 ResourceManager로 옮기고 임시 인벤토리를 비웁니다.
    /// </summary>
    public void DepositAllToMainInventory()
    {
        foreach (var item in tempResources)
        {
            ResourceManager.Instance.AddResource(item.Key, item.Value.amount);
        }
        Debug.Log($"임시 인벤토리의 모든 자원을 메인 인벤토리로 이전했습니다.");
        tempResources.Clear();

        // 임시 인벤토리가 비워졌음을 UI에 알립니다.
        OnTemporaryResourceChanged?.Invoke(new Dictionary<MineralData, (int, List<ResourceSource>)>(tempResources));
        UpdateDebugDisplay();
    }

    /// <summary>
    /// 플레이어 사망 시 임시 인벤토리의 모든 아이템을 담은 가방(Crate)을 생성합니다.
    /// </summary>
    public void DropAllItemsOnDeath()
    {
        if (tempResources.Count == 0)
        {
            Debug.Log("임시 인벤토리가 비어있어 아무것도 떨어뜨리지 않습니다.");
            return;
        }

        if (deathBoxPrefab == null)
        {
            Debug.LogError("Death Box 프리팹이 할당되지 않았습니다! 아이템을 떨어뜨릴 수 없습니다.");
            return;
        }

        Debug.Log("플레이어 사망. 임시 인벤토리의 모든 아이템을 그 자리에 떨어뜨립니다.");
        GameObject boxInstance = Instantiate(deathBoxPrefab, transform.position, Quaternion.identity);
        DeathBox deathBox = boxInstance.GetComponent<DeathBox>();
        deathBox?.Initialize(tempResources);

        // 모든 아이템을 DeathBox로 옮겼으므로 임시 인벤토리를 비웁니다.
        tempResources.Clear();
        OnTemporaryResourceChanged?.Invoke(new Dictionary<MineralData, (int, List<ResourceSource>)>(tempResources));
        UpdateDebugDisplay();
    }

    /// <summary>
    /// 현재 임시 자원 목록을 반환합니다.
    /// </summary>
    public Dictionary<MineralData, (int amount, List<ResourceSource> sources)> GetAllTempResources()
    {
        return new Dictionary<MineralData, (int, List<ResourceSource>)>(tempResources);
    }

    /// <summary>
    /// 임시 인벤토리에서 특정 자원을 지정된 양만큼 사용합니다.
    /// </summary>
    /// <param name="mineral">사용할 광물</param>
    /// <param name="amountToUse">사용하려는 양</param>
    /// <returns>실제로 사용된 양</returns>
    public int UseResource(MineralData mineral, int amountToUse)
    {
        if (!tempResources.ContainsKey(mineral)) return 0;

        var entry = tempResources[mineral];
        int spentAmount = Mathf.Min(entry.amount, amountToUse);

        entry.amount -= spentAmount;

        if (entry.amount <= 0)
        {
            tempResources.Remove(mineral);
        }
        else
        {
            tempResources[mineral] = entry;
        }

        return spentAmount;
    }

    private void UpdateDebugDisplay()
    {
        debugTempResources.Clear();
        foreach (var kvp in tempResources)
        {
            debugTempResources.Add(new TempResourceDisplay { mineral = kvp.Key, amount = kvp.Value.amount });
        }
    }
}