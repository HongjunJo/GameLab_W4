using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] GameObject Slice;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SliceStart()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<EnemyBase>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
        Slice.SetActive(true);
        Invoke("DestroyThis", 3f);
    }

    void DestroyThis()
    {
        Destroy(gameObject);
    }
}
