using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PersonaCutscene : MonoBehaviour
{
    [SerializeField] private Camera baseCamera;
    [SerializeField] private Camera cutsceneCamera;

    [Header("Root Group")]
    [SerializeField] private CanvasGroup rootGroup;

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
    [SerializeField] private float holdDuration = 0.6f;
    [SerializeField] private float exitFadeDuration = 0.25f;

    [SerializeField] private GameObject worldUI;  // 체력바 포함 상위 UI
    
    private Sequence seq;
    private bool isPlaying;
    private float prevTimeScale;
    private Action _onFinished;

    private void Awake()
    {
        DisableVisuals();
    }

    private void DisableVisuals()
    {
        if (rootGroup) rootGroup.alpha = 0f;
        if (portraitGroup) portraitGroup.alpha = 0f;
        if (titleGroup) titleGroup.alpha = 0f;
        if (vignetteGroup) vignetteGroup.alpha = 0f;

        if (redFlashBG)
        {
            Color c = redFlashBG.color;
            c.a = 0;
            redFlashBG.color = c;
        }

        if (cutsceneCamera)
            cutsceneCamera.enabled = false;
    }

    public void Play(Action onFinished = null)
    {
        if (isPlaying) return;

        _onFinished = onFinished;

        StartSequence();
    }

    private void StartSequence()
    {
        isPlaying = true;
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        if (worldUI) worldUI.SetActive(false); // 🔥 여기 추가

        if (baseCamera) baseCamera.enabled = false;
        if (cutsceneCamera) cutsceneCamera.enabled = true;
        if (rootGroup) rootGroup.alpha = 1f;

        ResetUIPositions();
        seq?.Kill();
        seq = DOTween.Sequence().SetUpdate(true);

        seq.AppendCallback(() => slashEffect?.Play());

        seq.Join(redFlashBG?.DOFade(0.9f, 0.12f));
        seq.Join(vignetteGroup?.DOFade(0.25f, 0.25f));

        seq.Append(characterPortrait.DOAnchorPosX(0f, portraitDuration)
            .SetEase(portraitEase));
        seq.Join(portraitGroup.DOFade(1f, portraitDuration * 0.8f));

        seq.Insert(seq.Duration() - 0.15f,
                                      skillTitle.DOAnchorPosY(344.6f, titleDuration).SetEase(titleEase));
        seq.Insert(seq.Duration() - 0.15f,
            titleGroup.DOFade(1f, titleDuration * 0.9f));

        seq.AppendInterval(holdDuration);

        seq.Append(rootGroup.DOFade(0f, exitFadeDuration));
        seq.OnComplete(FinishCutscene);
    }

    private void ResetUIPositions()
    {
        var posP = characterPortrait.anchoredPosition;
        posP.x = portraitFromX;
        characterPortrait.anchoredPosition = posP;

        var posT = skillTitle.anchoredPosition;
        posT.y = titleFromY;
        skillTitle.anchoredPosition = posT;

        portraitGroup.alpha = 0f;
        titleGroup.alpha = 0f;
    }

    private void FinishCutscene()
    {
        if (worldUI) worldUI.SetActive(true); 
        if (cutsceneCamera) cutsceneCamera.enabled = false;
        if (baseCamera) baseCamera.enabled = true;

        Time.timeScale = prevTimeScale;
        isPlaying = false;

        _onFinished?.Invoke();
        _onFinished = null;
        DisableVisuals();
    }

    private void OnDestroy()
    {
        seq?.Kill();
        if (isPlaying)
            Time.timeScale = prevTimeScale;
    }
}
