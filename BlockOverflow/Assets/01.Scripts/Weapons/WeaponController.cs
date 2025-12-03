using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour {
    [SerializeField] private Weapon weapon;
    [SerializeField] private WeaponType weaponType;
    private PlayerInput _playerInput;
    //private InputAction _shootAction;
    private Gamepad _gamepad;
    
    private CameraController _cameraController;
    private PlayerHealth _playerHealth;

    private float finalRevengeMultiplier = 1;

    private void Awake() {
        if (weapon == null) weapon = GetComponentInChildren<Weapon>();
        _playerInput = GetComponentInParent<PlayerInput>();
        _cameraController = FindFirstObjectByType<CameraController>();
        _playerHealth = GetComponent<PlayerHealth>();
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
        
        float damageMultiplier = 1f;
        if (_playerHealth.CurrentHealth <= _playerHealth.MaxHealth * 0.3f)
        {
            damageMultiplier = finalRevengeMultiplier;
        }
        
        if (Mouse.current != null && Mouse.current.leftButton.isPressed) {
            if (weapon.Fire(damageMultiplier)) CameraShake();
        }
        if (_gamepad != null)
        {
            float trigger = _gamepad.rightTrigger.ReadValue(); // 0 ~ 1
            // Debug.Log($"{name} RightTrigger: {trigger}");

            if (trigger >= 0.1f)
            {
                if (weapon.Fire(damageMultiplier)) CameraShake();
            }
        }
        
    }

    void CameraShake()
    {
        switch (weaponType)
        {
            case WeaponType.Pistol:
                _cameraController?.ShakeCamera(0.03f, 0.02f, 15);
                break;
            case WeaponType.Sniper:
                _cameraController?.ShakeCamera(0.1f, 0.07f, 25);
                break;
            default:
                _cameraController?.ShakeCamera(0.05f, 0.05f, 20);
                break;
        }
    }

    public void SetWeapon(WeaponData newWeapon, int playerIdx) {
        weapon?.Init(newWeapon, playerIdx);
    }
    
    public void SetUpgrades(int damageIncrease, float fireRateIncrease, float bulletSizeMultiplier, float finalRevengeMult = 1f, bool reflectOnWalls = false) {
        weapon?.SetUpgrades(damageIncrease, fireRateIncrease, bulletSizeMultiplier, reflectOnWalls);
        finalRevengeMultiplier = finalRevengeMult;
    }
}
