using UnityEngine;

public class CharacterSlash : MonoBehaviour
{
    [SerializeField] DangerGaugeSystem dangerGaugeSystem;
    [SerializeField] LayerMask layerMask;
    [SerializeField] Transform target;
    [SerializeField] GameObject Sword;
    [SerializeField] GameObject Slash;
    //최대 검 길이
    [SerializeField] private float maxSwordLength;
    [SerializeField] private float sizeOffset;
    //현재 검 길이
    private float currentSwordLength = 1f;
    //기본 소모 공기량
    [SerializeField] private float airOffset;
    private Collider2D[] hitList = new Collider2D[10];
    [SerializeField] private float slashCoolTime = 0.15f;
    [SerializeField] private float slashTempTime = 0;
    //베는 중
    [SerializeField] private bool isSlash = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!isSlash)
            {
                GetComponent<Animator>().Play("Slash");
                SlashCheck();
            }

        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (currentSwordLength >= maxSwordLength)
                return;
            currentSwordLength += sizeOffset;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (currentSwordLength - sizeOffset <= 0)
            {
                return;
            }
            currentSwordLength -= sizeOffset;
        }
        ReSize();
        UseAir();
        chkCoolTime();
    }

    void chkCoolTime()
    {
        if (slashTempTime >= slashCoolTime)
        {
            isSlash = false;
        }
        else
        {
            slashTempTime += Time.deltaTime;
        }
    }

    void UseAir()
    {
        if (currentSwordLength <= 0)
            return;
        //검 길이 만큼 소모
        dangerGaugeSystem.DecreaseDanger(airOffset*currentSwordLength*Time.deltaTime);
    }

    void ReSize()
    {
        Sword.transform.localScale = new Vector3(1, 1 * currentSwordLength, 1);
        Slash.transform.localScale = new Vector3(1*currentSwordLength,1,1);
    }
    void SlashCheck()
    {
        slashTempTime = 0;
        isSlash = true;
        hitList = Physics2D.OverlapCircleAll(transform.position+ new Vector3(0.5f,0f,0f), 0.75f * currentSwordLength, layerMask);
        foreach (Collider2D hit in hitList)
        {
            if (hit.CompareTag("Enemy"))
            {
                hit.GetComponent<Enemy>().SliceStart();
                
            }
            if (hit.CompareTag("Ore"))
            {
                hit.GetComponent<Ore>().SliceStart();
            }
        
        }
        
    }
    void OnDrawGizmos() // 범위 그리기
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0.5f,0f,0f),0.75f * currentSwordLength);
    }

}
