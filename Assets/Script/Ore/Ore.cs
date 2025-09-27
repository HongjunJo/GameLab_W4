using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class Ore : MonoBehaviour
{
    [SerializeField] GameObject DropObject;
    public OreList oreList;
    [SerializeField] private int maxRandRange = 5;
    private int currentPool;
    public void DropOre()
    {
        currentPool = Random.Range(1, maxRandRange);
        for (int i = 0; i < currentPool; i++)
        {
            Instantiate(DropObject, transform.position + new Vector3(Random.Range(-maxRandRange*0.1f,maxRandRange*0.1f), Random.Range(0.1f,maxRandRange*0.1f), 0), Quaternion.identity);
        }
    }
}
