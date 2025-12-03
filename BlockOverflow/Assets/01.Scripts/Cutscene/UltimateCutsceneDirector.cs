using System;
using UnityEngine;
using DG.Tweening;

public class UltimateCutsceneDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cutsceneCamera;
    [SerializeField] public Transform playerTransform;
    [SerializeField] public Transform firePoint;
    [SerializeField] private GameObject worldUI;

    [Header("Camera Settings")]
    [SerializeField] private float zoomFOV = 32f;
    [SerializeField] private float moveDuration = 0.7f;
    [SerializeField] private float tiltAngle = 10f;

    private float originalFOV;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private Action onFinished;

    public Camera Cam => cutsceneCamera;

    private void Awake()
    {
        if (!cutsceneCamera)
            cutsceneCamera = GetComponent<Camera>();
    }

    private void RefreshPlayerRefs()
    {
        if (!playerTransform)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (!firePoint && playerTransform)
            firePoint = playerTransform.Find("Weapon/FirePos");
    }

    public void Play(Action finished)
    {
        onFinished = finished;
        RefreshPlayerRefs();
        if (!playerTransform) return;

        Camera mainCam = Camera.main;

        cutsceneCamera.transform.position = mainCam.transform.position;
        cutsceneCamera.transform.rotation = mainCam.transform.rotation;
        cutsceneCamera.fieldOfView = mainCam.fieldOfView;

        originalCamPos = cutsceneCamera.transform.position;
        originalCamRot = cutsceneCamera.transform.rotation;
        originalFOV = cutsceneCamera.fieldOfView;

        mainCam.enabled = false;
        cutsceneCamera.enabled = true;

        Time.timeScale = 0f;
        worldUI?.SetActive(false);

        Vector3 charPos = playerTransform.position;
        float baseZ = originalCamPos.z;

        // 1️⃣ 캐릭터 기준 적당한 위치
        Vector3 focusPos = firePoint ? firePoint.position : playerTransform.position;

        Vector3 cinematicPos = new Vector3(
            focusPos.x - 0.5f,   // 살짝 왼쪽에서 비춤
            focusPos.y + 1.1f,   // 머리 위쪽
            originalCamPos.z     // ❗Z는 절대 건드리지 않음
        );

        // 2️⃣ 트윈 시퀀스
        DOTween.Kill(cutsceneCamera.transform);

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        // 이동 + 시선
        seq.Append(
            cutsceneCamera.transform.DOMove(cinematicPos, moveDuration)
                .SetEase(Ease.OutCubic)
        );
        seq.Join(
            cutsceneCamera.transform.DOLookAt(focusPos, moveDuration)
                .SetEase(Ease.OutCubic)
        );

        // 살짝 틀기
        seq.Append(
            cutsceneCamera.transform.DORotateQuaternion(
                Quaternion.Euler(7f, 18f, tiltAngle), 0.4f
            )
        );

        // 임팩트
        seq.Join(
            cutsceneCamera.transform.DOShakePosition(
                0.25f, 0.18f, 10, 90f, false, true
            )
        );

        seq.OnComplete(FinishCutscene);

    }

    private void FinishCutscene()
    {
        DOTween.Kill(cutsceneCamera.transform);

        cutsceneCamera.transform.position = originalCamPos;
        cutsceneCamera.transform.rotation = originalCamRot;
        cutsceneCamera.fieldOfView = originalFOV;

        Time.timeScale = 1f;
        worldUI?.SetActive(true);

        Camera mainCam = Camera.main;
        mainCam.enabled = true;
        cutsceneCamera.enabled = false;

        onFinished?.Invoke();
        onFinished = null;
    }
}
