using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Weapon : MonoBehaviour 
{
    [SerializeField] protected WeaponData data;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    protected int playerIdx; 
    
    protected float nextFireTime;
    
    protected int extraDamage = 0;
    protected float extraFireRate = 0f;
    protected BulletData bulletData;

    private static readonly int IsShooting = Animator.StringToHash("isShooting");
    
    public virtual void Init(WeaponData weaponData, int playerIndex)
    {
        data = weaponData;
        playerIdx = playerIndex;
        gameObject.name = weaponData.name;
        bulletData = weaponData.bulletData;
        
        if (!firePoint) firePoint = transform;
        else
        {
            firePoint.localPosition = data.firePosoffset;
        }
        nextFireTime = 0f;
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        ApplyWeaponData();
    }
    
    protected void ApplyWeaponData()
    {
        if (data == null) return;
        
        // 1) 탄 프리팹 / 머즐 플래시를 데이터 기준으로 덮어쓰기 (데이터에 있으면)
        if (data.bulletPrefab != null)
            bulletPrefab = data.bulletPrefab;

        if (data.muzzleFlashPrefab != null)
            muzzleFlashPrefab = data.muzzleFlashPrefab;

        // 2) 무기 스프라이트 교체
        if (spriteRenderer != null && data.weaponSprite != null)
            spriteRenderer.sprite = data.weaponSprite;

        // 3) 애니메이션 교체
        if (animator != null && data.animatorController != null)
            animator.runtimeAnimatorController = data.animatorController;
        
    }
    
    public void SetUpgrades(int damageIncrease, float fireRateIncrease)
    {
        extraDamage = damageIncrease;
        extraFireRate = fireRateIncrease;
    }

    private void Update()
    {
        animator.SetBool(IsShooting, !CanFire());
    }

    public virtual bool Fire()
    {
        if (!CanFire() || bulletPrefab == null) return false;

        Vector3 spawnPos = firePoint.position;
        Vector3 direction = firePoint.right * transform.localScale.x;

        if (direction.sqrMagnitude <= Mathf.Epsilon) return false;

        ShootBullet(spawnPos, direction);
        ScheduleNextShot();

        return true;
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
            cooldown = 1f / (data.fireRate + extraFireRate);
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
            bulletComponent.SetDamage(data.damage + extraDamage);
            bulletComponent.Init(pos, dir, bulletData, playerIdx);
        }
        Vector3 muzzlepos = pos + firePoint.right * -0.1f;
        // 탄환 생성 후
        if (muzzleFlashPrefab)
        {
            Transform flash = null;

            if (ObjectPoolManager.Instance)
                flash = ObjectPoolManager.Instance.Get(muzzleFlashPrefab, muzzlepos, bulletRotation).transform;
            else
                flash = Instantiate(muzzleFlashPrefab, muzzlepos, bulletRotation).transform;

            // 수명이 짧은 이펙트는 자동 Release 스크립트 붙여두면 됨
        }

        
        
    }
}
