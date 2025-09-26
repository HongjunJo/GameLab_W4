using UnityEngine;
using System.Collections.Generic; // Dictionary를 사용하기 위해 추가
using TMPro; // TextMeshPro를 사용하기 위해 추가

// 스크립트의 역할을 더 명확하게 알 수 있도록 이름을 변경하는 것을 추천합니다. (예: PlayerInventoryUIController)
public class PlayerInventoryUIController : MonoBehaviour
{
    [Header("UI 표시/숨김 설정")]
    [Tooltip("표시하거나 숨길 UI 게임 오브젝트를 여기에 연결하세요.")]
    public GameObject inventoryPanel; // 실제 켜고 끌 UI 패널 (예: 인벤토리의 배경 이미지)

    [Tooltip("UI를 활성화할 키입니다.")]
    public KeyCode activationKey = KeyCode.C; // 인벤토리(C) 키를 기본값으로 설정

    [Tooltip("UI가 나타나기까지 키를 누르고 있어야 하는 시간입니다.")]
    public float holdDuration = 0.5f; // 0.5초 누르고 있으면 UI가 켜지도록 설정

    [Header("인벤토리 표시 설정")]
    [Tooltip("게임에 존재하는 모든 광물 종류(MineralData)를 여기에 등록하세요.")]
    [SerializeField] private List<MineralData> allMineralTypes = new List<MineralData>();

    [Tooltip("각 광물의 개수를 표시할 TextMeshProUGUI UI 요소들을 순서대로 연결하세요.")]
    [SerializeField] private List<TextMeshProUGUI> mineralCountTexts = new List<TextMeshProUGUI>();

    [Header("UI 방향 고정 설정")]
    [SerializeField] private Transform target; // 플레이어 Transform

    private TemporaryInventory playerTemporaryInventory;
    private float holdTimer = 0f;

    void Awake()
    {
        // 플레이어의 임시 인벤토리 컴포넌트를 찾습니다.
        if (target != null)
        {
            playerTemporaryInventory = target.GetComponent<TemporaryInventory>();
        }

        if (playerTemporaryInventory == null)
        {
            Debug.LogError("Player의 TemporaryInventory를 찾을 수 없습니다!", this);
            enabled = false;
            return;
        }

        if (allMineralTypes.Count != mineralCountTexts.Count)
        {
            Debug.LogWarning("allMineralTypes와 mineralCountTexts의 개수가 일치하지 않습니다. UI가 올바르게 표시되지 않을 수 있습니다.", this);
        }
    }

    void Start()
    {
        // 시작할 때 UI는 항상 꺼진 상태로 만듦
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // 임시 인벤토리의 자원이 변경될 때마다 UI를 업데이트하도록 이벤트 구독
        // TemporaryInventory의 static 이벤트를 구독합니다.
        TemporaryInventory.OnTemporaryResourceChanged += HandleInventoryChange;
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 구독 해제
        // TemporaryInventory의 static 이벤트 구독을 해제합니다.
        TemporaryInventory.OnTemporaryResourceChanged -= HandleInventoryChange;
    }

    void Update()
    {
        // 3. 키 입력 처리 (UI 켜고 끄기)
        // 설정된 키(activationKey)를 누르고 있을 때
        if (Input.GetKey(activationKey))
        {
            holdTimer += Time.deltaTime; // 타이머 시간 증가

            // 타이머가 설정된 시간(holdDuration)을 넘어서면 UI를 켭니다.
            if (holdTimer >= holdDuration)
            {
                ShowInventory();
            }
        }
        // 설정된 키에서 손을 뗐을 때
        else if (Input.GetKeyUp(activationKey))
        {
            holdTimer = 0f; // 타이머 초기화
            HideInventory(); // UI를 끕니다.
        }
    }

    void LateUpdate()
    {
        // 4. 방향 및 크기 고정 (사용자 요청에 따라 이 로직은 수정하지 않음)
        if (target != null)
        {
            if (Mathf.Sign(target.localScale.x) == -1)
            {
                transform.localScale = new Vector3(-1, 1, 1); // 부모의 스케일 영향을 제거
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1); // 부모의 스케일 영향을 제거
            }
        }

        // 부모 오브젝트의 움직임이 모두 끝난 후 마지막에 처리
        transform.localRotation = Quaternion.identity; // 부모의 회전 영향을 제거
    }

    // UI를 켜는 함수
    private void ShowInventory()
    {
        if (inventoryPanel != null && !inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
            UpdateInventoryDisplay(playerTemporaryInventory.GetAllTempResources()); // UI를 켤 때 현재 인벤토리 상태를 즉시 업데이트
        }
    }

    // UI를 끄는 함수
    private void HideInventory()
    {
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 임시 인벤토리의 내용을 UI에 업데이트합니다.
    /// </summary>
    private void HandleInventoryChange(Dictionary<MineralData, (int amount, List<ResourceSource> sources)> updatedResources)
    {
        // 인벤토리 패널이 비활성화 상태여도 이 이벤트는 호출될 수 있으므로, 활성화 상태일 때만 UI를 업데이트합니다.
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            UpdateInventoryDisplay(updatedResources);
        }
    }

    private void UpdateInventoryDisplay(Dictionary<MineralData, (int amount, List<ResourceSource> sources)> resources)
    {
        for (int i = 0; i < allMineralTypes.Count; i++)
        {
            if (i >= mineralCountTexts.Count || mineralCountTexts[i] == null) continue;

            MineralData mineral = allMineralTypes[i];
            int count = 0;

            // 임시 인벤토리에 해당 광물이 있으면 개수를 가져오고, 없으면 0으로 유지
            if (resources.TryGetValue(mineral, out var entry))
            {
                count = entry.amount;
            }

            // TextMeshPro 텍스트 업데이트
            mineralCountTexts[i].text = $"X {count}";
        }
    }
}
