using UnityEngine;

public class Ore : MonoBehaviour
{
    [SerializeField] Rigidbody2D[] OreBodies;
    [SerializeField] GameObject DropObject;
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
        Invoke("DestroyThis", 1f);
    }

    void DropOre()
    {
        Instantiate(DropObject,transform.position - new Vector3(0,0.5f,0),Quaternion.identity);
    }

    void DestroyThis()
    {
        DropOre();
        Destroy(gameObject);
    }
}
