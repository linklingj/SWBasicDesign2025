using UnityEngine;
using UnityEngine.InputSystem;

public class AimMovement : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform rightHandPivot;
    [SerializeField] private Transform leftHandPivot;
    [SerializeField] private Transform weapon; // 보통 이 스크립트 붙은 자기 자신

    [Header("설정")]
    [SerializeField, Range(0f, 1f)] private float stickDeadZone = 0.1f;
    [SerializeField, Range(1f, 100f)] private float rotateLerp = 20f;
    [SerializeField, Range(0f, 1f)] private float sideSwitchDeadZone = 0.1f;
    [SerializeField] private Vector3 weaponLocalPos = new Vector3(0.25f, 0, 0);
    [SerializeField] private float aimRange = 4f; // 스틱 방향으로 얼마나 떨어진 곳을 조준점으로 볼지
    
    
    // 휴식 각도 & 리셋 시간
    
    [Header("휴식 각도 & 리셋 시간")]
    [SerializeField] float rightRestAngle = 0f;
    [SerializeField] float leftRestAngle  = 360f;
    [SerializeField] float resetDuration  = 0.15f; // 0이면 즉시 스냅
    
    
    
    [Header("자연스러운 흔들림")]
    [SerializeField] private bool swayInactiveHand = true;          // 스웨이 On/Off
    [SerializeField] private float swayAmplitude = 1f;              // 흔들림 진폭(도)
    [SerializeField] private float swayFrequency = 0.8f;            // 흔들림 속도(Hz)
    [SerializeField, Range(0f, 1f)] private float swayFollow = 0.25f; // 따라가는 정도(보간 속도)
    [SerializeField, Range(0f,1f)] private float swayCenterFollow = 0.1f; // '센터'가 현재 각도를 따라가는 속도

    private PlayerInput _playerInput;
    private InputAction _aimAction;
    private Transform _activeHand, _inactiveHand;
    private Transform _playerRoot; // 손 바꿈 기준점
    private bool _useRight = true;
    
    
    private float _swayCenterRight; // 오른손 중심각(도)
    private float _swayCenterLeft;  // 왼손 중심각(도)
    
    private Coroutine _resetCoRight;
    private Coroutine _resetCoLeft;
    
    private void Awake()
    {
        if (!weapon) weapon = transform;

        _playerInput = GetComponentInParent<PlayerInput>();
        if (!_playerInput)
        {
            Debug.LogError($"{name} ❌ PlayerInput not found in parents");
            enabled = false; return;
        }

        _aimAction = _playerInput.actions?.FindAction("Aim");
        if (_aimAction == null)
        {
            Debug.LogError($"{name} ❌ 'Aim' action not found in {_playerInput.actions?.name}");
            enabled = false; return;
        }

        _playerRoot = _playerInput.transform;

        // 피벗 스케일 보호 (회전 뒤집힘 방지)
        if (rightHandPivot) rightHandPivot.localScale = Vector3.one;
        if (leftHandPivot)  leftHandPivot.localScale  = Vector3.one;

        SetActiveHand(true, force:true);
    }

    private void Update()
    {
        Vector2 aim = _aimAction.ReadValue<Vector2>();
        
        //비활성화 손 흔들림
        if (swayInactiveHand && _inactiveHand)
        {
            //손별 센터 레퍼런스 선택
            ref float center = ref (_inactiveHand == rightHandPivot ? ref _swayCenterRight : ref _swayCenterLeft);

            // 현재 각도를 읽고, 센터가 그쪽으로 서서히 따라가게(고정 기준 없이 현재 기준)
            float current = _inactiveHand.eulerAngles.z;
            center = Mathf.LerpAngle(center, current, swayCenterFollow);

            //  센터 ± 사인파로 목표 각도 생성
            float wobble = Mathf.Sin(Time.time * Mathf.PI * 2f * swayFrequency) * swayAmplitude;
            float targetAngle = center + wobble;

            // 4) 부드럽게 회전
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
            _inactiveHand.rotation = Quaternion.Slerp(_inactiveHand.rotation, targetRot, swayFollow);
        }
        if (aim.sqrMagnitude < stickDeadZone * stickDeadZone)
            return;

        // 현재 활성 손 기준의 타겟점(조금 멀리)
        Vector3 target = _activeHand.position + (Vector3)(aim.normalized * aimRange);

        // 손 바꿈: 캐릭터 루트 대비 조준점이 좌/우에 있는지로 결정
        float dx = target.x - _playerRoot.position.x;
        bool wantRight = dx > sideSwitchDeadZone ? true
                        : dx < -sideSwitchDeadZone ? false
                        : _useRight;

        if (wantRight != _useRight)
        {
            SetActiveHand(wantRight);
            // 손 바꾼 뒤에도 같은 타겟을 다시 계산(피벗 바뀌었으니)
            target = _activeHand.position + (Vector3)(aim.normalized * aimRange);
        }
        Debug.DrawLine(_activeHand.position, target, Color.yellow);

        RotateHandToward(_activeHand, target);
        
        
        //비활성화 손 흔들림
        if (swayInactiveHand && _inactiveHand)
        {
            //손별 센터 레퍼런스 선택
            ref float center = ref (_inactiveHand == rightHandPivot ? ref _swayCenterRight : ref _swayCenterLeft);

            // 현재 각도를 읽고, 센터가 그쪽으로 서서히 따라가게(고정 기준 없이 현재 기준)
            float current = _inactiveHand.eulerAngles.z;
            center = Mathf.LerpAngle(center, current, swayCenterFollow);

            //  센터 ± 사인파로 목표 각도 생성
            float wobble = Mathf.Sin(Time.time * Mathf.PI * 2f * swayFrequency) * swayAmplitude;
            float targetAngle = center + wobble;

            // 4) 부드럽게 회전
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
            _inactiveHand.rotation = Quaternion.Slerp(_inactiveHand.rotation, targetRot, swayFollow);
        }

    }

    private void SetActiveHand(bool useRight, bool force=false)
    {
        _useRight = useRight;
        _activeHand   = useRight ? rightHandPivot : leftHandPivot;
        _inactiveHand = useRight ? leftHandPivot  : rightHandPivot;

        if (!_activeHand)
        {
            Debug.LogError($"{name} ❌ Missing hand pivot (right/left)"); 
            return;
        }

        // 부모 스케일/회전 영향 최소화: 피벗은 (1,1,1) 유지
        _activeHand.localScale = Vector3.one;
        if (_inactiveHand) _inactiveHand.localScale = Vector3.one;

        // 무기는 활성 손 밑으로 붙이고 로컬 회전은 0으로 (오프셋이 따로 필요하면 이후 적용)
        if (weapon && (force || weapon.parent != _activeHand))
        {
            weapon.SetParent(_activeHand, false);
            var offset = weaponLocalPos;
            if (!useRight)
            {
                offset.x *= -1; //왼손이면 x축 반전
            }
            
            Vector3 scale = weapon.localScale;
            scale.x *= -1f;   // 오직 x축만 뒤집기
            weapon.localScale = scale;
            

            weapon.localPosition = offset; // 무기 위치
            weapon.localRotation = Quaternion.identity;
            
            if (_inactiveHand == rightHandPivot)
                _swayCenterRight = rightHandPivot.eulerAngles.z;
            else
                _swayCenterLeft  = leftHandPivot.eulerAngles.z;
                
        }
        
        ResetHandToRest(_inactiveHand);
    }

    private void RotateHandToward(Transform handPivot, Vector3 targetPos)
    {
        if (!handPivot) return;

        Vector2 dir = targetPos - handPivot.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (!_useRight)
            angle = 180f + angle;

        Quaternion q = Quaternion.Euler(0f, 0f, angle);
        float t = rotateLerp * Time.deltaTime;
        handPivot.rotation = Quaternion.Lerp(handPivot.rotation, q, t);

        // SpriteRenderer 반전
        var sr = weapon.GetComponentInChildren<SpriteRenderer>();
        if (sr)
            sr.flipY = (Mathf.Cos(angle * Mathf.Deg2Rad) < 0f); // 오른쪽/왼쪽에 따라 반전
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
