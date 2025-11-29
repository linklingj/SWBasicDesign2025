
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 각 UI 버튼에 붙어서
///  - 마우스 오버 시 살짝 커지고
///  - 마우스가 나가면 원래 크기로 돌아오며
///  - 선택되면 커지면서 살짝 흔들리는 애니메이션을 재생한다.
///
/// 첫 번째 버튼(혹은 원하는 버튼)은 인스펙터에서 defaultSelected를 켜서
/// 기본 선택 상태로 둘 수 있다.
/// </summary>
public class UIAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Scale Settings")]
    [SerializeField] private float normalScale = 1f;      // 기본 크기
    [SerializeField] private float hoverScale = 1.1f;     // 마우스 오버 시 크기
    [SerializeField] private float selectedScale = 1.2f;  // 선택 시 크기
    [SerializeField] private float tweenDuration = 0.15f; // 스케일 트윈 시간

    [Header("Selection Anim")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private Vector3 shakeStrength = new Vector3(0.15f, 0.15f, 0f);
    [SerializeField] private int shakeVibrato = 15;
    [SerializeField] private float shakeRandomness = 90f;
    [SerializeField] private bool defaultSelected = false; // 씬 시작 시 기본 선택 여부

    private RectTransform rectTransform;
    private Tween currentTween;

    // 현재 선택된 버튼 (전역적으로 하나만 선택되도록 유지)
    private static UIAnimator currentSelected;

    private void Awake()
    {
        rectTransform = transform as RectTransform;

        // 시작 시 기본 크기 설정
        rectTransform.localScale = Vector3.one * normalScale;
    }

    private void OnEnable()
    {
        // 기본 선택 플래그가 켜져 있고 아직 선택된 버튼이 없다면 이 버튼을 선택
        if (defaultSelected && currentSelected == null)
        {
            Select(playAnim: false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 선택된 버튼이면 hover 애니메이션은 따로 필요 없음
        if (currentSelected == this)
            return;

        PlayScaleTween(hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 선택된 버튼이면 원래 크기로 돌리지 않는다
        if (currentSelected == this)
            return;

        PlayScaleTween(normalScale);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Select(playAnim: true);
    }

    /// <summary>
    /// 이 버튼을 선택 상태로 만든다.
    /// </summary>
    public void Select(bool playAnim)
    {
        if (currentSelected == this)
            return;

        // 이전 선택 해제
        if (currentSelected != null)
        {
            currentSelected.Deselect();
        }

        currentSelected = this;

        currentTween?.Kill();

        if (playAnim)
        {
            // 선택 시: 살짝 커지면서 shake
            Sequence seq = DOTween.Sequence();
            seq.Append(rectTransform.DOScale(Vector3.one * selectedScale, tweenDuration).SetEase(Ease.OutQuad));
            seq.Join(rectTransform.DOShakeScale(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness));
            currentTween = seq;
        }
        else
        {
            rectTransform.localScale = Vector3.one * selectedScale;
        }
    }

    /// <summary>
    /// 선택 해제 시 기본 크기로 되돌린다.
    /// </summary>
    public void Deselect()
    {
        currentTween?.Kill();
        PlayScaleTween(normalScale);
    }

    private void PlayScaleTween(float targetScale)
    {
        currentTween?.Kill();
        currentTween = rectTransform
            .DOScale(Vector3.one * targetScale, tweenDuration)
            .SetEase(Ease.OutQuad);
    }
}
