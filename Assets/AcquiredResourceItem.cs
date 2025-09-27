using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AcquiredResourceItem : MonoBehaviour
{
    [SerializeField] private Image mineralIcon;
    [SerializeField] private TextMeshProUGUI amountText;
    private Transform target; // 플레이어 Transform

    private void LateUpdate()
    {
        // 부모 오브젝트(플레이어)의 움직임이 모두 끝난 후 마지막에 처리
        if (target != null)
        {
            // 부모의 x 스케일이 -1(좌측)이 되면, 자식인 이 UI의 x 스케일을 -1로 만들어 뒤집힘을 상쇄
            transform.localScale = new Vector3(Mathf.Sign(target.localScale.x), 1, 1);
        }

        // 부모의 회전 값에 영향을 받지 않도록 항상 정면을 보도록 고정
        transform.localRotation = Quaternion.identity;
    }

    public void SetItem(MineralData mineral, int amount)
    {
        if (mineralIcon != null)
        {
            mineralIcon.color = mineral.mineralColor;
        }
        if (amountText != null)
        {
            amountText.text = $"+{amount}";
        }
    }

    /// <summary>
    /// 이 UI가 따라다닐 대상을 설정합니다. (보통 플레이어)
    /// </summary>
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }
}
