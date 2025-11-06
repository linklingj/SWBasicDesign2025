using System;
using UnityEngine;

public class Weapon : MonoBehaviour 
{
    [SerializeField] protected WeaponData data;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] private Animator animator;

    protected float nextFireTime;

    private static readonly int IsShooting = Animator.StringToHash("isShooting");
    
    public virtual void Init()
    {
        if (!firePoint) firePoint = transform;
        nextFireTime = 0f;
    }

    private void Update()
    {
        animator.SetBool(IsShooting, !CanFire());
    }

    public virtual void Fire()
    {
        if (!CanFire() || bulletPrefab == null) return;

        Vector3 spawnPos = firePoint.position;
        Vector3 direction = firePoint.right * transform.localScale.x;

        if (direction.sqrMagnitude <= Mathf.Epsilon) return;

        ShootBullet(spawnPos, direction);
        ScheduleNextShot();
    }

    protected virtual bool CanFire()
    {
        return Time.time >= nextFireTime;
    }

    protected virtual void ScheduleNextShot()
    {
        float cooldown = 0f;
        if (data != null && data.fireRate > 0f)
        {
            cooldown = 1f / data.fireRate;
        }

        nextFireTime = Time.time + cooldown;
    }

    protected virtual void ShootBullet(Vector3 pos, Vector3 dir)
    {
        if (bulletPrefab == null) return;

        Quaternion bulletRotation = Quaternion.identity;
        if (dir.sqrMagnitude > 0f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bulletRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        Transform bullet = null;
        if (ObjectPoolManager.Instance)
        {
            bullet = ObjectPoolManager.Instance.Get(bulletPrefab, pos, bulletRotation).transform;
        }

        if (bullet == null)
        {
            bullet = Instantiate(bulletPrefab, pos, bulletRotation).transform;
        }

        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent)
        {
            bulletComponent.Init(dir);
        }
    }
}
