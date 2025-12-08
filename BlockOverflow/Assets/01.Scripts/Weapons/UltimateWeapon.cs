using UnityEngine;
using DG.Tweening;

public class UltimateWeapon : Weapon
{
    [Header("Ultimate Settings")]
    [SerializeField] private float ultimateSpeed = 25f;
    [SerializeField] private float ultimateRange = 60f;
    [SerializeField] private float ultimateDamage = 60f;
    private float totalUltimateDamage;
    
    
    [Header("Ultimate Visual")]
    [SerializeField] private GameObject normalWeaponVisual;   // 기본 무기 스프라이트 오브젝트
    [SerializeField] private GameObject ultiWeaponVisual;     // ulti 무기
    [SerializeField] private ParticleSystem chargeFx;         // 에너지 차징

    [SerializeField] private float popScale     = 0.4f;       
    [SerializeField] private float popDuration  = 1f;      
    [SerializeField] private float chargeTime   = 0.3f;       // 차지 연출 시간

    public void SetFirePoint(Transform fp) => firePoint = fp;
    private bool isAnimating = false;
    private Vector3 chargepos = new Vector3(-3, 0, 0);
    
    private void Awake()
    {
        // 궁극기 비주얼 기본은 꺼두기
        if (ultiWeaponVisual != null)
            ultiWeaponVisual.SetActive(false);
        chargeFx.transform.position = firePoint.position + chargepos;
        totalUltimateDamage = ultimateDamage;
    }
    
    public void SetUltDamageAdd(float add)
    {
        totalUltimateDamage = ultimateDamage + add;
    }
    
    public override bool Fire(float damageMultiplier = 1f)
    {
        if (isAnimating) return false;
        if (bulletPrefab == null || !CanFire()) return false;

        Vector3 pos = firePoint.position;
        Vector3 dir = firePoint.right;

        // 좌우 반전 대응
        if (transform.lossyScale.x < 0)
            dir = -dir;
        
        StartUltimateSequence(pos, dir);
        return true;
    }
    
    
    private void StartUltimateSequence(Vector3 pos, Vector3 dir)
    {
        isAnimating = true;

        // 기존 무기 비주얼 숨기기
        if (normalWeaponVisual != null)
            normalWeaponVisual.SetActive(false);

        // 궁극기 오브젝트 없으면 그냥 바로 쏘고 끝
        if (ultiWeaponVisual == null)
        {
            DoUltimateShot(pos, dir);
            isAnimating = false;
            if (normalWeaponVisual != null)
                normalWeaponVisual.SetActive(true);
            return;
        }

        Transform t = ultiWeaponVisual.transform;
        ultiWeaponVisual.SetActive(true);
        t.DOKill();
        t.localScale = Vector3.zero;

        // DOTween 시퀀스로 연출
        Sequence seq = DOTween.Sequence().SetTarget(t);

        
        seq.Append(
            t.DOScale(popScale, popDuration)
                .SetEase(Ease.OutBack)
        );

        
        seq.Append(
            t.DOScale(0.35f, 0.3f)
                .SetEase(Ease.OutQuad)
        );
        
        AudioPlayer.Instance.Play("Ulti_Charge");
        
        seq.AppendCallback(() =>
        {
            if (chargeFx != null)
                chargeFx.Play();
                
        });

        if (chargeTime > 0f)
            seq.AppendInterval(chargeTime);

        // 4) 여기서 실제 궁극기 발사 (기존 Fire 로직 추출)
        seq.AppendCallback(() =>
        {
            DoUltimateShot(pos, dir);
        });

        // 5) 연출 끝 정리
        seq.AppendCallback(() =>
        {
            if (chargeFx != null)
                chargeFx.Stop();

            if (ultiWeaponVisual != null)
                ultiWeaponVisual.SetActive(false);

            if (normalWeaponVisual != null)
                normalWeaponVisual.SetActive(true);

            isAnimating = false;
        });


    }


    protected override void ShootBullet(Vector3 pos, Vector3 dir, float damageMultiplier = 1f)
    {
        Quaternion rot = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        var go = Instantiate(bulletPrefab, pos, rot);
        Bullet b = go.GetComponent<Bullet>();
        if (!b) return;

        b.SetUltimate(true);
        b.SetDamage(totalUltimateDamage);

        // 🔥 BulletData 새로 만들어 적용
        BulletData data = new BulletData();
        data.bulletSpeed = ultimateSpeed;
        data.range = ultimateRange;

        b.Init(pos, dir, data, playerIdx);
    }
    
    private void DoUltimateShot(Vector3 pos, Vector3 dir)
    {
        Vector3 p = firePoint.position;
        Vector3 d = firePoint.right;

        // 좌우 반전 대응
        if (transform.lossyScale.x < 0)
            d = -d;
        
        ShootBullet(p, d);
        ScheduleNextShot();
        PlayRecoil(d);

        Debug.Log($"🔥 Ultimate Fired! dmg={totalUltimateDamage}");
        
        AudioPlayer.Instance.Play("Ulti_Sound");
    }
    
}