using System;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;

public class Weapon : MonoBehaviour 
{
    [SerializeField] protected WeaponData data;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform firePoint;
    //[SerializeField] private Animator animator;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject usedAmmoPrefab;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("반동")]
    [SerializeField] private Transform recoilPivot;     // 흔들릴 기준
    [SerializeField] private float recoilAngle = 6f;    // 몇 도 정도 튕길지
    [SerializeField] private float recoilDuration = 0.05f;
    [SerializeField] private float recoilDistance = 0.06f;
    
    protected int playerIdx; 
    protected float nextFireTime;
    
    protected int extraDamage = 0;
    protected float extraFireRate = 0f;
    protected BulletData bulletData;

    // 반동 기준값
    private Vector3 originalRecoilEuler;      // 로컬 회전
    private Vector3 originalRecoilLocalPos;   // 로컬 위치

    //private static readonly int IsShooting = Animator.StringToHash("isShooting");
    
    public virtual void Init(WeaponData weaponData, int playerIndex)
    {
        data = weaponData;
        playerIdx = playerIndex;
        gameObject.name = weaponData.name;
        bulletData = weaponData.bulletData;
        
        if (!firePoint) 
            firePoint = transform;
        else
            firePoint.localPosition = data.firePosoffset;

        nextFireTime = 0f;
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (recoilPivot == null)
            recoilPivot = transform;

        // 🔥 반동 기준값 저장 (로컬 기준)
        originalRecoilEuler    = recoilPivot.localEulerAngles;
        originalRecoilLocalPos = recoilPivot.localPosition;

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
        
        if (data.usedAmmoPrefab != null)
            usedAmmoPrefab = data.usedAmmoPrefab;

        // 2) 무기 스프라이트 교체
        if (spriteRenderer != null && data.weaponSprite != null)
            spriteRenderer.sprite = data.weaponSprite;

        // 3) 애니메이션 교체
        //if (animator != null && data.animatorController != null)
        //    animator.runtimeAnimatorController = data.animatorController;
    }
    
    public void SetUpgrades(int damageIncrease, float fireRateIncrease)
    {
        extraDamage = damageIncrease;
        extraFireRate = fireRateIncrease;
    }

    private void Update()
    {
        //animator.SetBool(IsShooting, !CanFire());
    }

    public virtual bool Fire()
    {
        if (!CanFire() || bulletPrefab == null) return false;

        Vector3 spawnPos = firePoint.position;
        Vector3 direction = firePoint.right * transform.localScale.x;  // 월드 방향

        if (direction.sqrMagnitude <= Mathf.Epsilon) return false;

        ShootBullet(spawnPos, direction);
        ScheduleNextShot();

        // 🔥 반동 재생 (이제 로컬 기준이라 캐릭터 따라감)
        PlayRecoil(direction);

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

        // 머즐 플래시
        if (muzzleFlashPrefab)
        {
            Transform flash = null;

            if (ObjectPoolManager.Instance)
                flash = ObjectPoolManager.Instance.Get(muzzleFlashPrefab, muzzlepos, bulletRotation).transform;
            else
                flash = Instantiate(muzzleFlashPrefab, muzzlepos, bulletRotation).transform;

            // 수명이 짧은 이펙트는 자동 Release 스크립트 붙여두면 됨
        }

        // 탄피 배출
        if (usedAmmoPrefab)
        {
            Transform ammo = null;
            
            Vector3 ammoPos = transform.position;
            if (dir.sqrMagnitude > 0f)
            {
                ammoPos += -dir.normalized * 0.1f; 
            }
            
            // 회전 계산 (3D 기준)
            float xRot = (dir.x > 0) ? -75f : -105f;     
            Quaternion ammoRot = Quaternion.Euler(xRot, -90f, 0f);
            
            if (ObjectPoolManager.Instance)
                ammo = ObjectPoolManager.Instance.Get(usedAmmoPrefab, ammoPos, ammoRot).transform;
            else
                ammo = Instantiate(usedAmmoPrefab, ammoPos, ammoRot).transform;
        }
    }
    
    /// <summary>
    /// 총 반동 애니메이션 (로컬 기준으로만 움직이게 수정)
    /// </summary>
    protected virtual void PlayRecoil(Vector3 shotDirWorld)
    {
        if (!recoilPivot) return;

        // 이 타겟에 걸린 트윈들 제거
        recoilPivot.DOKill();

        // 기준값 복원 (로컬 기준)
        recoilPivot.localEulerAngles = originalRecoilEuler;
        recoilPivot.localPosition    = originalRecoilLocalPos;

        // === 1) 위치 반동 ===
        // 총의 +x가 총구 방향이라 가정 → -x로 살짝 뒤로 밀기
        Vector3 localBackDir   = new Vector3(-1f, 0f, 0f);  // 로컬 -x
        Vector3 startLocalPos  = originalRecoilLocalPos;
        Vector3 recoilLocalPos = startLocalPos + localBackDir * recoilDistance;

        // === 2) 회전 반동 (좌우에 따라 각도 부호 바꾸고 싶을 때) ===
        // shotDirWorld.x > 0 → 오른쪽 발사, < 0 → 왼쪽 발사
        float side = Mathf.Sign(shotDirWorld.x == 0 ? transform.right.x : shotDirWorld.x);
        float signedAngle = recoilAngle * side * -1f; // 방향 반대면 -1f 빼기/빼기 제거로 조절

        Sequence seq = DOTween.Sequence().SetTarget(recoilPivot);

        // 1단계: 뒤로 밀리면서 회전 (로컬 기준)
        seq.Append(
            recoilPivot.DOLocalMove(recoilLocalPos, recoilDuration)
                .SetEase(Ease.OutQuad)
        );
        seq.Join(
            recoilPivot.DOLocalRotate(
                originalRecoilEuler + new Vector3(0f, 0f, signedAngle),
                recoilDuration
            ).SetEase(Ease.OutQuad)
        );

        // 2단계: 다시 원래 자리로 복귀 (로컬 기준)
        seq.Append(
            recoilPivot.DOLocalMove(startLocalPos, recoilDuration)
                .SetEase(Ease.InQuad)
        );
        seq.Join(
            recoilPivot.DOLocalRotate(
                originalRecoilEuler,
                recoilDuration
            ).SetEase(Ease.InQuad)
        );
    }
    
    private void OnDisable()
    {
        if (recoilPivot != null)
            DOTween.Kill(recoilPivot);
    }

    private void OnDestroy()
    {
        if (recoilPivot != null)
            DOTween.Kill(recoilPivot);
    }
}
