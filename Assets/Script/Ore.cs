using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class Ore : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] GameObject[] OreBodies;
    [SerializeField] GameObject DropObject;
    [SerializeField] private float bodyRemainTime;
    [SerializeField] private float respawnTime;
    [SerializeField] private int maxHp = 3;
    [SerializeField] private int currentHp;
    private Vector3 SavedPostion;
    private Quaternion SavedRotaion;
    private Vector3[] SavedBodyPos;
    private Quaternion[] SavedBodyRotaion;
    void Start()
    {
        OreBodies = new GameObject[target.childCount];
        SavedBodyPos = new Vector3[OreBodies.Length];
        SavedBodyRotaion = new Quaternion[OreBodies.Length];
        currentHp = maxHp;
        SavedPostion = transform.position;
        SavedRotaion = transform.rotation;
        for (int i = 0; i < target.childCount; i++)
        {
            OreBodies[i] = target.GetChild(i).gameObject;
        }
        for (int i = 0; i < OreBodies.Length; i++)
        {
            SavedBodyPos[i] = OreBodies[i].transform.position;
            SavedBodyRotaion[i] = OreBodies[i].transform.rotation;
        }
    }
    void ResetOre()
    {
        currentHp = maxHp;
        transform.SetPositionAndRotation(SavedPostion, SavedRotaion);
        for (int i = 0; i < OreBodies.Length; i++)
        {
            OreBodies[i].SetActive(true);
            OreBodies[i].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            OreBodies[i].transform.SetPositionAndRotation(SavedBodyPos[i], SavedBodyRotaion[i]);
            OreBodies[i].SetActive(true);
        }
        GetComponent<CapsuleCollider2D>().enabled = true;
    }
    void DelOre()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
    }
    void DropOre()
    {
        Instantiate(DropObject, transform.position, Quaternion.identity);
    }
    public void SliceStart()
    {
        if (currentHp - 1 <= 0)
        {
            StartCoroutine(nameof(RespawnCoroutine));
            OreBodies[currentHp-1].SetActive(false);
            currentHp--;
        }
        else
        {
            OreBodies[currentHp-1].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            StartCoroutine(DelBody(OreBodies[currentHp-1]));
            currentHp--;
        }

    }
    private IEnumerator DelBody(GameObject _body)
    {
        yield return new WaitForSeconds(bodyRemainTime);
        _body.SetActive(false);
    }
    private IEnumerator RespawnCoroutine()
    {
        if (respawnTime > 0f)
        {
            DropOre();
            DelOre();
            yield return new WaitForSeconds(respawnTime);
            ResetOre();
            Debug.Log("Test");
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
