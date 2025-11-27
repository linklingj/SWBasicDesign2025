using UnityEngine;

public class ScreenFireFitter : MonoBehaviour
{
    [SerializeField] private Transform fireTop;
    [SerializeField] private Transform fireBottom;
    [SerializeField] private Transform fireLeft;
    [SerializeField] private Transform fireRight;

    // 카메라에서 얼마나 떨어진 깊이에 VFX를 둘지 (near 클리핑에서 살짝 띄움)
    [SerializeField] private float depthOffsetFromNear = 10f;

    // 🔥 방향별 오프셋
    [SerializeField] private float topOffset   = 0f;
    [SerializeField] private float bottomOffset = 0f;
    [SerializeField] private float leftOffset  = 0f;
    [SerializeField] private float rightOffset = 0f;

    private void Awake()
    {
        if (fireTop == null) fireTop = transform.Find("Top");
        if (fireBottom == null) fireBottom = transform.Find("Bottom");
        if (fireLeft == null) fireLeft = transform.Find("Left");
        if (fireRight == null) fireRight = transform.Find("Right");
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float depth = cam.nearClipPlane + depthOffsetFromNear;

        // 4개 화면 모서리의 월드 좌표
        Vector3 worldBL = cam.ViewportToWorldPoint(new Vector3(0f, 0f, depth)); //bottom-left
        Vector3 worldBR = cam.ViewportToWorldPoint(new Vector3(1f, 0f, depth)); //bottom-right
        Vector3 worldTL = cam.ViewportToWorldPoint(new Vector3(0f, 1f, depth)); //top-left
        Vector3 worldTR = cam.ViewportToWorldPoint(new Vector3(1f, 1f, depth)); //top-right

        // ScreenFireFitter 로컬 좌표로 변환
        worldBL = transform.InverseTransformPoint(worldBL);
        worldBR = transform.InverseTransformPoint(worldBR);
        worldTL = transform.InverseTransformPoint(worldTL);
        worldTR = transform.InverseTransformPoint(worldTR);

        // TOP
        if (fireTop != null)
        {
            Vector3 mid = (worldTL + worldTR) * 0.5f;
            float length = (worldTR - worldTL).magnitude;

            mid.y += topOffset;

            fireTop.localPosition = mid;
            fireTop.localScale = new Vector3(length, fireTop.localScale.y, 1f);
        }

        // BOTTOM
        if (fireBottom != null)
        {
            Vector3 mid = (worldBL + worldBR) * 0.5f;
            float length = (worldBR - worldBL).magnitude;

            mid.y += bottomOffset;

            fireBottom.localPosition = mid;
            fireBottom.localScale = new Vector3(length, fireBottom.localScale.y, 1f);
        }

        // LEFT
        if (fireLeft != null)
        {
            Vector3 mid = (worldBL + worldTL) * 0.5f;
            float length = (worldTL - worldBL).magnitude;

            mid.x += leftOffset;

            fireLeft.localPosition = mid;
            fireLeft.localScale = new Vector3(fireLeft.localScale.x, length, 1f);
        }

        // RIGHT
        if (fireRight != null)
        {
            Vector3 mid = (worldBR + worldTR) * 0.5f;
            float length = (worldTR - worldBR).magnitude;

            mid.x += rightOffset;

            fireRight.localPosition = mid;
            fireRight.localScale = new Vector3(fireRight.localScale.x, length, 1f);
        }
    }
}
