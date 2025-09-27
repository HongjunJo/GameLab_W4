using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 광산 - 특정 광물을 주기적으로 생산하는 건물
/// </summary>
public class Mine : MonoBehaviour, IInteractable
{
    [Header("Mine Settings")]
    [SerializeField] private BuildingRecipe activationRecipe;
    [SerializeField] private bool isBuilt = false;
    
    [Header("Visual")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Material activeMaterial;
    
    // Public 속성들
    public bool IsBuilt => isBuilt;

    private TemporaryInventory _playerTemporaryInventory;
    
    private void Awake()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
        
        UpdateVisual();
    }
    
    private void Start()
    {
        // 콜라이더 설정
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        // 플레이어의 임시 인벤토리 참조를 가져옵니다.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            _playerTemporaryInventory = playerObject.GetComponent<TemporaryInventory>();
        }
        if (_playerTemporaryInventory == null)
        {
            Debug.LogError("Mine: Player의 TemporaryInventory를 찾을 수 없습니다!");
        }
    }
    
    public void StopInteract()
    {
        // 광산은 홀드 상호작용이 없으므로 비워둡니다.
    }

    public bool CanInteract()
    {
        // 건설되지 않았고, 건설 비용이 충분할 때만 상호작용 가능
        return !isBuilt && activationRecipe != null && CanAffordWithTemporaryInventory(activationRecipe.resourceCosts);
    }
    
    public void Interact()
    {
        if (CanInteract())
        {
            BuildMine();
        }
    }

    public string GetInteractionText()
    {
        // 이미 건설되었다면 상호작용 텍스트를 표시하지 않음
        if (!isBuilt)
        {
            string costText = activationRecipe.GetCostAsString();
            if (CanInteract())
            {
                return $"Build {activationRecipe.recipeName}\n{costText}";
            }
            else
            {
                return $"Build {activationRecipe.recipeName}\n<color=red>Need more resources</color>\n{costText}";
            }
        }
        // return $"{activationRecipe.recipeName} (Built)\nPress 'W' to Teleport";
        return null;
    }
    
    /// <summary>
    /// 광산 건설
    /// </summary>
    private void BuildMine()
    {
        if (activationRecipe == null || !CanAffordWithTemporaryInventory(activationRecipe.resourceCosts)) return;
        
        // 비용 소모
        if (TrySpendCombinedResources(activationRecipe.resourceCosts))
        {
            isBuilt = true;
            
            // 비주얼 업데이트
            UpdateVisual();
            
            // 이벤트 발생
            GameEvents.BuildingActivated($"Mine_{activationRecipe.recipeName}");
            
            Debug.Log($"Mine built!");
        }
    }
    
    /// <summary>
    /// 비주얼 업데이트
    /// </summary>
    private void UpdateVisual()
    {
        if (objectRenderer == null) return;
        
        if (isBuilt && activeMaterial != null)
        {
            objectRenderer.material = activeMaterial;
        }
        else if (inactiveMaterial != null)
        {
            objectRenderer.material = inactiveMaterial;
        }
    }
    
    /// <summary>
    /// 수동으로 광산 설정 (테스트용)
    /// </summary>
    [ContextMenu("Build Mine (Test)")]
    public void BuildMineTest()
    {
        isBuilt = true;
        UpdateVisual();
    }

    /// <summary>
    /// 창고와 임시 인벤토리를 모두 확인하여 건설 가능한지 확인합니다.
    /// </summary>
    private bool CanAffordWithTemporaryInventory(List<ResourceCost> costs)
    {
        if (ResourceManager.Instance == null || _playerTemporaryInventory == null) return false;

        var tempResources = _playerTemporaryInventory.GetAllTempResources();

        foreach (var cost in costs)
        {
            int mainAmount = ResourceManager.Instance.GetResourceAmount(cost.mineral);
            int tempAmount = 0;
            if (tempResources.TryGetValue(cost.mineral, out var entry))
            {
                tempAmount = entry.amount;
            }

            if (mainAmount + tempAmount < cost.amount)
            {
                return false; // 하나라도 부족하면 즉시 false 반환
            }
        }
        return true; // 모든 자원이 충분하면 true 반환
    }

    /// <summary>
    /// 임시 인벤토리와 창고에서 자원을 순차적으로 소모합니다.
    /// </summary>
    private bool TrySpendCombinedResources(List<ResourceCost> costs)
    {
        if (!CanAffordWithTemporaryInventory(costs)) return false;

        foreach (var cost in costs)
        {
            int remainingCost = cost.amount;

            // 1. 임시 인벤토리에서 먼저 차감
            if (_playerTemporaryInventory != null)
            {
                int tempSpent = _playerTemporaryInventory.UseResource(cost.mineral, remainingCost);
                remainingCost -= tempSpent;
            }

            // 2. 남은 비용이 있다면 창고에서 차감
            if (remainingCost > 0)
            {
                ResourceManager.Instance.UseResource(cost.mineral, remainingCost);
            }
        }
        return true;
    }
}