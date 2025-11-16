using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class CameraController : SerializedMonoBehaviour
{
    
    [MinMaxSlider(-50, 50, true)]
    public Vector2 cameraRangeX = new Vector2();
    [MinMaxSlider(-30, 30, true)]
    public Vector2 cameraRangeY = new Vector2();

    [SerializeField] private Vector2 followOffset = Vector2.zero;
    
    [SerializeField, EnumPaging] private Dictionary<zoomType, float> zoomSize = new Dictionary<zoomType, float>();
    public enum zoomType { Wide, Normal, Close }
    
    
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    
    /// <summary>
    /// 카메라 줌을 부드럽게 변경하고, cameraRange를 지키며 위치 보정
    /// </summary>
    [Button]
    public void ZoomTo(Vector3 focusPoint, zoomType zoomType = zoomType.Normal, float zoomDuration = 0.3f)
    {
        if (cam == null) cam = Camera.main;

        // 목표 사이즈로 부드럽게 변경
        cam.DOFieldOfView(zoomSize[zoomType], zoomDuration).SetEase(Ease.InOutQuad);
        // 카메라 위치 보정
        transform.DOMove(ClampCameraPosition(focusPoint), zoomDuration).SetEase(Ease.InOutQuad);
    }

    /// <summary>
    /// 카메라 위치를 cameraRange 내로 제한
    /// </summary>
    private Vector3 ClampCameraPosition(Vector3 focusPoint)
    {

        // clamp 적용
        float clampedX = Mathf.Clamp(focusPoint.x, cameraRangeX.x, cameraRangeX.y);
        float clampedY = Mathf.Clamp(focusPoint.y, cameraRangeY.x, cameraRangeY.y );

        return new Vector3(clampedX, clampedY, transform.position.z);
    }
    
    public void ShakeCamera(float duration = 0.5f, float strength = 0.5f, int vibrato = 10)
    {
        if (cam == null) cam = Camera.main;
        transform.DOShakePosition(duration, strength, vibrato);
    }
}
