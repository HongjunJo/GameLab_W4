using System.Collections;
using System.ComponentModel;
using UnityEngine;

[
    RequireComponent(typeof(CapsuleCollider2D))
]
// 적/오브젝트가 "베이기"(슬라이스) 당할 때 파츠별로 분리, 리스폰/삭제를 관리하는 스크립트
public class Slice : MonoBehaviour
{
    [Header("Enemy Death Settings")]
    [SerializeField] private bool destroyOnDeath = false; // true면 적이 죽을 때 오브젝트 삭제
    [SerializeField] Transform target; // 파츠(자식 오브젝트)들의 부모 Transform
    [SerializeField] GameObject[] TargetBodies; // 파츠 오브젝트 배열
    [SerializeField] private float bodyRemainTime = 1f; // 파츠가 떨어진 뒤 비활성화까지 대기 시간
    [SerializeField] private float respawnTime = 1f; // 리스폰 대기 시간
    [SerializeField] private int maxHp; // 파츠 개수(최대 HP)
    [SerializeField] private int currentHp; // 현재 남은 파츠(HP)
    private Vector3 SavedPostion; // 오브젝트 원래 위치
    private Quaternion SavedRotaion; // 오브젝트 원래 회전s
    private Vector3[] SavedBodyPos; // 각 파츠 원래 위치
    private Quaternion[] SavedBodyRotaion; // 각 파츠 원래 회전
    // 시작 시 파츠 정보 및 위치/회전 저장
    void Start()
    {
        maxHp = target.childCount;
        TargetBodies = new GameObject[target.childCount];
        SavedBodyPos = new Vector3[TargetBodies.Length];
        SavedBodyRotaion = new Quaternion[TargetBodies.Length];
        currentHp = maxHp;
        SavedPostion = transform.position;
        SavedRotaion = transform.rotation;
        // 파츠 오브젝트 배열 및 위치/회전 저장
        for (int i = 0; i < target.childCount; i++)
        {
            TargetBodies[i] = target.GetChild(i).gameObject;
        }
        for (int i = 0; i < TargetBodies.Length; i++)
        {
            SavedBodyPos[i] = TargetBodies[i].transform.position;
            SavedBodyRotaion[i] = TargetBodies[i].transform.rotation;
        }
    }
    // 오브젝트/파츠를 원래 상태로 복구
    void ResetTarget()
    {
        currentHp = maxHp;
        transform.SetPositionAndRotation(SavedPostion, SavedRotaion);
        for (int i = 0; i < TargetBodies.Length; i++)
        {
            // 파츠 활성화 및 위치/회전/물리 초기화
            TargetBodies[i].SetActive(true);
            TargetBodies[i].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            TargetBodies[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            TargetBodies[i].transform.SetPositionAndRotation(SavedBodyPos[i], SavedBodyRotaion[i]);
            TargetBodies[i].SetActive(true);
        }
        GetComponent<CapsuleCollider2D>().enabled = true;
        if (GetComponent<EnemyBase>() != null)
        {
            GetComponent<Rigidbody2D>().simulated = true;
        }
    }
    // Collider 비활성화(공격/충돌 안 받게)
    void DelTarget()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
    }
    
    // 외부에서 호출: 적이 "베이기" 당할 때 파츠 분리/비활성화/리스폰 처리
    public void SliceStart()
    {
        if (currentHp - 1 <= 0)
        {
            // 마지막 파츠가 잘리면 리스폰/삭제 코루틴 실행
            StartCoroutine(nameof(RespawnCoroutine));
            TargetBodies[currentHp - 1].SetActive(false);
            currentHp--;
        }
        else
        {
            // 파츠 Rigidbody2D를 Dynamic으로 바꾸고 일정 시간 후 비활성화
            TargetBodies[currentHp - 1].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            TargetBodies[currentHp - 1].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            StartCoroutine(DelBody(TargetBodies[currentHp - 1]));
            currentHp--;
        }

    }
    // 파츠가 떨어진 뒤 일정 시간 후 비활성화
    private IEnumerator DelBody(GameObject _body)
    {
        // if (GetComponent<EnemyBase>() != null && currentHp == 2)
        // {
        //     GetComponent<Rigidbody2D>().simulated = false;
        // }
        yield return new WaitForSeconds(bodyRemainTime);
        _body.SetActive(false);
        // if (GetComponent<EnemyBase>() != null && currentHp == 1)
        // {
        //     TargetBodies[currentHp - 1].SetActive(false);
        // }
            
    }
    // 마지막 파츠가 잘렸을 때 리스폰/삭제/특수효과 처리
    private IEnumerator RespawnCoroutine()
    {
        // EnemyBase+destroyOnDeath면 오브젝트 삭제
        if (GetComponent<EnemyBase>() != null && destroyOnDeath)
        {
            Destroy(gameObject);
            yield break;
        }
        if (respawnTime > 0f)
        {
            // Ore면 드랍, 일정 시간 후 리셋
            if (GetComponent<Ore>() != null)
                GetComponent<Ore>().DropOre();

            DelTarget();
            yield return new WaitForSeconds(respawnTime);
            ResetTarget();
        }
        else
        {
            // 리스폰 없이 바로 비활성화/특수효과
            DelTarget();
            if (GetComponent<OxygenStone>() != null)
                GetComponent<OxygenStone>().ActiveSafeZone();
        }
    }
}
