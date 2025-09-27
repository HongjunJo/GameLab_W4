using UnityEngine;

public class OxygenStone : MonoBehaviour
{
    [SerializeField] SafeZone safeZone;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActiveSafeZone()
    {
        safeZone.ActivateSafeZone();
    }
}
