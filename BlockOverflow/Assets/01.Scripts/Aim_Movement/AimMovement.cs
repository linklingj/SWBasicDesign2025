using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;


public class AimMovement : MonoBehaviour
{
    [Header("오브젝트")] 
    [SerializeField] private Camera cam;
    [SerializeField] private Transform rightHandPivot; //오른손 피벗
    [SerializeField] private Transform leftHandPivot; //왼손 피벗
    [SerializeField] private Transform weapon;
    
    [Header("무기 위치")]
    [SerializeField] private Vector3 weaponLocalPos = new Vector3(0.25f, 0, 0);
    [SerializeField] private float weaponLocalAngle = 0f; //무기 기본 각도
    [SerializeField] private float leftHandAngleAdd = 0f; //왼손일 때 무기의 각도 조절(오브젝트가 어색할 때 각도 조절용)
    
    
    [Header("조이스틱")]
    [SerializeField, Range(0f, 1f)] private float stickDeadZone = 0.1f;
    //스틱 데드라인 ( 저 간격에 들어온 값 무시)
    
    
    [Header("회전")]
    [SerializeField, Range(1f, 100f)] private float rotateLerp = 20f; //회전 속도(높을수록 빠르게)
    [FormerlySerializedAs("sideSwitchDeadzone")] [SerializeField, Range(0f, 1f)] private float sideSwitchDeadZone = 0.1f; //손 전환 데드존
    
    
    // 휴식 각도 & 리셋 시간
    [FormerlySerializedAs("RightRestAngle")]
    [Header("휴식 각도 & 리셋 시간")]
    [SerializeField] float rightRestAngle = 0f;
    [FormerlySerializedAs("LeftRestAngle")] [SerializeField] float leftRestAngle  = 180f;
    [FormerlySerializedAs("ResetDuration")] [SerializeField] float resetDuration  = 0.15f; // 0이면 즉시 스냅

    [Header("자연스러운 흔들림")]
    [SerializeField] private bool swayInactiveHand = true;          // 스웨이 On/Off
    [SerializeField] private float swayAmplitude = 1f;              // 흔들림 진폭(도)
    [SerializeField] private float swayFrequency = 0.8f;            // 흔들림 속도(Hz)
    [SerializeField, Range(0f, 1f)] private float swayFollow = 0.25f; // 따라가는 정도(보간 속도)
    [SerializeField, Range(0f,1f)] private float swayCenterFollow = 0.1f; // '센터'가 현재 각도를 따라가는 속도
    
    
    
    private Transform _activeHandPivot; //현재 사용중인 손
    private Transform _inactiveHandPivot; //현재 사용하지 않는 손
    private bool _usingRightHand = true; // 현재 오른손 사용중? 맞으면 ture, 왼손이면 false
    
    
    // 손 리셋용 코루틴 참조(중복 실행 방지)
    private Coroutine _resetCoRight;
    private Coroutine _resetCoLeft;

    private float _swayCenterRight; // 오른손 중심각(도)
    private float _swayCenterLeft;  // 왼손 중심각(도)

    
            
    
    
    void Start()
    {
        if (cam == null) cam = Camera.main;

        SetActiveHand(true, true);


    }

    void Update()
    {
        Vector3 targetPos; //조준 목표 좌표
        if (!TryGetAimTarget(out targetPos)) return; //조준 좌표 획득 실패 시 종료
        
        float dx = targetPos.x - transform.position.x; //플레이어 기준 조준 좌표의 x좌표 차이
        bool wantRight;//오른손 사용 희망?
        if (dx < -sideSwitchDeadZone) wantRight = false;
        else if (dx > sideSwitchDeadZone) wantRight = true; //양수면(오른쪽에 있으면) 오른손 사용 희망
        else wantRight = _usingRightHand; //데드존 안이면 현재 손 유지
        
        
        if (wantRight != _usingRightHand) SetActiveHand(wantRight); //손 전환
        RotateHandToward(_activeHandPivot, targetPos);
        
        Debug.DrawLine(_activeHandPivot.position, targetPos, Color.yellow); //조준선 확인용
        
        
        
        //비활성화 손 흔들림
        if (swayInactiveHand && _inactiveHandPivot)
        {
            //손별 센터 레퍼런스 선택
            ref float center = ref (_inactiveHandPivot == rightHandPivot ? ref _swayCenterRight : ref _swayCenterLeft);

            // 현재 각도를 읽고, 센터가 그쪽으로 서서히 따라가게(고정 기준 없이 현재 기준)
            float current = _inactiveHandPivot.eulerAngles.z;
            center = Mathf.LerpAngle(center, current, swayCenterFollow);

            //  센터 ± 사인파로 목표 각도 생성
            float wobble = Mathf.Sin(Time.time * Mathf.PI * 2f * swayFrequency) * swayAmplitude;
            float targetAngle = center + wobble;

            // 4) 부드럽게 회전
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
            _inactiveHandPivot.rotation = Quaternion.Slerp(_inactiveHandPivot.rotation, targetRot, swayFollow);
        }
        
       
        
    }
    


    // 손 전환
    private void SetActiveHand(bool useRight, bool forceReparent = false)
    {
        _usingRightHand = useRight; //현재 사용중인 손 설정
        _activeHandPivot = useRight ? rightHandPivot : leftHandPivot; //활성화 손 설정
        _inactiveHandPivot = useRight ? leftHandPivot : rightHandPivot; //비활성화 손 설정

// 무기 부모 교체(손 바뀜에 따라)
        if (weapon != null && (forceReparent || weapon.parent != _activeHandPivot)) //무기 있고, (강제 재부모화이거나, 무기 부모가 현재 활성 손이 아니면)
        {
            weapon.SetParent(_activeHandPivot,worldPositionStays: false);//활성화 한손을 부로모 할당 worldPositionStays: false -> 로컬포지션 유지

            var offset = weaponLocalPos;
            if (!_usingRightHand)
            {
                offset.x *= -1; //왼손이면 x축 반전
            }
            weapon.localScale = new Vector3(weapon.localScale.x * -1f, weapon.localScale.y, weapon.localScale.z); //무기를 반대손으로 넘길때 좌우 반전
            
            weapon.localPosition = offset; // 무기 위치

            
    
            
            float ang = weaponLocalAngle + (!_usingRightHand ? leftHandAngleAdd : 0f); //왼손이면 각도 추가
            weapon.localRotation = Quaternion.Euler(0, 0, ang);// 무기 각도
            
            if (_inactiveHandPivot == rightHandPivot)
                _swayCenterRight = rightHandPivot.eulerAngles.z;
            else
                _swayCenterLeft  = leftHandPivot.eulerAngles.z;
            
            
        }
        
        ResetHandToRest(_inactiveHandPivot);
        //if (_inactiveHandPivot != null) _inactiveHandPivot.gameObject.SetActive(false); // 비활성화 손 비활성화
        //if (_activeHandPivot != null) _activeHandPivot.gameObject.SetActive(true); // 활성화 손 활성화
        
    }
    
    
    //마우스 or 스틱 좌표 획득 시도(실패 시 false 반환)
    private bool TryGetAimTarget(out Vector3 aimTarget) 
    {
        aimTarget = default; //out이니까 일단 초기화

        if (Mouse.current != null) //마우스값이 존재하면
        {
            Vector2 mp = Mouse.current.position.ReadValue(); //좌표 획득

            if (mp.x >= 0 && mp.x <= Screen.width && mp.y >= 0 && mp.y <= Screen.height) //커서가 화면 안?
            {
                Vector3 sp = new Vector3(mp.x, mp.y, 0f); //스크린 좌표
                aimTarget = cam.ScreenToWorldPoint(sp);//월드 좌표로 변환
                aimTarget.z = 0f;
                return true;
            }
        }
        if (Gamepad.current != null) //마우스 x, 게임패드 존재
        {
            Vector2 stick = Gamepad.current.rightStick.ReadValue(); //오른쪽 스틱 값 획득

            if (stick.sqrMagnitude > stickDeadZone * stickDeadZone) //스틱입력값 제곱이 데드존 제곱보다 크면
            {
                aimTarget = transform.position + (Vector3)stick;
                return true;
            }
        }
        
        
        return false;

    }
    
    
    
    private void RotateHandToward(Transform handPivot, Vector3 targetPos) //목표 좌표쪽으로 회전
    {
        if (!handPivot) return;
        
        Vector2 dir = targetPos - handPivot.position; //피벗과 목표 좌표 사이의 상대 값
        
        //arctan(x,y) -> 라디안 * 180/파이 -> 각도
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (!_usingRightHand) angle += 180f; //왼손이면 180도 회전
        
        float relativeAngle = Mathf.DeltaAngle(0, angle); //각도 정규화
        relativeAngle = Mathf.Clamp(relativeAngle, -90, 90); //-90~90도 사이로 제한(손이 뒤로 돌아가는 것 방지)
        
        
        
        
        angle = relativeAngle; //제한된 각도를 설정
       
        Quaternion q = Quaternion.Euler(0f, 0f, angle); //회전
        float t = rotateLerp * Time.deltaTime; //보간 계수
        handPivot.rotation = Quaternion.Lerp(handPivot.rotation, q, t);
        
        
    }
    
    
    
    
    
    
    //손 전환 시 자연스럽게 위치 조정
    private void ResetHandToRest(Transform handPivot)
    {
        if (!handPivot) return; //손이 없으면 종료

        // 손 각도 설정
        float rest = (handPivot == rightHandPivot) ? rightRestAngle : leftRestAngle;

        // 이전에 돌던 코루틴 중단
        if (handPivot == rightHandPivot)
        {
            if (_resetCoRight != null) { StopCoroutine(_resetCoRight); _resetCoRight = null; }
            _resetCoRight =  StartCoroutine(SmoothReset(handPivot, rest, resetDuration));
        }
        else // left
        {
            if (_resetCoLeft  != null) { StopCoroutine(_resetCoLeft);  _resetCoLeft  = null; }
            _resetCoLeft = StartCoroutine(SmoothReset(handPivot, rest, resetDuration));
        }
    }
    
    //손을 부드럽게 휴식 각도로 되돌림
    private System.Collections.IEnumerator SmoothReset(Transform t, float restAngle, float duration)
    {
        if (!t) yield break;//손이 없으면 종료

        Quaternion from = t.rotation; //현재 회전
        Quaternion to   = Quaternion.Euler(0f, 0f, restAngle); //목표 회전

        if (duration <= 0f) //시간이 0이하면
        {
            t.rotation = to; // 즉시 회전
            yield break;
        }

        float elapsed = 0f; //경과 시간
        while (elapsed < duration) //시간이 다 될 때까지
        {
            elapsed += Time.deltaTime; //시간 누적
            float k = Mathf.Clamp01(elapsed / duration); 
            t.rotation = Quaternion.Slerp(from, to, k); // 회전
            yield return null; // 다음 프레임
        }
        t.rotation = to; //시간 다 됬을때 위치 안맞을수도 있으니.. 시간 다되면 목표 위치로 강제 이동
    }

    //씬 끝나면 코루틴 정리
    private void OnDisable()
    {
        StopAllCoroutines();
        _resetCoRight = _resetCoLeft = null;
    }

    
    
    
    
}


