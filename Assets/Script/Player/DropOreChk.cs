using UnityEngine;

public class DropOreChk : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DropOre"))
        {
            Debug.Log(collision.gameObject.GetComponent<DropOre>().currentOre);
            Destroy(collision.gameObject);
        }
    }
}
