using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어가 사망 시 떨어뜨린 아이템을 보관하는 컨테이너입니다.
/// 플레이어와 상호작용하여 아이템을 돌려줄 수 있습니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DeathBox : MonoBehaviour
{
    // 보관된 아이템 목록
    private Dictionary<MineralData, (int amount, List<ResourceSource> sources)> storedItems;

    // 플레이어 감지 및 상호작용 관련 변수
    private bool isPlayerInRange = false;
    private TemporaryInventory playerInventoryCache;

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.F;

    /// <summary>
    /// 이 컨테이너에 아이템을 설정합니다.
    /// </summary>
    /// <param name="itemsToStore">플레이어의 임시 인벤토리에서 전달받은 아이템 목록</param>
    public void Initialize(Dictionary<MineralData, (int amount, List<ResourceSource> sources)> itemsToStore)
    {
        storedItems = new Dictionary<MineralData, (int, List<ResourceSource>)>(itemsToStore);
        Debug.Log($"사망 지점에 {storedItems.Count} 종류의 아이템이 담긴 가방이 생성되었습니다.");
    }

    private void Awake()
    {
        // 콜라이더가 트리거로 설정되어 있는지 확인합니다.
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }
    /// <summary>
    /// 플레이어가 아이템을 회수할 때 호출됩니다.
    /// </summary>
    /// <param name="playerInventory">아이템을 받을 플레이어의 임시 인벤토리</param>
    public void RetrieveItems(TemporaryInventory playerInventory)
    {
        if (storedItems == null || storedItems.Count == 0)
        {
            Debug.Log("가방이 비어있습니다.");
            Destroy(gameObject);
            return;
        }

        Debug.Log("가방에서 아이템을 회수합니다.");
        foreach (var item in storedItems)
        {
            MineralData mineral = item.Key;
            int amount = item.Value.amount;
            List<ResourceSource> sources = item.Value.sources;

            // 각 아이템은 1개씩, 해당하는 source와 함께 추가되어야 합니다.
            for (int i = 0; i < amount; i++)
            {
                // sources 리스트가 비어있지 않다면, 마지막 요소를 반복해서 사용합니다.
                // 이는 TemporaryInventory.AddResource가 아이템을 여러 개 추가해도 source는 하나만 기록하는 방식에 대응하기 위함입니다.
                if (sources != null && sources.Count > 0)
                {
                    playerInventory.AddResource(mineral, 1, sources[sources.Count - 1]);
                }
                else
                {
                    // 만약의 경우 source 정보가 없더라도 아이템은 돌려받아야 합니다.
                    playerInventory.AddResource(mineral, 1, null);
                }
            }
        }

        // 모든 아이템을 돌려준 후 가방(오브젝트)을 파괴합니다.
        storedItems.Clear();
        Destroy(gameObject);
    }

    private void Update()
    {
        // 플레이어가 범위 안에 있고 상호작용 키를 눌렀을 때 아이템 회수
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            if (playerInventoryCache != null)
            {
                RetrieveItems(playerInventoryCache);
            }
            else
            {
                Debug.LogWarning("플레이어 인벤토리를 찾을 수 없어 아이템을 회수할 수 없습니다.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerInventoryCache = other.GetComponent<TemporaryInventory>();
            // 여기에 "F키로 아이템 회수" 같은 UI를 띄우는 로직을 추가할 수 있습니다.
            Debug.Log("DeathBox 범위에 진입. F키로 아이템을 회수할 수 있습니다.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerInventoryCache = null;
            // 여기에 상호작용 UI를 숨기는 로직을 추가할 수 있습니다.
            Debug.Log("DeathBox 범위에서 이탈.");
        }
    }
}
