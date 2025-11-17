using System;
using DG.Tweening;
using UnityEngine;

public class BlockAnimator : MonoBehaviour {
    public bool interactable = true;
    
    [SerializeField] BlockAnimatorData blockAnimData;

    private Sequence seq;
    private Tweener scaleTween, moveTween, rotateTween;

    private bool mousePopupInteraction;

    private void Start()
    {
        mousePopupInteraction = true;
    }

    void KillAllTweens()
    {
        if (seq != null && seq.IsActive()) seq.Kill();
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
        if (moveTween != null && moveTween.IsActive()) moveTween.Kill();
        if (rotateTween != null && rotateTween.IsActive()) rotateTween.Kill();
    }

    // 블록이 처음 나타날 때 애니메이션
    // 사이즈가 0에서 shownSize로, easeoutback
    public void FirstAppearAnim(int idx, int total)
    {
        KillAllTweens();
        if (!interactable) return;
        float diff = idx - (float)(total - 1) / 2;
        Vector3 offset = new Vector3(diff * blockAnimData.generateSpread, 0, 0);
        transform.position = blockAnimData.showPosition + offset;
        transform.localScale = Vector3.zero;
        scaleTween = transform.DOScale(blockAnimData.shownSize, blockAnimData.appearDuration)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject);
    }

    
    //마우스 가져갔을때 살짝 커짐
    public void OnMouseEnter()
    {
        if (!interactable) return;
        if (!mousePopupInteraction) return;
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
        scaleTween = transform.DOScale(blockAnimData.shownSize * 1.05f, blockAnimData.hoverDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    //원래 크기로 되돌아옴
    public void OnMouseExit()
    {
        if (!interactable) return;
        if (!mousePopupInteraction) return;
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
        scaleTween = transform.DOScale(blockAnimData.shownSize, blockAnimData.hoverDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    //블록이 살짝 커지면서 흔들림(회전)
    public void SelectedAnim()
    {
        KillAllTweens();
        mousePopupInteraction = false;
        seq = DOTween.Sequence().SetLink(gameObject);
        seq.Join(transform.DOScale(blockAnimData.shownSize * 1.3f, blockAnimData.selectDuration).SetEase(Ease.OutBack));
        seq.Join(transform.DOShakeRotation(blockAnimData.selectDuration, blockAnimData.shakeStrength, blockAnimData.shakeVibrato));
        seq.Append(transform.DOScale(Vector3.zero, blockAnimData.disappearDuration).SetEase(Ease.InQuad));
        seq.Append(transform.DOScale(blockAnimData.placedSize, blockAnimData.appearDuration)
            .SetDelay(blockAnimData.selectDuration)
            .OnPlay(() => transform.position = blockAnimData.selectedPosition))
            .SetEase(Ease.OutBack);
    }

    //작아지다가 소멸함
    public void NotSelectedAnim()
    {
        KillAllTweens();
        scaleTween = transform.DOScale(Vector3.zero, blockAnimData.disappearDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => gameObject.SetActive(false));
    }

    //dragsize로 커지고 살짝 흔들림
    public void OnDragStartAnim()
    {
        KillAllTweens();
        seq = DOTween.Sequence().SetLink(gameObject);
        seq.Join(transform.DOScale(blockAnimData.dragSize, blockAnimData.dragDuration).SetEase(Ease.OutBack));
        seq.Join(transform.DOShakeRotation(blockAnimData.dragDuration, blockAnimData.shakeStrength * 0.5f, blockAnimData.shakeVibrato));
    }

    //placedsize로 돌아감, position 위치로 이동함
    public void OnDropAnim(Vector3 position)
    {
        KillAllTweens();
        seq = DOTween.Sequence().SetLink(gameObject);
        seq.Join(transform.DOMove(position, blockAnimData.dropDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(blockAnimData.placedSize, blockAnimData.dropDuration).SetEase(Ease.OutBack));
        seq.Append(transform.DORotate(Vector3.zero, 0.05f));
    }

    //placedsize로 돌아감
    public void PlacedAnim(Vector3 position)
    {
        KillAllTweens();
        seq = DOTween.Sequence().SetLink(gameObject);
        seq.Join(transform.DOMove(position, blockAnimData.dropDuration).SetEase(Ease.OutBack));
        scaleTween = transform.DOScale(blockAnimData.placedSize, 0.2f)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }
    
}
