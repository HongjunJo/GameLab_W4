using UnityEngine;

public class OxygenStone : MonoBehaviour
{
    [SerializeField] SafeZone safeZone;
    public void ActiveSafeZone()
    {
        safeZone.ActivateSafeZone();
    }
}
