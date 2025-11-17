using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour {
    [SerializeField] private Weapon weapon;

    private void Awake() {
        if (weapon == null) weapon = GetComponentInChildren<Weapon>();
        weapon?.Init();
    }

    private void Update() {
        if (weapon == null) return;
        if (Mouse.current != null && Mouse.current.leftButton.isPressed) {
            weapon.Fire();
        }
    }

    public void SetWeapon(Weapon newWeapon) {
        if (weapon == newWeapon) return;
        weapon = newWeapon;
        weapon?.Init();
    }
    
    public void SetUpgrades(int damageIncrease, float fireRateIncrease) {
        weapon?.SetUpgrades(damageIncrease, fireRateIncrease);
    }
}
