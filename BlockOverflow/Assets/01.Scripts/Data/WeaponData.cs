using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject {
    [Title("$name", "Weapon 에서 사용", TitleAlignments.Centered)]
    [LabelText("이름")] public string weaponName;
    [LabelText("데이지"), Min(0)] public float damage;
    [LabelText("연사 속도"), Min(0)] public float fireRate;
    [LabelText("무기 타입")] public WeaponType weaponType;
    [LabelText("사운드")] public AudioData shotSound;
    [LabelText("사운드 이름")] public string shotSoundName;
    public BulletData bulletData;
    
    
    [Title("외형 / 이펙트")]
    [PreviewField(Alignment = ObjectFieldAlignment.Left)]
    [LabelText("무기 스프라이트")] 
    public Sprite weaponSprite;

    [LabelText("탄 프리팹")]
    public GameObject bulletPrefab;
    public GameObject usedAmmoPrefab;

    [LabelText("머즐 플래시 프리팹")]
    public GameObject muzzleFlashPrefab;

    [LabelText("총구 위치 오프셋")] public Vector3 firePosoffset;

    [LabelText("애니메이션")]
    public RuntimeAnimatorController animatorController;
    
    
    

}
