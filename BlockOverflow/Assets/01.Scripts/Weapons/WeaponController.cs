using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour {
    [SerializeField] private Weapon weapon;
    private PlayerInput _playerInput;
    private InputAction _shootAction;

    private void Awake() {
        if (weapon == null) weapon = GetComponentInChildren<Weapon>();
        _playerInput = GetComponentInParent<PlayerInput>();
        _shootAction = _playerInput.actions?.FindAction("Attack");
        weapon?.Init();
    }

    private void Update() {
        if (weapon == null) return;
        // if (Mouse.current != null && Mouse.current.leftButton.isPressed) {
        //     weapon.Fire();
        // }
        
    }

    public void SetWeapon(Weapon newWeapon) {
        if (weapon == newWeapon) return;
        weapon = newWeapon;
        weapon?.Init();
    }
}
