using UnityEngine;

// 스크립트의 역할을 더 명확하게 알 수 있도록 이름을 변경하는 것을 추천합니다.
public class UIRotationAndVisibility : MonoBehaviour
{
    [Header("UI 표시/숨김 설정")]
    [Tooltip("표시하거나 숨길 UI 게임 오브젝트를 여기에 연결하세요.")]
    public GameObject inventoryPanel; // 실제 켜고 끌 UI 패널 (예: 인벤토리의 배경 이미지)

    [Tooltip("UI를 활성화할 키입니다.")]
    public KeyCode activationKey = KeyCode.C; // 인벤토리(C) 키를 기본값으로 설정

    [Tooltip("UI가 나타나기까지 키를 누르고 있어야 하는 시간입니다.")]
    public float holdDuration = 0.5f; // 0.5초 누르고 있으면 UI가 켜지도록 설정


    [Header("UI 방향 고정 설정")]
    private Quaternion initialRotation;
    private Vector3 initialScale;
    private float holdTimer = 0f;

    [SerializeField] Transform target;

    void Start()
    {
        // 1. 방향 고정을 위한 초기값 저장
        initialRotation = transform.rotation;
        initialScale = transform.localScale;

        // 2. 시작할 때 UI는 항상 꺼진 상태로 만듦
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
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
        // 4. 방향 및 크기 고정
        if (Mathf.Sign(target.localScale.x) == -1)
        {
            transform.localScale = new Vector3(-1, 1, 1); // 부모의 스케일 영향을 제거
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1); // 부모의 스케일 영향을 제거
        }

        // 부모 오브젝트의 움직임이 모두 끝난 후 마지막에 처리
        transform.localRotation = Quaternion.identity; // 부모의 회전 영향을 제거);
        // transform.localScale = new Vector3(1, 1, 1); // 부모의 스케일 영향을 제거
    }

    // UI를 켜는 함수
    private void ShowInventory()
    {
        if (inventoryPanel != null && !inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
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
}