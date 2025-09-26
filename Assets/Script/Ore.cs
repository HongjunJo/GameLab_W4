using System.Collections;
using UnityEngine;

public class Ore : MonoBehaviour
{
    [SerializeField] Rigidbody2D[] OreBodies;
    [SerializeField] GameObject DropObject;
    [SerializeField] private float respawnTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SliceStart()
    {
        foreach (Rigidbody2D OreBody in OreBodies)
        {
            OreBody.simulated = true;
        }
        StartCoroutine(nameof(RespawnCoroutine));
        // Invoke(nameof(DestroyThis), 1f);
    }

    void DropOre()
    {
        Instantiate(DropObject, transform.position, Quaternion.identity);
    }

    void DestroyThis()
    {
        DropOre();
        Destroy(gameObject);
    }
    private IEnumerator RespawnCoroutine()
    {
        if (respawnTime > 0f)
        {
            DropOre();
            // 시각적 오브젝트와 콜라이더를 비활성화합니다.
            // gameObject.SetActive(false);
            // GetComponent<Collider2D>().enabled = false; // 상호작용을 막기 위해 콜라이더도 끔
            // 설정된 시간만큼 대기
            yield return new WaitForSeconds(respawnTime);
            Debug.Log("TEst");
            ResetResource();
        }

        else
        {
            gameObject.SetActive(false);
        }
    }
    private void ResetResource()
    {
        // currentHp = maxHp;
        // gameObject.SetActive(true);
        // GetComponent<Collider2D>().enabled = true;
    }
}
