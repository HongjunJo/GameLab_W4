using UnityEngine;

public class CharacterSlash : MonoBehaviour
{
    [SerializeField] DangerGaugeSystem dangerGaugeSystem;
    [SerializeField] LayerMask layerMask;
    [SerializeField] GameObject Sword;
    [SerializeField] GameObject Slash;
    private Collider2D[] hitList = new Collider2D[50];
    
    //현재 검 길이
    private float currentSwordLength = 1f;
    //평타 쿨
    [SerializeField] private float slashCoolTime = 0.15f;
    private float slashTempTime = 0;
    [Header("Air")]
    //기본 소모 공기량
    [SerializeField] private float RequireAir;
    [SerializeField] private float reduceAir;
    [SerializeField] private float slashReduceAir;
    [SerializeField] private bool SlashUseAirMode = false;
    [Header("Sword Length")]
    //최대 검 길이
    [SerializeField] private float maxSwordLength;
    [SerializeField] private float sizeOffset;
    
    //베는 중
    private bool isSlash = false;

    [SerializeField] SpriteRenderer spriteRenderer;

    void Start()
    {
    }

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
        dangerGaugeSystem.DecreaseDanger(reduceAir * currentSwordLength * Time.deltaTime);

        
    }
    void ReSize()
    {
        if (dangerGaugeSystem.GetDangerRatio() <= RequireAir)
        {
            float airRatio = Mathf.InverseLerp(0f, RequireAir, dangerGaugeSystem.GetDangerRatio());
            Sword.transform.localScale = new Vector3(1, 1 * currentSwordLength * airRatio, 1);
            Slash.transform.localScale = new Vector3(1*currentSwordLength* airRatio,1,1);
        }
        else
        {
            Sword.transform.localScale = new Vector3(1, 1 * currentSwordLength, 1);
            Slash.transform.localScale = new Vector3(1*currentSwordLength,1,1);
        }
        
    }
    void SlashCheck()
    {
        slashTempTime = 0;
        isSlash = true;
        // hitList = Physics2D.OverlapCircleAll(transform.position+ new Vector3(0.5f,0f,0f), 0.75f * currentSwordLength, layerMask);
        // float width = spriteRenderer.bounds.size.x;   // 월드 단위 가로
        // float height = spriteRenderer.bounds.size.y;  // 월드 단위 세로
        // Vector3 boxCenter = transform.position + new Vector3(width / 2f, -0.1f, 0f) + new Vector3(0.3f,0f,0f);
        // Vector2 boxSize = new Vector2(width, height);

        // hitList = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, layerMask);
        //베기 공기 소모 활성화시
        if (SlashUseAirMode)
            dangerGaugeSystem.DecreaseDanger(slashReduceAir);
        // foreach (Collider2D hit in hitList)
        // {
        //     if (hit.CompareTag("Enemy"))
        //     {
        //         hit.GetComponent<Enemy>().SliceStart();

        //     }
        //     if (hit.CompareTag("Ore"))
        //     {
        //         hit.GetComponent<Ore>().SliceStart();
        //     }

        // }
    }
    // void OnDrawGizmos() // 범위 그리기
    // {
    //     // Gizmos.color = Color.red;
    //     // Gizmos.DrawWireSphere(transform.position + new Vector3(0.5f,0f,0f),0.75f * currentSwordLength);

    //     float width = spriteRenderer.bounds.size.x;   // 월드 단위 가로
    //     float height = spriteRenderer.bounds.size.y;  // 월드 단위 세로

    //     // 왼쪽 끝을 기준으로 중앙으로 이동
    //     Vector3 boxCenter = transform.position + new Vector3(width / 2f, -0.1f, 0f) + new Vector3(0.3f,0f,0f);
    //     Vector2 boxSize = new Vector2(width, height);

    //     // // 색상
    //     Gizmos.color = Color.red;
    //     // 박스 그리기
    //     Gizmos.DrawWireCube(boxCenter, boxSize);
    // }

}
