using UnityEngine;
using UnityEngine.InputSystem;


public class EyeFollowing : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float radius = 0.15f; // 눈알이 움직일 수 있는 최대 거리
    [SerializeField] private float followSpeed = 10f; // 부드러운 따라가기 속도

    [Header("References")]
    [SerializeField] private Vector3 eyeCenter; 

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }
    
    private Vector2 GetMouseWorldPosition()
    {
        Vector3 mouse = Mouse.current.position.ReadValue();
        float depth = Mathf.Abs(transform.position.z - cam.transform.position.z);
        if (depth < 0.0001f)
            depth = Mathf.Abs(cam.transform.position.z);

        mouse.z = depth;
        return cam.ScreenToWorldPoint(mouse);
    }

    private void Update()
    {

        // 마우스 → 월드 좌표
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        mouseWorldPos.z = eyeCenter.z;

        // 기준점 → 마우스 방향
        Vector3 direction = (mouseWorldPos - eyeCenter);

        // 반경 제한
        Vector3 clampedOffset = Vector3.ClampMagnitude(direction, radius);

        // 목표 위치 = 중심 + 제한된 방향
        Vector3 targetPos = eyeCenter + clampedOffset;

        // 부드럽게 이동
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * followSpeed
        );
    }
}

