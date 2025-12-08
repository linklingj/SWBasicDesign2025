using System;
using UnityEngine;
using DG.Tweening;

public class UltimateCutsceneDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cutsceneCamera;
    [SerializeField] private GameObject worldUI;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip ultimateSFX;
    [SerializeField] private SpriteRenderer[] backgroundBlur;

    [Header("Entry Move (Diagonal Intro)")]
    [SerializeField] private float entryXOffset = 18f;
    [SerializeField] private float entryZOffset = 6f;
    [SerializeField] private float entryDuration = 0.55f;

    [Header("Close-Up")]
    [SerializeField] private float baseHeight = 2.2f;
    [SerializeField] private float closeHeight = 1.7f;
    [SerializeField] private float closeZOffset = 1.8f;
    [SerializeField] private float closeXOffset = 2.2f;
    [SerializeField] private float closeUpFOV = 25f;
    [SerializeField] private float closeDuration = 0.6f;

    [Header("Tilt")]
    [SerializeField] private float tiltX = 10f;
    [SerializeField] private float tiltY = 25f;

    [Header("FX")]
    [SerializeField] private float slightShakePower = 0.1f;

    [Header("Cinematic Bars")]
    [SerializeField] private RectTransform barTop;
    [SerializeField] private RectTransform barBottom;
    [SerializeField] private float barAnimTime = 0.35f;
    [SerializeField] private Color bgColor;
    [SerializeField] private float barSize = 160f;

    private CameraController camController;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private float originalFOV;
    private Action onFinished;

    private Transform target;
    private Transform firePoint;
    private Color originalBGColor;

    private const float StageCenterX = 0f;

    public void Play(Transform user, Transform firePos, Action finished)
    {
        originalBGColor = backgroundBlur[0].color;
        foreach (var bg in backgroundBlur) bg.color = bgColor;

        target = user;
        firePoint = firePos ? firePos : user;
        onFinished = finished;

        if (!cutsceneCamera) cutsceneCamera = Camera.main;
        cutsceneCamera.depth = 20;
        camController = cutsceneCamera.GetComponentInParent<CameraController>();
        
        if (camController)
        {
            camController.HideFireTemporary();
            camController.Pause();
        }

        // **카메라와 무기에 걸린 Tween을 안전하게만 Kill**
        DOTween.Kill(cutsceneCamera.transform);
        DOTween.Kill(user); // 무기 반동 Tween도 여기서 제거

        worldUI?.SetActive(false);
        Time.timeScale = 0f;

        // 사운드
        if (ultimateSFX && sfxSource)
            sfxSource.PlayOneShot(ultimateSFX);

        originalPos = cutsceneCamera.transform.position;
        originalRot = cutsceneCamera.transform.rotation;
        originalFOV = cutsceneCamera.fieldOfView;

        bool playerIsLeft = (target.position.x < StageCenterX);
        float moveDir = playerIsLeft ? +1f : -1f;
        float tiltDir = playerIsLeft ? -1f : +1f;

        Vector3 focus = firePoint.position;
        Vector3 viewPoint = focus + Vector3.up * 1.45f;
        float camZ = -5f;

        Vector3 startPos = new Vector3(focus.x, focus.y + baseHeight, camZ);
        Vector3 diagonalPos = new Vector3(focus.x + entryXOffset * moveDir, focus.y + baseHeight, camZ + entryZOffset);
        Vector3 closePos = new Vector3(focus.x + closeXOffset * moveDir, focus.y + closeHeight + 0.4f, camZ + closeZOffset);

        cutsceneCamera.transform.position = startPos;
        cutsceneCamera.transform.rotation =
            Quaternion.LookRotation(viewPoint - startPos, Vector3.up)
            * Quaternion.Euler(tiltX, tiltY * tiltDir, 0);

        DOTween.Kill(cutsceneCamera.transform);

        Sequence seq = DOTween.Sequence().SetUpdate(true); // **TimeScale 0에서도 작동**

        ShowBlackBars(seq);

        seq.Append(cutsceneCamera.transform.DOMove(diagonalPos, entryDuration)
            .SetEase(Ease.OutQuad));

        seq.Append(cutsceneCamera.transform.DOMove(closePos, closeDuration)
            .SetEase(Ease.InOutBack));
        seq.Join(cutsceneCamera.transform.DOLookAt(viewPoint, closeDuration));
        seq.Join(cutsceneCamera.DOFieldOfView(closeUpFOV, closeDuration));
        seq.Join(cutsceneCamera.transform.DORotateQuaternion(
            Quaternion.Euler(tiltX, 0, 0), closeDuration));

        seq.Append(cutsceneCamera.transform.DOShakePosition(0.15f, slightShakePower, 20).SetUpdate(true));
        seq.AppendCallback(() => Time.timeScale = 0.05f);
        seq.AppendInterval(0.04f);
        seq.AppendCallback(() => Time.timeScale = 0f);

        seq.OnComplete(FinishCutscene);
    }

    private void FinishCutscene()
    {
        Time.timeScale = 1f;
        worldUI?.SetActive(true);

        cutsceneCamera.depth = -10;
        cutsceneCamera.transform.position = originalPos;
        cutsceneCamera.transform.rotation = originalRot;
        cutsceneCamera.fieldOfView = originalFOV;

        HideBlackBars(() =>
        {
            onFinished?.Invoke();
            onFinished = null;
        });

        foreach (var bg in backgroundBlur) bg.color = originalBGColor;

        if (camController)
        {
            camController.ShowFireTemporary();
            camController.Resume();
        }
    }

    void ShowBlackBars(Sequence seq)
    {
        if (!barTop || !barBottom) return;

        barTop.gameObject.SetActive(true);
        barBottom.gameObject.SetActive(true);

        barTop.anchoredPosition = new Vector2(0, barSize);
        barBottom.anchoredPosition = new Vector2(0, -barSize);

        seq.Join(barTop.DOAnchorPosY(0, barAnimTime).SetEase(Ease.OutCubic));
        seq.Join(barBottom.DOAnchorPosY(0, barAnimTime).SetEase(Ease.OutCubic));
    }

    void HideBlackBars(Action onComplete = null)
    {
        if (!barTop || !barBottom)
        {
            onComplete?.Invoke();
            return;
        }

        Sequence s = DOTween.Sequence().SetUpdate(true);
        s.Join(barTop.DOAnchorPosY(barSize, barAnimTime).SetEase(Ease.InCubic));
        s.Join(barBottom.DOAnchorPosY(-barSize, barAnimTime).SetEase(Ease.InCubic));
        s.OnComplete(() =>
        {
            barTop.gameObject.SetActive(false);
            barBottom.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }
}
