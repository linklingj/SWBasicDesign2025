using DG.Tweening;
using System;
using UnityEngine;

public class LogoStartup : MonoBehaviour {
    [SerializeField] private RectTransform baseLogo;
    [SerializeField] private RectTransform logoText;
    private void Start()
    {
        baseLogo.localScale = Vector3.zero;
        logoText.localScale = Vector3.zero;
        
        var textCanvasGroup = logoText.GetComponent<CanvasGroup>();
        if (textCanvasGroup == null)
            textCanvasGroup = logoText.gameObject.AddComponent<CanvasGroup>();
        textCanvasGroup.alpha = 0f;
        
        Sequence sequence = DOTween.Sequence();
        
        sequence//.Append(baseLogo.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBack))
                .Append(logoText.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack))
                .Join(textCanvasGroup.DOFade(1f, 0.6f));
        
        sequence.OnComplete(StartIdleAnimation);
    }

    private void StartIdleAnimation()
    {
        // baseLogo: 살짝 커졌다 작아졌다 반복
        // baseLogo.DOScale(1.05f, 1.5f)
        //     .SetEase(DG.Tweening.Ease.InOutSine)
        //     .SetLoops(-1, DG.Tweening.LoopType.Yoyo);

        // logoText: 3초마다 회전 흔들림
        logoText.DORotate(new Vector3(0, 0, 3f), 0.15f)
            .SetEase(DG.Tweening.Ease.InOutSine)
            .SetLoops(4, DG.Tweening.LoopType.Yoyo)
            .SetDelay(3f)
            .OnComplete(() => StartIdleAnimation()); // 재귀 호출
    }
}
