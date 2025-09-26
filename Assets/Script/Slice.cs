using UnityEngine;

public class Slice : MonoBehaviour
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
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<Enemy>().SliceStart();

        }
        if (collision.CompareTag("Ore"))
        {
            collision.GetComponent<Ore>().SliceStart();
        }
    }
}
