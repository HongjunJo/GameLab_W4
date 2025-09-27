using UnityEngine;

public class SetInitialAnimation : MonoBehaviour
{
    // 인스펙터 창에서 시작할 애니메이션 상태의 이름을 직접 입력받습니다.
    public string initialStateName;

    void Start()
    {
        // 이 스크립트가 붙어있는 게임 오브젝트의 Animator 컴포넌트를 가져옵니다.
        Animator animator = GetComponent<Animator>();

        // Animator 컴포넌트가 있고, 상태 이름이 비어있지 않은지 확인합니다.
        if (animator != null && !string.IsNullOrEmpty(initialStateName))
        {
            // 지정된 이름의 애니메이션 상태를 즉시 재생시킵니다.
            // 이렇게 하면 Entry에서 연결된 기본 상태를 무시하고 원하는 애니메이션부터 시작합니다.
            animator.Play(initialStateName);
        }
        else
        {
            Debug.LogWarning("Animator 컴포넌트가 없거나 initialStateName이 설정되지 않았습니다.", this.gameObject);
        }
    }
}