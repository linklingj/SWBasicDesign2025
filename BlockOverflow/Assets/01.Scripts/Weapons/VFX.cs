using UnityEngine;

public class VFX : PoolObject
{
    [Header("기본 설정")]
    [SerializeField] private float lifetime = 0.3f;     // Animator가 없을 때만 사용
    [SerializeField] private Animator animator;         // 선택사항 (없으면 자동 무시)

    private float timer;
    private float activeLifetime;

    private void OnEnable()
    {
        timer = 0f;
        

        if (animator)
        {
            // 🔹 Animator 초기화 후 첫 프레임부터 재생
            animator.Rebind();
            animator.Update(0f);
            animator.Play(0, 0, 0f);
            

            // 🔹 현재 Animator의 첫 번째 클립 길이로 lifetime 자동 설정
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            if (clips != null && clips.Length > 0)
                activeLifetime = clips[0].length;
            else
                activeLifetime = lifetime; // 애니메이션이 없으면 fallback
        }
        else
        {
            activeLifetime = lifetime;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= activeLifetime)
        {
            Release();
        }
    }
}