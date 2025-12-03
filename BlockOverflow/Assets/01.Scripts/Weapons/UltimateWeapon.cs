using UnityEngine;

public class UltimateWeapon : Weapon
{
    [Header("Ultimate Settings")]
    [SerializeField] private float ultimateSpeed = 25f;
    [SerializeField] private float ultimateRange = 60f;
    [SerializeField] private float ultimateDamage = 60f;

    public void SetFirePoint(Transform fp) => firePoint = fp;
    public override bool Fire(float damageMultiplier = 1f)
    {
        if (bulletPrefab == null || !CanFire()) return false;

        Vector3 pos = firePoint.position;
        Vector3 dir = firePoint.right;

        // 좌우 반전 대응
        if (transform.lossyScale.x < 0)
            dir = -dir;

        // 총 기본 기능 재사용 (recoil 등)
        ShootBullet(pos, dir);
        ScheduleNextShot();
        PlayRecoil(dir);

        Debug.Log($"🔥 Ultimate Fired! dmg={ultimateDamage}");

        return true;
    }

    protected override void ShootBullet(Vector3 pos, Vector3 dir, float damageMultiplier = 1f)
    {
        Quaternion rot = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        var go = Instantiate(bulletPrefab, pos, rot);
        Bullet b = go.GetComponent<Bullet>();
        if (!b) return;

        b.SetUltimate(true);
        b.SetDamage(ultimateDamage);

        // 🔥 BulletData 새로 만들어 적용
        BulletData data = new BulletData();
        data.bulletSpeed = ultimateSpeed;
        data.range = ultimateRange;

        b.Init(pos, dir, data, playerIdx);
    }
    
}