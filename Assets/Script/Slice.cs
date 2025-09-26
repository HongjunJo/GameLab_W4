using System.Collections;
using System.ComponentModel;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class Slice : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] GameObject[] TargetBodies;
    [SerializeField] private float bodyRemainTime = 1f;
    [SerializeField] private float respawnTime = 1f;
    [SerializeField] private int maxHp;
    [SerializeField] private int currentHp;
    private Vector3 SavedPostion;
    private Quaternion SavedRotaion;
    private Vector3[] SavedBodyPos;
    private Quaternion[] SavedBodyRotaion;
    void Start()
    {
        maxHp = target.childCount;
        TargetBodies = new GameObject[target.childCount];
        SavedBodyPos = new Vector3[TargetBodies.Length];
        SavedBodyRotaion = new Quaternion[TargetBodies.Length];
        currentHp = maxHp;
        SavedPostion = transform.position;
        SavedRotaion = transform.rotation;
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
    void ResetTarget()
    {
        currentHp = maxHp;
        transform.SetPositionAndRotation(SavedPostion, SavedRotaion);
        for (int i = 0; i < TargetBodies.Length; i++)
        {
            TargetBodies[i].SetActive(true);
            TargetBodies[i].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            TargetBodies[i].transform.SetPositionAndRotation(SavedBodyPos[i], SavedBodyRotaion[i]);
            TargetBodies[i].SetActive(true);
        }
        GetComponent<CapsuleCollider2D>().enabled = true;
    }
    void DelTarget()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
    }
    
    public void SliceStart()
    {
        if (GetComponent<EnemyBase>() != null && currentHp == 1)
        return;
        if (currentHp - 1 <= 0)
        {
            StartCoroutine(nameof(RespawnCoroutine));
            TargetBodies[currentHp - 1].SetActive(false);
            currentHp--;
        }
        else
        {
            TargetBodies[currentHp - 1].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            StartCoroutine(DelBody(TargetBodies[currentHp - 1]));
            currentHp--;
        }

    }
    private IEnumerator DelBody(GameObject _body)
    {
        yield return new WaitForSeconds(bodyRemainTime);
        _body.SetActive(false);
        if (GetComponent<EnemyBase>() != null && currentHp == 1)
            gameObject.SetActive(false);
    }
    private IEnumerator RespawnCoroutine()
    {
        if (respawnTime > 0f)
        {
            if (GetComponent<Ore>() != null)
                GetComponent<Ore>().DropOre();
            DelTarget();
            yield return new WaitForSeconds(respawnTime);
            ResetTarget();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
