using UnityEngine;

public class Slash : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Ore"))
        {
            // collision.GetComponent<Slice>().SliceStart();

            RaycastHit2D hit = Physics2D.Linecast(transform.position, collision.transform.position, LayerMask.GetMask("Ground"));

            if (hit.collider == null) // 벽에 막히지 않음
            {
                collision.GetComponent<Slice>()?.SliceStart();
            }
        }
    }
}
