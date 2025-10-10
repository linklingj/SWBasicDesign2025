using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData")]
public class BulletData : ScriptableObject {
    [Title("$name", "Bullet 에서 사용", TitleAlignments.Centered)]
    [LabelText("이름")] public string name;
    [LabelText("사거리"), Min(0)] public float range;
    [LabelText("총알 속도"), Min(0)] public float bulletSpeed;

}
