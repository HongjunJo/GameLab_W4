using UnityEngine;

public class DropOreChk : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (collision.gameObject.CompareTag("DropOre"))
    //     {
    //         Debug.Log(collision.gameObject.GetComponent<DropOre>().currentOre);
    //         Debug.Log("Test");
    //     }
    // }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DropOre"))
        {
            Debug.Log(collision.gameObject.GetComponent<DropOre>().currentOre);
            Destroy(collision.gameObject);
        }
    }
}
