using UnityEngine;

/// <summary>
/// 플레이어가 일정 범위 내에 들어오면 연결된 월드 스페이스 UI를 활성화하고,
/// 범위를 벗어나면 비활성화하는 스크립트.
/// </summary>
public class ProximityUI : MonoBehaviour
{
    [Header("UI 설정")]
    [Tooltip("플레이어가 가까이 왔을 때 활성화할 UI 오브젝트입니다.")]
    [SerializeField] private GameObject uiToShow;

    [Header("감지 설정")]
    [Tooltip("UI를 활성화할 감지 범위입니다.")]
    [SerializeField] private float detectionRadius = 5f;
    [Tooltip("플레이어를 식별하기 위한 태그입니다.")]
    [SerializeField] private string playerTag = "Player";

    [Header("상호작용 설정 (선택사항)")]
    [Tooltip("체크 시, 플레이어가 범위 내에서 아래 지정된 키를 눌러야 UI가 활성화됩니다.")]
    [SerializeField] private bool useInteractionKey = false;
    [Tooltip("UI를 활성화할 상호작용 키입니다.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.F;

    private Transform playerTransform;
    private bool isPlayerInRange = false;

    private void Start()
    {
        // 플레이어 오브젝트를 찾습니다.
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError($"'{playerTag}' 태그를 가진 플레이어를 찾을 수 없습니다. 플레이어 태그를 확인해주세요.", this);
            enabled = false; // 플레이어를 못찾으면 스크립트 비활성화
            return;
        }

        // 시작 시 UI를 숨깁니다.
        if (uiToShow != null)
        {
            uiToShow.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerTransform == null || uiToShow == null) return;

        // 플레이어와 이 오브젝트 사이의 거리를 계산하여 범위 내에 있는지 확인합니다.
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerInRange = distance <= detectionRadius;

        if (useInteractionKey)
        {
            // 상호작용 키 사용 모드
            // 플레이어가 범위 안에 있고 상호작용 키를 눌렀을 때 UI를 활성화합니다.
            if (isPlayerInRange && Input.GetKeyDown(interactionKey))
            {
                // UI의 다음 활성화 상태를 결정합니다.
                bool nextActiveState = !uiToShow.activeSelf;

                // UI를 토글합니다.
                uiToShow.SetActive(nextActiveState);

                // MovementLimiter를 사용하여 모든 게임 입력을 제어합니다.
                // UI가 활성화되면(true) 입력을 비활성화(false)하고,
                // UI가 비활성화되면(false) 입력을 활성화(true)합니다.
                MovementLimiter.Instance?.SetInputEnabled(!nextActiveState);
                Debug.Log($"ProximityUI: Game Input Enabled set to {!nextActiveState}");
            }

            // 상호작용 모드일 때, 플레이어가 범위를 벗어나면 UI를 강제로 닫고 이동을 활성화합니다.
            if (!isPlayerInRange && uiToShow.activeSelf)
            {
                uiToShow.SetActive(false);
                MovementLimiter.Instance?.SetInputEnabled(true);
                Debug.Log("ProximityUI: Player left range, UI closed and input enabled.");
            }
        }
        else
        {
            // 기존 근접 감지 모드
            // UI 상태가 변경되어야 할 때만 SetActive를 호출하여 최적화합니다.
            if (uiToShow.activeSelf != isPlayerInRange)
            {
                uiToShow.SetActive(isPlayerInRange);
            }
        }
    }
}