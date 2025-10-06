using System;
using DG.Tweening;
using UnityEngine;

public class BlockAnimatior : MonoBehaviour {
    [SerializeField] private Vector3 shownSize;
    [SerializeField] private Vector3 placedSize;
    [SerializeField] private Vector3 dragSize;

    [Header("Animation Settings")]
    [SerializeField] private float appearDuration = 0.35f;
    [SerializeField] private float hoverDuration = 0.15f;
    [SerializeField] private float selectDuration = 0.2f;
    [SerializeField] private float dragDuration = 0.2f;
    [SerializeField] private float dropDuration = 0.25f;
    [SerializeField] private float disappearDuration = 0.2f;
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private int shakeVibrato = 10;

    private Sequence seq;
    private Tweener scaleTween, moveTween, rotateTween;

    void KillAllTweens()
    {
        if (seq != null && seq.IsActive()) seq.Kill();
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
        if (moveTween != null && moveTween.IsActive()) moveTween.Kill();
        if (rotateTween != null && rotateTween.IsActive()) rotateTween.Kill();
    }

    private void Start()
    {
        FirstAppearAnim();
    }

    // 블록이 처음 나타날 때 애니메이션
    // 사이즈가 0에서 shownSize로, easeoutback
    public void FirstAppearAnim()
    {
        //KillAllTweens();
        transform.localScale = Vector3.zero;
        scaleTween = transform.DOScale(shownSize, appearDuration)
            .SetEase(Ease.OutBack);
        //.SetLink(gameObject);
    }

    //살짝 커짐
    public void OnMouseEnter()
    {
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
        scaleTween = transform.DOScale(shownSize * 1.05f, hoverDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    //원래 크기로 되돌아옴
    public void OnMouseExit()
    {
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
        scaleTween = transform.DOScale(shownSize, hoverDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    //블록이 살짝 커지면서 흔들림(회전)
    public void SelectedAnim()
    {
        KillAllTweens();
        seq = DOTween.Sequence().SetLink(gameObject);
        seq.Join(transform.DOScale(shownSize * 1.1f, selectDuration).SetEase(Ease.OutBack));
        seq.Join(transform.DOShakeRotation(selectDuration, shakeStrength, shakeVibrato));
    }

    //작아지다가 소멸함
    public void NotSelectedAnim()
    {
        KillAllTweens();
        scaleTween = transform.DOScale(Vector3.zero, disappearDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => gameObject.SetActive(false))
            .SetLink(gameObject);
    }

    //dragsize로 커지고 살짝 흔들림
    public void OnDragStartAnim()
    {
        KillAllTweens();
        seq = DOTween.Sequence().SetLink(gameObject);
        seq.Join(transform.DOScale(dragSize, dragDuration).SetEase(Ease.OutBack));
        seq.Join(transform.DOShakeRotation(dragDuration, shakeStrength * 0.5f, shakeVibrato));
    }

    //placedsize로 돌아감, position 위치로 이동함
    public void OnDropAnim(Vector3 position)
    {
        KillAllTweens();
        seq = DOTween.Sequence().SetLink(gameObject);
        seq.Join(transform.DOMove(position, dropDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(placedSize, dropDuration).SetEase(Ease.OutBack));
        seq.Append(transform.DORotate(Vector3.zero, 0.05f));
    }

    //placedsize로 돌아감
    public void PlacedAnim()
    {
        KillAllTweens();
        scaleTween = transform.DOScale(placedSize, 0.2f)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }
    
}
