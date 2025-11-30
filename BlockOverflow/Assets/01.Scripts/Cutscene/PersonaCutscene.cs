using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class PersonaCutscene : MonoBehaviour
{
    [SerializeField] private Camera baseCamera;
    [SerializeField] private Camera cutsceneCamera;

    [Header("Root UI")]
    [SerializeField] private CanvasGroup rootGroup;

    [Header("Portrait")]
    [SerializeField] private RectTransform characterPortrait;
    [SerializeField] private CanvasGroup portraitGroup;
    [SerializeField] private float portraitFromX = -800f;
    [SerializeField] private float portraitDuration = 0.35f;
    [SerializeField] private Ease portraitEase = Ease.OutCubic;

    [Header("Text Effect")]
    [SerializeField] private TextLineData[] textDataLines;
    [SerializeField] private float typeSpeed = 0.04f;
    [SerializeField] private float lineDelay = 0.3f;
    [SerializeField] private TextMeshProUGUI textPrefab;
    [SerializeField] private Transform textParent;

    [Header("Effects")]
    [SerializeField] private Image redFlashBG;
    [SerializeField] private ParticleSystem slashEffect;
    [SerializeField] private CanvasGroup vignetteGroup;

    [Header("Exit")]
    [SerializeField] private float exitFadeDuration = 0.3f;

    [Header("World UI")]
    [SerializeField] private GameObject worldUI;

    private Sequence cutsceneSeq;
    private Sequence typingSeq;
    private float prevTimeScale;
    private bool isPlaying;
    private Action onFinished;
    private List<TextMeshProUGUI> spawnedTexts = new List<TextMeshProUGUI>();
    
    

    private void Awake()
    {
        HideAll();
    }

    private void HideAll()
    {
        rootGroup.alpha = 0f;
        portraitGroup.alpha = 0f;
        vignetteGroup.alpha = 0f;

        if (redFlashBG != null)
        {
            Color c = redFlashBG.color;
            c.a = 0f;
            redFlashBG.color = c;
        }
        if (cutsceneCamera) cutsceneCamera.enabled = false;
    }

    public void Play(Action finished = null)
    {
        if (isPlaying) return;
        isPlaying = true;

        onFinished = finished;
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (worldUI) worldUI.SetActive(false);

        baseCamera.enabled = false;
        cutsceneCamera.enabled = true;
        rootGroup.alpha = 1f;

        ResetPortrait();
        CreateSequence();
    }

    private void ResetPortrait()
    {
        portraitGroup.alpha = 0f;
        characterPortrait.anchoredPosition = new Vector2(portraitFromX, characterPortrait.anchoredPosition.y);
    }

    private void CreateSequence()
    {
        cutsceneSeq?.Kill();
        cutsceneSeq = DOTween.Sequence().SetUpdate(true);

        cutsceneSeq.AppendCallback(() =>
        {
            var systems = slashEffect.GetComponentsInChildren<ParticleSystem>();

            foreach (var ps in systems)
            {
                var main = ps.main;
                main.useUnscaledTime = true;
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Simulate(0f, true, true, true);
                ps.Play(true);
            }

            // 🔥 Slash 이동
            slashEffect.transform.DOLocalMoveX(6f, 0.45f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);

            // 🎥 시네마틱 카메라 연출
            DoCameraEffects();
        });



        cutsceneSeq.Join(redFlashBG.DOFade(0.85f, 0.12f));
        cutsceneSeq.Join(vignetteGroup.DOFade(0.3f, 0.25f));

        cutsceneSeq.Append(characterPortrait.DOAnchorPosX(0f, portraitDuration).SetEase(portraitEase));
        cutsceneSeq.Join(portraitGroup.DOFade(1f, portraitDuration * 0.85f));

        cutsceneSeq.AppendCallback(() => StartTyping());

        cutsceneSeq.AppendInterval(textDataLines.Length * (lineDelay + 0.2f));

        cutsceneSeq.Append(rootGroup.DOFade(0f, exitFadeDuration));
        cutsceneSeq.OnComplete(Finish);
    }

    private void StartTyping()
    {
        typingSeq?.Kill();
        typingSeq = DOTween.Sequence().SetUpdate(true);

        foreach (var data in textDataLines)
        {
            TextMeshProUGUI tmp = Instantiate(textPrefab, textParent);
            spawnedTexts.Add(tmp);

            var rect = tmp.GetComponent<RectTransform>();
            rect.anchoredPosition = data.anchoredPos;
            tmp.text = "";

            string line = data.text;

            // ⏱ 한 글자씩 타이핑
            typingSeq.Append(
                DOVirtual.Int(0, line.Length, line.Length * typeSpeed, c =>
                {
                    tmp.text = line.Substring(0, c);
                })
            );

            // 💥 등장 시 펀치 스케일
            typingSeq.Join(
                rect.DOPunchScale(Vector3.one * 0.25f, 0.25f, 10, 1f)
            );

            // ⚡ 한 번 튕기기
            typingSeq.AppendCallback(() =>
            {
                rect.DOShakePosition(0.18f, 10f, 20, 90f, false, true);
            });

            typingSeq.AppendInterval(lineDelay);
        }

        // 🔥 모든 텍스트가 끝난 뒤 최종 효과!
        typingSeq.AppendCallback(() =>
        {
            if (spawnedTexts.Count > 0)
            {
                var last = spawnedTexts[spawnedTexts.Count - 1].rectTransform;
                last.DOShakePosition(0.32f, 20f, 40, 120f, false, true);

                redFlashBG.DOFade(1f, 0.07f).SetLoops(2, LoopType.Yoyo);
            }
        });
    }




    private void Finish()
    {
        foreach (var tmp in spawnedTexts)
        {
            if (tmp != null)
                Destroy(tmp.gameObject);
        }
        spawnedTexts.Clear();
        
        if (worldUI) worldUI.SetActive(true);

        cutsceneCamera.enabled = false;
        baseCamera.enabled = true;

        Time.timeScale = prevTimeScale;

        isPlaying = false;
        onFinished?.Invoke();
        onFinished = null;

        HideAll();
    }

    private void OnDestroy()
    {
        cutsceneSeq?.Kill();
        typingSeq?.Kill();
        if (isPlaying)
            Time.timeScale = prevTimeScale;
    }
    private void DoCameraEffects()
    {
        if (!cutsceneCamera) return;

        var cam = cutsceneCamera;
        float startSize = cam.orthographicSize;

        Sequence camSeq = DOTween.Sequence().SetUpdate(true);

        // ░▒▓ 👁 1) 아주 서서히… 줌인 (대비 심화) ▓▒░
        camSeq.Append(
            DOTween.To(
                    () => cam.orthographicSize,
                    v => cam.orthographicSize = v,
                    startSize * 0.78f,    // 더 가까이 (22% 확대 효과)
                    2.2f                 // ★ 매우 느리게 진행
                )
                .SetEase(Ease.InOutQuad)
        );

        // ░▒▓ 📡 2) 느린 흔들림 → 살아있는 느낌 ▓▒░
        camSeq.Join(
            cam.transform.DOShakePosition(
                    2.2f,                  // 줌 전체 구간 동안 지속적으로
                    new Vector3(0.25f, 0.20f, 0),   // 미세한 파동 같은 흔들림
                    4,                    // 천천히 덜컹
                    70f,
                    false,
                    true
                )
                .SetUpdate(true)
        );
       

        // ░▒▓ 🌑 4) 원래대로 복귀 ▓▒░
        camSeq.Append(
            DOTween.To(
                () => cam.orthographicSize,
                v => cam.orthographicSize = v,
                startSize,
                0.8f
            ).SetEase(Ease.OutSine)
        );
    }

}
