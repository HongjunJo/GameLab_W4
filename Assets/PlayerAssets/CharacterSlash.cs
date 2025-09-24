using UnityEngine;

public class CharacterSlash : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;
    [SerializeField] Transform target;
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

    }

    void SlashCheck()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 1.5f,layerMask);
        if (hit)
        {
            Debug.Log(hit.name);
            if (hit.CompareTag("Enemy"))
            {
                hit.GetComponent<Enemy>().SliceStart();
            }
        }
    }

    void OnDrawGizmos() // 범위 그리기
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,1.5f);
    }

}
