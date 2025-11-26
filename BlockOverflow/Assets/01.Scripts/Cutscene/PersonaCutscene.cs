using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PersonaCutscene : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera baseCamera;     // 원래 게임 카메라
    [SerializeField] private Camera cutsceneCamera; // Overlay 카메라

    [Header("Root Group (전체 페이드용, 선택사항)")]
    [SerializeField] private CanvasGroup rootGroup; // 없으면 null 둬도 됨

    [Header("Portrait")]
    [SerializeField] private RectTransform characterPortrait;
    [SerializeField] private CanvasGroup portraitGroup;
    [SerializeField] private float portraitFromX = -800f;
    [SerializeField] private float portraitDuration = 0.35f;
    [SerializeField] private Ease portraitEase = Ease.OutCubic;

    [Header("Skill Title")]
    [SerializeField] private TextMeshProUGUI skillTitleText;
    [SerializeField] private RectTransform skillTitle;
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private float titleFromY = -300f;
    [SerializeField] private float titleDuration = 0.25f;
    [SerializeField] private Ease titleEase = Ease.OutBack;

    [Header("Effects")]
    [SerializeField] private Image redFlashBG;
    [SerializeField] private ParticleSystem slashEffect;
    [SerializeField] private CanvasGroup vignetteGroup;

    [Header("Timing")]
    [SerializeField] private float holdDuration = 0.6f;   // 정지 유지 시간
    [SerializeField] private float exitFadeDuration = 0.25f;

    private Sequence currentSeq;
    private bool isPlaying;
    private float prevTimeScale;

    private Action _onFinished;

    private void Awake()
    {
        // 시작 시 안 보이게 초기화
        if (rootGroup) rootGroup.alpha = 0f;
        if (portraitGroup) portraitGroup.alpha = 0f;
        if (titleGroup) titleGroup.alpha = 0f;
        if (vignetteGroup) vignetteGroup.alpha = 0f;

        if (redFlashBG)
        {
            var c = redFlashBG.color;
            c.a = 0f;
            redFlashBG.color = c;
        }

        if (cutsceneCamera)
            cutsceneCamera.enabled = false;
    }

    /// <summary>
    /// 궁극기 컷신 시작. PlayerController에서 호출.
    /// </summary>
    public void Play(string skillName, Action onFinished = null)
    {
        if (isPlaying) return;

        _onFinished = onFinished;
        if (skillTitleText && !string.IsNullOrEmpty(skillName))
            skillTitleText.text = skillName;

        StartSequence();
    }

    private void StartSequence()
    {
        isPlaying = true;

        // 시간 멈추기
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // 카메라 세팅
        if (cutsceneCamera) cutsceneCamera.enabled = true;

        // 루트/초기 상태 세팅
        if (rootGroup) rootGroup.alpha = 1f;

        if (portraitGroup) portraitGroup.alpha = 0f;
        if (titleGroup) titleGroup.alpha = 0f;
        if (vignetteGroup) vignetteGroup.alpha = 0f;

        if (characterPortrait)
        {
            var pos = characterPortrait.anchoredPosition;
            pos.x = portraitFromX;
            characterPortrait.anchoredPosition = pos;
        }

        if (skillTitle)
        {
            var pos = skillTitle.anchoredPosition;
            pos.y = titleFromY;
            skillTitle.anchoredPosition = pos;
        }

        if (redFlashBG)
        {
            var c = redFlashBG.color;
            c.a = 0f;
            redFlashBG.color = c;
        }

        // DOTween 시퀀스 (unscaledTime 사용!)
        currentSeq?.Kill();
        currentSeq = DOTween.Sequence().SetUpdate(true);

        // 1) 붉은 번쩍 + 비네트 페이드인 + 베기 이펙트
        currentSeq.AppendCallback(() =>
        {
            if (slashEffect) slashEffect.Play();
        });

        if (redFlashBG)
        {
            currentSeq.Join(
                redFlashBG.DOFade(0.9f, 0.12f)
                          .From(0f)
                          .SetEase(Ease.OutQuad)
            );
        }

        if (vignetteGroup)
        {
            currentSeq.Join(
                vignetteGroup.DOFade(0.8f, 0.25f)
                             .From(0f)
                             .SetEase(Ease.OutQuad)
            );
        }

        // 2) 포트레이트 슬라이드 인
        if (characterPortrait && portraitGroup)
        {
            currentSeq.Append(
                characterPortrait.DOAnchorPosX(0f, portraitDuration)
                                 .SetEase(portraitEase)
            );
            currentSeq.Join(
                portraitGroup.DOFade(1f, portraitDuration * 0.8f)
            );
        }
        else
        {
            currentSeq.AppendInterval(0.1f);
        }

        // 3) 스킬 타이틀 튀어나오기 (포트레이트 조금 후에)
        if (skillTitle && titleGroup)
        {
            currentSeq.Insert(
                currentSeq.Duration() - 0.15f, // 포트레이트 거의 끝날 때
                skillTitle.DOAnchorPosY(0f, titleDuration).SetEase(titleEase)
            );
            currentSeq.Insert(
                currentSeq.Duration() - 0.15f,
                titleGroup.DOFade(1f, titleDuration * 0.9f)
            );
        }

        // 4) 정지 유지
        currentSeq.AppendInterval(holdDuration);

        // 5) 전체 페이드아웃
        if (rootGroup)
        {
            currentSeq.Append(rootGroup.DOFade(0f, exitFadeDuration));
        }
        else
        {
            currentSeq.AppendInterval(exitFadeDuration);
        }

        currentSeq.OnComplete(FinishCutscene);
    }

    private void FinishCutscene()
    {
        if (cutsceneCamera)
            cutsceneCamera.enabled = false;

        Time.timeScale = prevTimeScale;
        isPlaying = false;

        _onFinished?.Invoke();
        _onFinished = null;
    }

    private void OnDestroy()
    {
        currentSeq?.Kill();
        if (isPlaying)
            Time.timeScale = prevTimeScale;
    }
}
