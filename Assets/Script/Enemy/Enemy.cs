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
        Slice.SetActive(true);
        Invoke("DestroyThis", 3f);
    }

    void DestroyThis()
    {
        Destroy(gameObject);
    }
}
