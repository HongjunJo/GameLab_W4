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

    void Start()
    {
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
            }
        }

        purchaseButton.onClick.AddListener(PurchaseAbility);
        ClearSelection();
    }

    // 능력 버튼 클릭 시 호출
    public void SelectAbility(int index)
    {
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
        abilityNameText.text = "능력을 선택하세요";
        abilityDescriptionText.text = "능력에 대한 설명이 여기에 표시됩니다.";
        // 모든 비용 텍스트를 초기화합니다.
        foreach (var text in costTexts)
        {
            text.text = "-";
        }
        if (abilityIconImage != null) abilityIconImage.enabled = false;
        purchaseButton.interactable = false;
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
        bool canAfford = ResourceManager.Instance.CanAfford(currentlySelectedAbility.costs);
        purchaseButton.interactable = canAfford;
    }

    // 구매 버튼 클릭 시 호출
    private void PurchaseAbility()
    {
        if (currentlySelectedAbility == null || ResourceManager.Instance == null) return;

        // 자원이 충분한지 다시 한번 확인하고 비용 차감
        if (ResourceManager.Instance.TrySpendResources(currentlySelectedAbility.costs))
        {
            // TODO: 플레이어에게 능력 적용하는 로직 호출
            // 예: player.AddAbility(currentlySelectedAbility);
            Debug.Log($"'{currentlySelectedAbility.abilityName}' 능력 구매 완료!");

            // 구매 후 UI 초기화
            ClearSelection();
        }
        else
        {
            Debug.Log("자원이 부족하여 구매할 수 없습니다.");
            // 여기에 "자원 부족" 알림 UI를 띄우면 더 좋습니다.
        }
    }
}
