using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 임시 인벤토리에 있는 자원을 주 저장소로 옮기는 상자입니다.
/// IInteractable 인터페이스를 구현하여 플레이어의 상호작용에 반응합니다.
/// ResourceManager의 총 자원량에 따라 시각적 표현이 변화합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Chest : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public struct VisualStep
    {
        public GameObject visualObject;
        public int requiredAmount;
    }

    [Header("Interaction Settings")]
    [SerializeField] private string interactionText = "상자에 보관";
    [SerializeField] private string emptyInventoryText = "보관할 자원이 없습니다";
    [SerializeField] private float interactionRange = 2.5f;

    [Header("Diegetic Visuals")]
    [Tooltip("자원량에 따라 순차적으로 활성화될 오브젝트 목록")]
    [SerializeField] private List<VisualStep> visualSteps = new List<VisualStep>();

    private TemporaryInventory _playerTemporaryInventory;

    private void Awake()
    {
        // 콜라이더가 트리거로 설정되어 있는지 확인합니다.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        // 자원 변경 이벤트가 발생할 때마다 시각적 업데이트를 하도록 등록합니다.
        GameEvents.OnResourceChanged += UpdateVisualsFromEvent;
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 구독을 해제합니다.
        GameEvents.OnResourceChanged -= UpdateVisualsFromEvent;
    }

    private void Start()
    {
        // 게임 시작 시 초기 시각적 상태를 설정합니다.
        // 모든 시각적 오브젝트를 비활성화 상태로 시작합니다.
        foreach (var step in visualSteps)
        {
            if (step.visualObject != null)
                step.visualObject.SetActive(false);
        }
        UpdateVisuals();
    }

    /// <summary>
    /// 플레이어가 상자와 상호작용할 수 있는지 여부를 결정합니다.
    /// 임시 인벤토리에 아이템이 하나라도 있어야 상호작용이 가능합니다.
    /// </summary>
    /// <returns>상호작용 가능하면 true, 아니면 false</returns>
    public bool CanInteract()
    {
        // 플레이어의 임시 인벤토리를 찾습니다. 매번 찾지 않도록 캐싱을 고려할 수 있습니다.
        if (_playerTemporaryInventory == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTemporaryInventory = player.GetComponent<TemporaryInventory>();
            }
        }

        if (_playerTemporaryInventory != null)
        {
            // 임시 인벤토리에 자원이 하나라도 있는지 확인합니다.
            return _playerTemporaryInventory.GetAllTempResources().Count > 0;
        }

        return false;
    }

    /// <summary>
    /// 상호작용이 시작될 때 호출됩니다.
    /// 임시 인벤토리의 모든 자원을 주 저장소로 옮깁니다.
    /// </summary>
    public void Interact()
    {
        if (!CanInteract() || _playerTemporaryInventory == null) return;

        _playerTemporaryInventory.DepositAllToMainInventory();
        Debug.Log("상자: 임시 인벤토리의 모든 자원을 주 저장소로 옮겼습니다.");
        // DepositAllToMainInventory가 GameEvents.OnResourceChanged를 호출하므로, 시각적 업데이트는 자동으로 처리됩니다.
        // 여기에 사운드나 시각 효과를 추가할 수 있습니다.
    }

    /// <summary>
    /// 상호작용이 중단될 때 호출됩니다. (단발성 상호작용이므로 비워둡니다)
    /// </summary>
    public void StopInteract()
    {
        // 이 오브젝트는 홀드 상호작용이 아니므로 특별한 로직이 필요 없습니다.
    }

    /// <summary>
    /// 상호작용 UI에 표시될 텍스트를 반환합니다.
    /// </summary>
    public string GetInteractionText()
    {
        return CanInteract() ? interactionText : emptyInventoryText;
    }

    /// <summary>
    /// GameEvents.OnResourceChanged 이벤트에 연결될 메서드입니다.
    /// 이벤트가 발생할 때마다 UpdateVisuals()를 호출합니다.
    /// </summary>
    /// <param name="allResources">ResourceManager에 있는 모든 자원의 최신 상태입니다. (이벤트 시그니처를 맞추기 위한 용도이며 직접 사용하지는 않습니다.)</param>
    private void UpdateVisualsFromEvent(Dictionary<MineralData, int> allResources)
    {
        UpdateVisuals();
    }

    /// <summary>
    /// ResourceManager의 총 자원량에 따라 상자의 시각적 표현을 업데이트합니다.
    /// </summary>
    private void UpdateVisuals()
    {
        if (ResourceManager.Instance == null) return;

        int totalResources = ResourceManager.Instance.GetTotalResourceCount();

        // 각 단계별로 조건을 확인하고 오브젝트를 활성화합니다.
        foreach (var step in visualSteps)
        {
            if (step.visualObject != null)
            {
                // 현재 총 자원량이 해당 단계의 요구량보다 많거나 같으면 활성화
                bool shouldBeActive = totalResources >= step.requiredAmount;
                if (step.visualObject.activeSelf != shouldBeActive)
                {
                    step.visualObject.SetActive(shouldBeActive);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}