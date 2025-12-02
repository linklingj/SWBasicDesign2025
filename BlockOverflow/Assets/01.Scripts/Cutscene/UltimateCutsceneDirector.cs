using System;
using UnityEngine;
using DG.Tweening;

public class UltimateCutsceneDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cutsceneCamera;      // 컷신용 카메라 (Hierarchy에 있는 CutsceneCam)
    [SerializeField] public Transform playerTransform;  // 플레이어 (필요하면 런타임에 다시 찾음)
    [SerializeField] public Transform firePoint;        // Weapon/FirePos
    [SerializeField] private GameObject worldUI;        // WorldUI 루트

    [Header("Camera Settings")]
    [SerializeField] private float zoomFOV = 30f;       // 컷신 때 목표 FOV
    [SerializeField] private float moveDuration = 0.6f; // 처음 줌+이동 시간
    [SerializeField] private float camRotateAngle = 8f; // 약간 기울이는 각도(Z축)

    private float originalFOV;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private Action onFinished;

    public Camera Cam => cutsceneCamera;

    private void Awake()
    {
        if (!cutsceneCamera)
            cutsceneCamera = GetComponent<Camera>();

        RefreshPlayerRefs();
        CacheCameraDefaultsFromMain();
    }

    /// <summary>
    /// Player / FirePos 참조 갱신
    /// </summary>
    private void RefreshPlayerRefs()
    {
        if (!playerTransform)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (!playerTransform)
        {
            Debug.LogWarning("❌ UltimateCutsceneDirector: Player not found yet.");
            return;
        }

        if (!firePoint)
        {
            // Player 안에 Weapon/FirePos 찾아보기
            firePoint = playerTransform.Find("Weapon/FirePos");
            if (!firePoint)
            {
                Debug.LogWarning("⚠ FirePos not found, using Player position as fallback.");
                firePoint = playerTransform;
            }
        }
    }

    /// <summary>
    /// 메인 카메라 상태를 기준으로 원래 값 저장
    /// </summary>
    private void CacheCameraDefaultsFromMain()
    {
        Camera mainCam = Camera.main;
        if (!mainCam || !cutsceneCamera) return;

        originalFOV   = mainCam.fieldOfView;
        originalCamPos = mainCam.transform.position;
        originalCamRot = mainCam.transform.rotation;

        // 컷신 카메라도 처음엔 메인카메라 위치/회전으로 맞춰둠
        cutsceneCamera.transform.position   = originalCamPos;
        cutsceneCamera.transform.rotation   = originalCamRot;
        cutsceneCamera.fieldOfView          = originalFOV;
    }

    /// <summary>
    /// 궁극기 컷신 시작
    /// </summary>
    public void Play(Action finished)
    {
        onFinished = finished;
        CacheCameraDefaultsFromMain();
        RefreshPlayerRefs();
        if (!playerTransform || !firePoint)
        {
            Debug.LogError("❌ Cutscene cannot start — missing player/firePoint!");
            FinishCutscene();
            return;
        }

        Time.timeScale = 0f;
        worldUI?.SetActive(false);

        // 카메라 시작 위치 = 메인 카메라 위치
        cutsceneCamera.transform.position = originalCamPos;
        cutsceneCamera.transform.rotation = originalCamRot;
        cutsceneCamera.fieldOfView = originalFOV;

        Vector3 focusPos = firePoint.position;
        Vector3 targetPos = firePoint.position + new Vector3(0f, 1.2f, +3f);
        targetPos.z = originalCamPos.z;

        Debug.Log($"🎬 CUTSCENE START — Focus {focusPos}, Target {targetPos}");

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        seq.Append(
            cutsceneCamera.DOFieldOfView(zoomFOV, moveDuration)
                .SetEase(Ease.OutCubic)
        );
        seq.Join(
            cutsceneCamera.transform.DOMove(targetPos, moveDuration)
                .SetEase(Ease.OutCubic)
        );

        seq.Append(
            cutsceneCamera.transform.DOLookAt(focusPos, 0.25f)
                .SetEase(Ease.OutSine)
        );
        seq.Join(
            cutsceneCamera.transform.DORotate(
                    new Vector3(0, 0, camRotateAngle), 0.4f)
                .SetEase(Ease.InOutSine)
        );

        seq.Append(
            cutsceneCamera.transform.DOShakePosition(
                0.3f, 0.18f, 12, 90f, false, true)
        );

        seq.AppendCallback(FinishCutscene);
    }


    private void FinishCutscene()
    {
        // 컷신 카메라 상태 원복 (다음 궁극기 대비용)
        if (cutsceneCamera)
        {
            cutsceneCamera.transform.position = originalCamPos;
            cutsceneCamera.transform.rotation = originalCamRot;
            cutsceneCamera.fieldOfView        = originalFOV;
        }

        Time.timeScale = 1f;
        if (worldUI) worldUI.SetActive(true);

        Debug.Log("🎬 CUTSCENE END");

        onFinished?.Invoke();
    }
}
