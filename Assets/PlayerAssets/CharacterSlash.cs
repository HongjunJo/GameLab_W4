using UnityEngine;
using System.Collections;

public class CharacterSlash : MonoBehaviour
{
    [SerializeField] DangerGaugeSystem dangerGaugeSystem;
    [SerializeField] LayerMask layerMask;
    [SerializeField] GameObject Sword;
    [SerializeField] GameObject Slash;
    private Collider2D[] hitList = new Collider2D[50];
    
    [SerializeField] private float currentSwordLength = 1f;
    [SerializeField] private float slashCoolTime = 0.15f; // 쿨타임

    [Header("Air")]
    [SerializeField] private float RequireAir;
    [SerializeField] private float reduceAir;
    [SerializeField] private float slashReduceAir;
    [SerializeField] private bool SlashUseAirMode = false;

    [Header("Sword Length")]
    [SerializeField] private float maxSwordLength;
    [SerializeField] private float sizeOffset;

    private bool isSlash = false;

    [SerializeField] SpriteRenderer spriteRenderer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!isSlash)
            {
                GetComponent<Animator>().Play("Slash");
                MovementLimiter.Instance.SetCanRotaion(false);
                SlashCheck();
                StartCoroutine(SlashCooldown()); // 쿨타임 코루틴 시작
            }
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (currentSwordLength < maxSwordLength)
                currentSwordLength += sizeOffset;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (currentSwordLength - sizeOffset > 0)
                currentSwordLength -= sizeOffset;
        }
        ReSize();
        UseAir();
    }

    IEnumerator SlashCooldown()
    {
        isSlash = true;
        yield return new WaitForSeconds(slashCoolTime);
        isSlash = false;
        MovementLimiter.Instance.SetCanRotaion(true);
    }

    void UseAir()
    {
        if (currentSwordLength <= 0) return;

        dangerGaugeSystem.DecreaseDanger(reduceAir * currentSwordLength * Time.deltaTime);
    }

    void ReSize()
    {
        if (dangerGaugeSystem.GetDangerRatio() <= RequireAir)
        {
            float airRatio = Mathf.InverseLerp(0f, RequireAir, dangerGaugeSystem.GetDangerRatio());
            Sword.transform.localScale = new Vector3(1, currentSwordLength * airRatio, 1);
            Slash.transform.localScale = new Vector3(1 * currentSwordLength * airRatio, 1, 1);
        }
        else
        {
            Sword.transform.localScale = new Vector3(1, currentSwordLength, 1);
            Slash.transform.localScale = new Vector3(0.5f, 0.7f, 1) + new Vector3(0.5f * currentSwordLength, 0.7f * currentSwordLength, 1);
        }
    }

    void SlashCheck()
    {
        // 베기 공기 소모 활성화시
        if (SlashUseAirMode)
            dangerGaugeSystem.DecreaseDanger(slashReduceAir);

        // 적 판정은 필요 시 다시 활성화
        /*
        foreach (Collider2D hit in hitList)
        {
            if (hit.CompareTag("Enemy"))
                hit.GetComponent<Enemy>().SliceStart();

            if (hit.CompareTag("Ore"))
                hit.GetComponent<Ore>().SliceStart();
        }
        */
    }
}
