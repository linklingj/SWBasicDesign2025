using UnityEngine;
using UnityEngine.InputSystem;


public class AimMovement : MonoBehaviour
{
    [Header("오브젝트")] 
    [SerializeField] private Camera cam;
    [SerializeField] private Transform rightHandPivot; //오른손
    [SerializeField] private Transform leftHandPivot; //왼손
    [SerializeField] private Transform weapon;
    
    [Header("무기 위치")]
    [SerializeField] private Vector3 weaponLocalPos = new Vector3(0.25f, 0, 0);
    [SerializeField] private float weaponLocalAngle = 0f;
    
    [Header("조이스틱")]
    [SerializeField] private string xAxis = "Horizontal"; 
    [SerializeField] private string yAxis = "Vertical";  
    [SerializeField, Range(0f, 1f)] private float stickDeadzone = 0.2f; 
    
    
    [Header("회전")]
    [SerializeField] private bool useSmooth = true;
    [SerializeField, Range(1f, 100f)] private float rotateLerp = 20f;
    
    
    
    private Transform _activeHandPivot; //현재 사용중인 손
    private Transform _inactiveHandPivot; //현재 사용하지 않는 손
    private bool _usingRightHand = true; // 현재 오른손 사용중?
    
    
    
    
    void Start()
    {
        if (cam == null) cam = Camera.main;

        SetActiveHand(true, true);


    }

    // Update is called once per frame
    void Update()
    {
        Vector3? mouseTarget = GetMouseWorldPoint();
        
        Vector2 stick = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;

        bool hasStick = stick.sqrMagnitude > stickDeadzone * stickDeadzone;



        Vector3 targetPos;
        if (mouseTarget.HasValue) targetPos = mouseTarget.Value;
        else if (hasStick) targetPos = transform.position + (Vector3)stick;
        else return;
        
        
        bool useRightHand = (targetPos.x - transform.position.x >= 0); //손 선택 (오른손이면 ture)
        
        if (useRightHand != _usingRightHand) SetActiveHand(useRightHand); // 현재 손이랑 상태가 다르면 상태 업데이트
        
        
 
    }




// 손 전환
    private void SetActiveHand(bool useRight, bool forceReparent = false)
    {
        _usingRightHand = useRight;
        _activeHandPivot = useRight ? rightHandPivot : leftHandPivot;
        _inactiveHandPivot = useRight ? leftHandPivot : rightHandPivot;

// 무기 부모 교체(손 바뀜에 따라)
        if (weapon != null && (forceReparent || weapon.parent != _activeHandPivot)) //무기 있고, (강제 재부모화이거나, 무기 부모가 현재 활성 손이 아니면)
        {
            weapon.SetParent(_activeHandPivot,worldPositionStays: false);//활성화 한손을 부로모 할당 worldPositionStays: false -> 로컬포지션 유지

            var offset = weaponLocalPos;
            if (!_usingRightHand) offset.x *= -1; //왼손이면 x축 반전
            weapon.localPosition = offset; // 무기 위치
            
            weapon.localRotation = Quaternion.Euler(0, 0, weaponLocalAngle);// 무기 각도
            
        }
        
        if (_inactiveHandPivot != null) _inactiveHandPivot.gameObject.gameObject.SetActive(false); // 비활성화 손 비활성화
        if (_activeHandPivot != null) _activeHandPivot.gameObject.SetActive(true); // 활성화 손 활성화
        
    }
    
    // 마우스 월드 좌표 얻기
    private Vector3? GetMouseWorldPoint()
    {
        if (cam == null) return null; //카메라 없으면 널
        
        Vector2 mp = Mouse.current.position.ReadValue();
        
        if (mp.x < 0 || mp.x > Screen.width || mp.y < 0 || mp.y > Screen.height) return null; //화면 밖이면 널


        Vector3 sp = new Vector3(mp.x, mp.y, 0f);
        Vector3 wp = cam.ScreenToWorldPoint(sp);//마우스 포인터 월드 좌표
        wp.z = 0; //z축 0으로 고정
        return wp;



    }

}


