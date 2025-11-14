using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject {
    [Title("$name", "Weapon 에서 사용", TitleAlignments.Centered)]
    [LabelText("이름")] public string weaponName;
    [LabelText("데이지"), Min(0)] public float damage;
    [LabelText("연사 속도"), Min(0)] public float fireRate;

}
