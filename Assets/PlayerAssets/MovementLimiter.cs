using UnityEngine;

public class MovementLimiter : MonoBehaviour
{
    public static MovementLimiter Instance;

    [SerializeField] public bool _initialCharacterCanMove = true;
    public bool CharacterCanMove;
    public bool CharacterCanRotate = true;

    private void OnEnable()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CharacterCanMove = _initialCharacterCanMove;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 캐릭터의 이동 가능 상태를 설정합니다.
    /// </summary>
    /// <param name="isEnabled">입력 활성화 여부</param>
    public void SetInputEnabled(bool isEnabled)
    {
        CharacterCanMove = isEnabled;
        CharacterCanRotate = isEnabled;
    }

    
    /// <summary>
    /// 캐릭터의 회전 가능 상태를 설정합니다.
    /// </summary>
    /// <param name="canRotaion">이동 가능 여부</param>
    public void SetCanRotaion(bool canRotaion)
    {
        CharacterCanRotate = canRotaion;
    }
}
