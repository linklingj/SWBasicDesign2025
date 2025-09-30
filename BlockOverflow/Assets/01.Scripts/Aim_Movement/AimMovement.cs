using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;


public class AimMovement : MonoBehaviour
{
    [Header("오브젝트")] 
    [SerializeField] private Camera cam;
    [SerializeField] private Transform rightHandPivot; //오른손
    [SerializeField] private Transform leftHandPivot; //왼손
    [SerializeField] private Transform weapon;
    
    [Header("무기 위치")]
    [SerializeField] private Vector3 weaponLocalPos = new Vector3(0.25f, 0, 0);
    [SerializeField] private float weaponLocalAngle = 0f; //무기 기본 각도
    [SerializeField] private float leftHandAngleAdd = 180f; //왼손일 때 무기의 각도 추가
    
    
    [FormerlySerializedAs("stickDeadzone")]
    [Header("조이스틱")]
    [SerializeField, Range(0f, 1f)] private float stickDeadZone = 0.1f; 
    
    
    [Header("회전")]
    [SerializeField, Range(1f, 100f)] private float rotateLerp = 20f;
    [SerializeField, Range(0f, 1f)] private float sideSwitchDeadzone = 0.1f; //손 전환 데드존

    
    
    
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
        Vector3 targetPos;
        if (!TryGetAimTarget(out targetPos)) return;
        
        float dx = targetPos.x - transform.position.x;
        bool wantRight;
        if (dx < -sideSwitchDeadzone) wantRight = false;
        else if (dx > sideSwitchDeadzone) wantRight = true;
        else wantRight = _usingRightHand; //데드존 안이면 현재 손 유지
        
        
        if (wantRight != _usingRightHand) SetActiveHand(wantRight); //손 전환
        RotateHandToward(_activeHandPivot, targetPos);
        
        Debug.DrawLine(_activeHandPivot.position, targetPos, Color.yellow);
       
        
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
            if (!_usingRightHand)
            {
                offset.x *= -1; //왼손이면 x축 반전
            }
            weapon.localScale = weapon.localScale * -1f;
            
            weapon.localPosition = offset; // 무기 위치

            
    
            
            float ang = weaponLocalAngle + (!_usingRightHand ? leftHandAngleAdd : 0f); //왼손이면 각도 추가
            weapon.localRotation = Quaternion.Euler(0, 0, ang);// 무기 각도
            
            
            
        }
        
        //if (_inactiveHandPivot != null) _inactiveHandPivot.gameObject.SetActive(false); // 비활성화 손 비활성화
        //if (_activeHandPivot != null) _activeHandPivot.gameObject.SetActive(true); // 활성화 손 활성화
        
    }
    
    
    //마우스 or 스틱 좌표 획득 시도(실패 시 false 반환)
    private bool TryGetAimTarget(out Vector3 aimTarget)
    {
        aimTarget = default;

        if (Mouse.current != null)
        {
            Vector2 mp = Mouse.current.position.ReadValue();

            if (mp.x >= 0 && mp.x <= Screen.width && mp.y >= 0 && mp.y <= Screen.height)
            {
                Vector3 sp = new Vector3(mp.x, mp.y, 0f);
                aimTarget = cam.ScreenToWorldPoint(sp);
                aimTarget.z = 0f;
                return true;
            }
        }
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();

            if (stick.sqrMagnitude > stickDeadZone * stickDeadZone)
            {
                aimTarget = transform.position + (Vector3)stick;
                return true;
            }
        }
        
        
        return false;

    }
    
    
    
    private void RotateHandToward(Transform handPivot, Vector3 targetPos)
    {
        if (!handPivot) return;
        
        Vector2 dir = targetPos - handPivot.position;
        
        //arctan(x,y) -> 라디안 * 180/파이 -> 각도
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (!_usingRightHand) angle += 180f;
        float baseAngle = 0;
        //float baseAngle = _usingRightHand ? 0f : 180f;
        float relativeAngle = Mathf.DeltaAngle(baseAngle, angle); 
        relativeAngle = Mathf.Clamp(relativeAngle, -90, 90);
        angle = baseAngle + relativeAngle;
        Quaternion q = Quaternion.Euler(0f, 0f, angle);
        float t = rotateLerp * Time.deltaTime;
        handPivot.rotation = Quaternion.Lerp(handPivot.rotation, q, t);
        
        
    }
    
    
    

}


