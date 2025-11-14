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
    [SerializeField] private float aimRange = 4f; // 스틱 방향으로 얼마나 떨어진 곳을 조준점으로 볼지

    private PlayerInput _playerInput;
    private InputAction _aimAction;
    private Transform _activeHand, _inactiveHand;
    private Transform _playerRoot; // 손 바꿈 기준점
    private bool _useRight = true;
    
    
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

        RotateHandToward(_activeHand, target);
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
            weapon.localRotation = Quaternion.identity;
        }
    }

    private void RotateHandToward(Transform handPivot, Vector3 targetPos)
    {
        if (!handPivot) return;

        Vector2 dir = targetPos - handPivot.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (!_useRight)
            angle = 180f - angle;

        Quaternion q = Quaternion.Euler(0f, 0f, angle);
        float t = rotateLerp * Time.deltaTime;
        handPivot.rotation = Quaternion.Lerp(handPivot.rotation, q, t);

        // SpriteRenderer 반전
        var sr = weapon.GetComponentInChildren<SpriteRenderer>();
        if (sr)
            sr.flipY = (Mathf.Cos(angle * Mathf.Deg2Rad) < 0f); // 오른쪽/왼쪽에 따라 반전
    }


}
