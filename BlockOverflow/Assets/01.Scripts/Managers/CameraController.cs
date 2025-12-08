using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class CameraController : SerializedMonoBehaviour
{
    [SerializeField] BattleManager battleManager;
    [SerializeField] Transform childObj;
    [SerializeField] Transform cameraTransform;
    
    [MinMaxSlider(-50, 50, true)]
    public Vector2 cameraRangeX = new Vector2();
    [MinMaxSlider(-30, 30, true)]
    public Vector2 cameraRangeY = new Vector2();

    [SerializeField] private Vector2 followOffset = Vector2.zero;
    
    [SerializeField, EnumPaging] private Dictionary<zoomType, float> zoomSize = new Dictionary<zoomType, float>();
    public enum zoomType { Wide, Normal, Close }

    public float moveStartTime;
    public float moveDuration;
    
    private Camera cam;
    private Transform startPos, endPos;

    [Header("Sine Movement Settings")]
    [SerializeField] private float sineAmplitude = 0.5f;
    [SerializeField] private float sineFrequency = 1f;
    private float sineTime;

    [Header("Vignette Settings")]
    [SerializeField] private UnityEngine.Rendering.Volume postProcessVolume;
    [SerializeField] private float vignetteMax = 0.45f;
    [SerializeField] private float vignetteBlinkSpeed = 6f;
    private UnityEngine.Rendering.Universal.Vignette vignette;
    
    [Header("Screen Effects")]
    [SerializeField] GameObject screenFireEffect;
    
    [Header("Sound")]
    [SerializeField] AudioData startShrinkSound;
    bool played = false;

    // 🔥 추가된 부분
    private bool IsPaused = false;
    private bool wasFollowingBeforePause = false;
    
    private void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        sineTime = 0;
        if (postProcessVolume != null) postProcessVolume.profile.TryGet(out vignette);
        if (screenFireEffect) screenFireEffect.SetActive(false);
    }

    private void Update()
    {
        MoveCamera();
        UpdateSineMovement();
    }

    [Button]
    public void ZoomTo(Vector3 focusPoint, zoomType zoomType = zoomType.Normal, float zoomDuration = 0.3f, float z = -31.6f)
    {
        if (cam == null) cam = Camera.main;

        if (cam.orthographic)
            cam.DOOrthoSize(zoomSize[zoomType], zoomDuration).SetEase(Ease.InOutQuad);
        else
            cam.DOFieldOfView(zoomSize[zoomType], zoomDuration).SetEase(Ease.InOutQuad);

        focusPoint.z = z;
        transform.DOMove(ClampCameraPosition(focusPoint), zoomDuration).SetEase(Ease.InOutQuad);
    }

    private Vector3 ClampCameraPosition(Vector3 focusPoint)
    {
        float clampedX = Mathf.Clamp(focusPoint.x, cameraRangeX.x, cameraRangeX.y);
        float clampedY = Mathf.Clamp(focusPoint.y, cameraRangeY.x, cameraRangeY.y);
        return new Vector3(clampedX, clampedY, focusPoint.z);
    }
    
    public void ShakeCamera(float duration = 0.5f, float strength = 0.5f, int vibrato = 10)
    {
        if (cam == null) cam = Camera.main;
        cameraTransform.DOShakePosition(duration, strength, vibrato);
    }
    
    public void SetTargetPositions(Transform startPos, Transform endPos)
    {
        this.startPos = startPos;
        this.endPos = endPos;
    }

    public void MoveCamera()
    {
        if (IsPaused) return; // 🔥 컷신 중엔 카메라 이동 정지

        if (!battleManager || !startPos || !endPos) return;
        if (!battleManager.gameStarted) return;

        float moveTime = battleManager.GameTime - moveStartTime;
        if (moveTime <= 0)
        {
            screenFireEffect.SetActive(false);
            return;
        }
        
        if (!played)
        {
            AudioPlayer.Instance.Play(startShrinkSound);
            played = true;
            screenFireEffect.SetActive(true);
        }
        
        float t = Mathf.Clamp01(moveTime / moveDuration);
        
        transform.position = Vector3.Lerp(startPos.position, endPos.position, t);

        float intensity = Mathf.Lerp(0.2f, vignetteMax, t);

        if (t > 0.99f) intensity += Mathf.Sin((moveTime - moveDuration) * vignetteBlinkSpeed) * 0.03f;

        if (vignette) vignette.intensity.value = Mathf.Clamp01(intensity);
    }
    
    private void UpdateSineMovement()
    {
        if (IsPaused) return; // 🔥 흔들림도 정지

        if (childObj == null) return;

        sineTime += Time.deltaTime * sineFrequency;
        float offsetY = Mathf.Sin(sineTime) * sineAmplitude;
        Vector3 localPos = childObj.localPosition;
        localPos.y = offsetY;
        childObj.localPosition = localPos;
    }

    public void HideFireTemporary()
    {
        if (played) screenFireEffect.SetActive(false);
        Debug.Log("Hide Fire Effect");
    }

    public void ShowFireTemporary()
    {
        if (played) screenFireEffect.SetActive(true);
        Debug.Log("show Fire Effect");
    }

    public void Pause()
    {
        wasFollowingBeforePause = !IsPaused;
        IsPaused = true;
    }

    public void Resume()
    {
        if (!wasFollowingBeforePause)
        {
            return;
        }
        IsPaused = false;
    }
}
