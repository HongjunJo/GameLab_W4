using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class Ore : MonoBehaviour
{
    [SerializeField] GameObject DropObject;
    public OreList oreList;
    public void DropOre()
    {
        Instantiate(DropObject, transform.position, Quaternion.identity);
    }
}
