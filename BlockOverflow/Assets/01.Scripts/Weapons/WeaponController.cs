using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour {
    [SerializeField] private Weapon weapon;
    [SerializeField] private WeaponType weaponType;
    private PlayerInput _playerInput;
    //private InputAction _shootAction;
    private Gamepad _gamepad;
    
    private CameraController _cameraController;

    private void Awake() {
        if (weapon == null) weapon = GetComponentInChildren<Weapon>();
        _playerInput = GetComponentInParent<PlayerInput>();
        _cameraController = FindFirstObjectByType<CameraController>();
        //weapon?.Init(0);
    }
    
    private void Start() {
        if (_playerInput != null)
        {
            _gamepad = _playerInput.GetDevice<Gamepad>();
        }
        if (_gamepad != null)
            Debug.Log($"{name} 에 할당된 Gamepad: {_gamepad.displayName}");
        else
            Debug.Log($"{name} 은 Gamepad 없이 생성되었습니다.");
    }

    private void Update() {
        if (weapon == null) return;
        if (Mouse.current != null && Mouse.current.leftButton.isPressed) {
            if (weapon.Fire())
                _cameraController?.ShakeCamera(0.05f, 0.03f, 20);
        }
        if (_gamepad != null)
        {
            float trigger = _gamepad.rightTrigger.ReadValue(); // 0 ~ 1
            // Debug.Log($"{name} RightTrigger: {trigger}");

            if (trigger >= 0.1f)
            {
                if (weapon.Fire()) // Fire 안에서 쿨타임 처리되어 있다고 가정
                    _cameraController.ShakeCamera(0.1f, 0.05f, 5);
            }
        }
        
    }

    public void SetWeapon(WeaponData newWeapon, int playerIdx) {
        weapon?.Init(newWeapon, playerIdx);
    }
    
    public void SetUpgrades(int damageIncrease, float fireRateIncrease) {
        weapon?.SetUpgrades(damageIncrease, fireRateIncrease);
    }
}
