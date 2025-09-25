using UnityEngine;

public class CharacterSlash : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;
    [SerializeField] Transform target;
    [SerializeField] GameObject Sword;
    [SerializeField] GameObject Slash;
    private float size = 1f;
     // NonAlloc용 고정 배열 (최대 10명까지 타격 가능)
    private Collider2D[] hitList = new Collider2D[10];
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            GetComponent<Animator>().Play("Slash");
            SlashCheck();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            size += 1;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (size - 1 <= 0)
            {
                return;
            }
            size -= 1;
        }
        ReSize();
    }

    void ReSize()
    {
        Sword.transform.localScale = new Vector3(1, 1 * size, 1);
        Slash.transform.localScale = new Vector3(1*size,1,1);
    }
    void SlashCheck()
    {
        hitList = Physics2D.OverlapCircleAll(transform.position+ new Vector3(0.5f,0f,0f), 0.75f * size, layerMask);
        foreach (Collider2D hit in hitList)
        {
            if (hit.CompareTag("Enemy"))
            {
                hit.GetComponent<Enemy>().SliceStart();
            }
            if (hit.CompareTag("Ore"))
            {
                Debug.Log("Test");
                hit.GetComponent<Ore>().SliceStart();
            }
        }
    }
    void OnDrawGizmos() // 범위 그리기
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0.5f,0f,0f),0.75f * size);
    }

}
