// Assets/Script/UI/ShopManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text; // StringBuilder를 사용하기 위해 추가

public class ShopManager : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityDescriptionText; // 능력 설명 텍스트
    [SerializeField] private List<TextMeshProUGUI> costTexts; // 5개의 비용 텍스트 리스트
    [SerializeField] private Image abilityIconImage; // 아이콘을 표시할 이미지
    [SerializeField] private Button purchaseButton; // 구매 버튼

    [Header("능력 데이터")]
    [Tooltip("상점에서 판매할 능력(AbilityData) 목록")]
    [SerializeField] private List<AbilityData> abilitiesForSale;

    [Header("능력 버튼")]
    [Tooltip("상점 UI에 있는 능력 선택 버튼들")]
    [SerializeField] private List<Button> abilityButtons; // 능력 선택 버튼

    private AbilityData currentlySelectedAbility;
    private int selectedButtonIndex = -1; // 키보드 네비게이션을 위한 현재 선택된 버튼 인덱스
    private int confirmedSelectionIndex = -1; // Enter로 확정된 선택 인덱스

    private TemporaryInventory playerTemporaryInventory;
    private Color defaultButtonColor = Color.white; // 버튼 기본 색상
    private Color selectedButtonColor = new Color(0.8f, 0.9f, 1f); // 선택된 버튼 하이라이트 색상

    // UI가 활성화될 때 호출
    private void OnEnable()
    {
        // UI가 켜지면 첫 번째 버튼을 자동으로 선택
        if (abilityButtons.Count > 0)
        {
            SelectButton(0);
            confirmedSelectionIndex = -1; // UI가 켜질 때 확정된 선택은 초기화
        }
        UpdatePurchaseButtonState(); // 구매 버튼 상태도 업데이트
    }

    // UI가 비활성화될 때 호출
    private void OnDisable()
    {
        ClearSelection();
        RestoreAllButtonColors(); // 모든 버튼 색상을 원래대로 복원
    }

    void Start()
    {
        // --- 게임 시작 시 모든 능력의 구매 상태를 초기화 ---
        foreach (var ability in abilitiesForSale)
        {
            ability.isPurchased = false;
        }

        // 판매할 능력의 수와 버튼의 수가 맞는지 확인
        if (abilitiesForSale.Count != abilityButtons.Count)
        {
            Debug.LogWarning("판매할 능력의 수와 버튼의 수가 일치하지 않습니다!");
        }

        // 각 능력 버튼에 클릭 이벤트 및 초기 아이콘 설정
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (i < abilitiesForSale.Count)
            {
                int index = i; // 클로저 문제 방지를 위해 인덱스 복사
                abilityButtons[i].onClick.AddListener(() => SelectAbility(index));

                // 버튼에 능력 아이콘이 있다면 설정
                Image buttonIcon = abilityButtons[i].GetComponent<Image>();
                if (buttonIcon != null && abilitiesForSale[i].icon != null)
                {
                    buttonIcon.sprite = abilitiesForSale[i].icon;
                }

                if (abilitiesForSale[i].isPurchased)
                {
                    SetButtonPurchased(abilityButtons[i]);
                }
            }
        }

        purchaseButton.onClick.AddListener(PurchaseAbility);
        ClearSelection();

        // 플레이어의 임시 인벤토리 참조를 가져옵니다.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTemporaryInventory = playerObject.GetComponent<TemporaryInventory>();
        }
        if (playerTemporaryInventory == null) {
            Debug.LogError("ShopManager: Player의 TemporaryInventory를 찾을 수 없습니다!");
        }
    }

    private void Update()
    {
        // UI가 비활성화 상태이면 키보드 입력을 받지 않음
        if (!gameObject.activeInHierarchy) return;

        // 자원 획득 시 구매 버튼이 실시간으로 활성화되도록 매 프레임 상태를 업데이트합니다.
        UpdatePurchaseButtonState();

        // --- 키보드 네비게이션 로직 ---
        int row1Count = 4; // 첫 번째 줄의 버튼 수
        int row2Count = 3; // 두 번째 줄의 버튼 수

        // 키보드 네비게이션 처리
        if (selectedButtonIndex < abilityButtons.Count && selectedButtonIndex >= 0) // 능력 버튼이 선택된 경우
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SelectButton(selectedButtonIndex + 1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SelectButton(selectedButtonIndex - 1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // 현재 첫 번째 줄에 있다면 (인덱스 0-3)
                if (selectedButtonIndex < row1Count)
                {
                    // 목표 위치인 두 번째 줄에 활성화된 버튼이 있는지 확인합니다.
                    bool secondRowHasInteractable = false;
                    for (int i = row1Count; i < row1Count + row2Count; i++)
                    {
                        if (abilityButtons[i].interactable)
                        {
                            secondRowHasInteractable = true;
                            break;
                        }
                    }

                    if (secondRowHasInteractable)
                    {
                        // 두 번째 줄에 활성화된 버튼이 있으면, 아래로 이동
                        int targetIndex = Mathf.Min(selectedButtonIndex + row1Count, row1Count + row2Count - 1);
                        SelectButton(targetIndex);
                    }
                    else
                    {
                        // 두 번째 줄이 모두 비활성화 상태이면, 바로 구매 버튼으로 이동
                        SelectPurchaseButton();
                    }
                }
                // 현재 두 번째 줄에 있다면 (인덱스 4-6)
                else
                {
                    // 두 번째 줄에서는 아래로 누르면 무조건 구매 버튼으로 이동합니다.
                    SelectPurchaseButton();
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // 현재 두 번째 줄에 있다면 (인덱스 4-6)
                if (selectedButtonIndex >= row1Count)
                {
                    SelectButton(selectedButtonIndex - row1Count);
                }
            }
        }
        else // 구매 버튼이 선택된 경우 (selectedButtonIndex == abilityButtons.Count)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // 구매 버튼으로 이동하기 직전에 선택했던 능력 버튼으로 돌아갑니다.
                // confirmedSelectionIndex가 유효하면 그곳으로, 아니면 두 번째 줄 중앙 버튼으로 갑니다.
                int returnIndex = (confirmedSelectionIndex != -1) ? confirmedSelectionIndex : row1Count + 1;
                SelectButton(returnIndex);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // 구매 버튼에서 아래 방향키를 누르면 아무 동작도 하지 않도록 막습니다.
                // 이렇게 하면 인덱스 6번으로 이동하는 버그가 해결됩니다.
                return;
            }
        }

        // Enter 키로 현재 선택된 버튼의 기능을 실행 (능력 정보 표시)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (selectedButtonIndex == abilityButtons.Count) // 구매 버튼이 선택된 경우
            {
                // 구매 버튼의 onClick 이벤트를 직접 호출
                if (purchaseButton.interactable)
                {
                    purchaseButton.onClick.Invoke();
                }
            }
            else if (selectedButtonIndex >= 0 && selectedButtonIndex < abilityButtons.Count)
            {
                // Enter를 누르면 현재 하이라이트된 버튼을 "선택 확정" 상태로 만듭니다.
                SelectAbility(selectedButtonIndex);
                confirmedSelectionIndex = selectedButtonIndex;
                // 시각적 피드백을 위해 버튼 색상 업데이트
                UpdateButtonSelectionColors();
            }
        }
    }

    private void SelectButton(int index)
    {
        if (abilityButtons.Count == 0) return;

        int newIndex = index;
        int startIndex = newIndex;
        bool isMovingRight = index > selectedButtonIndex || (index == 0 && selectedButtonIndex == abilityButtons.Count - 1);

        // 비활성화된 버튼을 건너뛰는 로직
        while (true)
        {
            // 인덱스 범위 조정
            if (newIndex < 0) newIndex = abilityButtons.Count - 1;
            if (newIndex >= abilityButtons.Count) newIndex = 0;

            // 현재 버튼이 활성화 상태이면 선택하고 루프 종료
            if (abilityButtons[newIndex].interactable)
            {
                selectedButtonIndex = newIndex;
                abilityButtons[selectedButtonIndex].Select();
                // SelectAbility(selectedButtonIndex); // 방향키 이동 시에는 상세 정보 자동 업데이트 안 함
                return;
            }

            // 모든 버튼을 순회했는데 활성화된 버튼이 없으면 종료
            if (newIndex == startIndex && !abilityButtons[newIndex].interactable) return;

            // 다음 버튼으로 이동
            newIndex += isMovingRight ? 1 : -1;
        }
    }

    private void SelectPurchaseButton()
    {
        // 구매 버튼의 활성화 여부와 관계없이 포커스를 이동시킵니다.
        // 이를 통해 사용자는 시각적으로 현재 선택 위치를 알 수 있습니다.
        selectedButtonIndex = abilityButtons.Count;
        // Unity의 EventSystem을 사용하여 구매 버튼을 하이라이트 처리
        purchaseButton.Select();
    }

    // 능력 버튼 클릭 시 호출
    public void SelectAbility(int index)
    {
        // 선택 확정 처리
        confirmedSelectionIndex = index;
        UpdateButtonSelectionColors();

        // 이미 구매한 능력은 선택할 수 없습니다.
        if (index < abilitiesForSale.Count && abilitiesForSale[index].isPurchased)
        {
            ClearSelection(); // 구매한 아이템을 선택하면 상세정보 창을 비웁니다.
            return;
        }
        
        if (index >= abilitiesForSale.Count) return;

        currentlySelectedAbility = abilitiesForSale[index];

        // --- UI 업데이트 ---
        abilityNameText.text = currentlySelectedAbility.abilityName;
        abilityDescriptionText.text = currentlySelectedAbility.description;

        // 아이콘 업데이트
        if (abilityIconImage != null)
        {
            if (currentlySelectedAbility.icon != null)
            {
                abilityIconImage.sprite = currentlySelectedAbility.icon;
                abilityIconImage.enabled = true;
            }
            else
            {
                abilityIconImage.enabled = false;
            }
        }

        // 5개의 비용 텍스트 업데이트
        for (int i = 0; i < costTexts.Count; i++)
        {
            if (i < currentlySelectedAbility.costs.Count)
            {
                // 표시할 비용 정보가 있는 경우
                var cost = currentlySelectedAbility.costs[i];
                costTexts[i].text = $"X {cost.amount}";
            }
            else
            {
                // 표시할 비용 정보가 없는 경우, 텍스트를 비웁니다.
                costTexts[i].text = "-";
            }
        }

        // 구매 가능 여부에 따라 버튼 상태 업데이트
        UpdatePurchaseButtonState();
    }

    // 선택 초기화
    private void ClearSelection()
    {
        currentlySelectedAbility = null;
        confirmedSelectionIndex = -1;
        abilityNameText.text = "능력을 선택하세요";
        abilityDescriptionText.text = "능력에 대한 설명이 여기에 표시됩니다.";
        // 모든 비용 텍스트를 초기화합니다.
        foreach (var text in costTexts)
        {
            text.text = "-";
        }
        if (abilityIconImage != null) abilityIconImage.enabled = false;
        purchaseButton.interactable = false;
        RestoreAllButtonColors();
    }
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && gameObject.activeInHierarchy && selectedButtonIndex != -1)
        {
            abilityButtons[selectedButtonIndex].Select();
        }
    }

    private void UpdateButtonSelectionColors()
    {
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (i >= abilitiesForSale.Count || abilitiesForSale[i].isPurchased) continue;

            Image buttonImage = abilityButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = (i == confirmedSelectionIndex) ? selectedButtonColor : defaultButtonColor;
            }
        }
    }

    private void RestoreAllButtonColors()
    {
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (i >= abilitiesForSale.Count || abilitiesForSale[i].isPurchased) continue;

            Image buttonImage = abilityButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = defaultButtonColor;
            }
        }
    }

    // 구매 버튼 상태 업데이트
    private void UpdatePurchaseButtonState()
    {
        if (currentlySelectedAbility == null || ResourceManager.Instance == null)
        {
            purchaseButton.interactable = false;
            return;
        }

        // ResourceManager를 통해 자원이 충분한지 확인
        bool canAfford = CanAffordWithTemporaryInventory(currentlySelectedAbility.costs);

        // 구매 가능하고, 아직 구매하지 않은 능력일 때만 버튼 활성화
        purchaseButton.interactable = canAfford && !currentlySelectedAbility.isPurchased;
    }

    /// <summary>
    /// 창고와 임시 인벤토리를 모두 확인하여 구매 가능한지 확인합니다.
    /// </summary>
    private bool CanAffordWithTemporaryInventory(List<ResourceCost> costs)
    {
        if (ResourceManager.Instance == null || playerTemporaryInventory == null) return false;

        var tempResources = playerTemporaryInventory.GetAllTempResources();

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
            int tempSpent = playerTemporaryInventory.UseResource(cost.mineral, remainingCost);
            remainingCost -= tempSpent;

            // 2. 남은 비용이 있다면 창고에서 차감
            if (remainingCost > 0)
            {
                ResourceManager.Instance.UseResource(cost.mineral, remainingCost);
            }
        }
        return true;
    }

    // 구매 버튼 클릭 시 호출
    private void PurchaseAbility()
    {
        if (currentlySelectedAbility == null || ResourceManager.Instance == null || playerTemporaryInventory == null) return;

        // 창고와 임시 인벤토리를 모두 사용하여 자원을 소모합니다.
        if (TrySpendCombinedResources(currentlySelectedAbility.costs))
        {
            Debug.Log($"'{currentlySelectedAbility.abilityName}' 능력 구매 완료!");

            // 구매 상태를 true로 변경
            currentlySelectedAbility.isPurchased = true;

            // 구매한 능력에 해당하는 버튼을 비활성화하고 색상을 변경합니다. (확정된 인덱스 기준)
            if (confirmedSelectionIndex >= 0 && confirmedSelectionIndex < abilityButtons.Count)
            {
                SetButtonPurchased(abilityButtons[confirmedSelectionIndex]);
            }

            // TODO: 플레이어에게 능력 적용하는 로직 호출
            // 예: player.AddAbility(currentlySelectedAbility);

            // 구매 후, 선택 정보는 유지하되 구매 버튼만 비활성화합니다.
            UpdatePurchaseButtonState();

            // 구매 후 포커스를 재설정합니다.
            FindNextFocus();
        }
        else
        {
            Debug.Log("자원이 부족하여 구매할 수 없습니다.");
            // 여기에 "자원 부족" 알림 UI를 띄우면 더 좋습니다.
        }
    }

    /// <summary>
    /// 구매 후 다음에 포커스할 버튼을 찾아 이동합니다.
    /// </summary>
    private void FindNextFocus()
    {
        // 우선순위: 1. 첫 번째 줄 -> 2. 두 번째 줄
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (abilityButtons[i].interactable)
            {
                // 활성화된 첫 번째 버튼을 찾으면 그곳으로 이동하고 종료
                SelectButton(i);
                ClearSelection(); // 포커스 이동 후, 상세 정보 창을 초기화합니다.
                return;
            }
        }

        // 모든 버튼이 비활성화되었다면, 현재 위치(구매 버튼)에 그대로 머무릅니다.
        // selectedButtonIndex는 구매 버튼 인덱스(abilityButtons.Count)를 유지합니다.
        // 이렇게 하면 더 이상 키 입력으로 움직이지 않게 됩니다.
        Debug.Log("모든 능력을 구매했습니다. 포커스가 더 이상 이동하지 않습니다.");
    }

    private void SetButtonPurchased(Button button)
    {
        button.interactable = false;
        button.GetComponent<Image>().color = Color.gray; // 시각적으로 비활성화 표시
    }
}
