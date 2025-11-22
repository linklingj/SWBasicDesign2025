using UnityEngine;

public class ScreenFireFitter : MonoBehaviour
{
    [SerializeField] private Transform fireTop;
    [SerializeField] private Transform fireBottom;
    [SerializeField] private Transform fireLeft;
    [SerializeField] private Transform fireRight;

    // 카메라에서 얼마나 떨어진 깊이에 VFX를 둘지 (near 클리핑에서 살짝만 띄움)
    [SerializeField] private float depthOffsetFromNear = 0.2f;

    // 띠 두께
    [SerializeField] private float edgeThickness = 1f;

    private void Awake()
    {
        // 이름으로 자동 찾기
        if (fireTop == null)    fireTop    = transform.Find("Top");
        if (fireBottom == null) fireBottom = transform.Find("Bottom");
        if (fireLeft == null)   fireLeft   = transform.Find("Left");
        if (fireRight == null)  fireRight  = transform.Find("Right");
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // 카메라에서 얼마나 앞쪽 평면에 둘지 (near + 조금)
        float depth = cam.nearClipPlane + depthOffsetFromNear;

        // 뷰포트 기준 모서리 4점의 "월드 좌표"를 구한다.
        // (0,0) = 왼아래, (1,1) = 오른위
        Vector3 worldBL = cam.ViewportToWorldPoint(new Vector3(0f, 0f, depth)); // Bottom Left
        Vector3 worldBR = cam.ViewportToWorldPoint(new Vector3(1f, 0f, depth)); // Bottom Right
        Vector3 worldTL = cam.ViewportToWorldPoint(new Vector3(0f, 1f, depth)); // Top Left
        Vector3 worldTR = cam.ViewportToWorldPoint(new Vector3(1f, 1f, depth)); // Top Right

        // 이 스크립트는 ScreenBorderVFX(카메라 자식) 기준으로 local 좌표를 쓴다.
        worldBL = transform.InverseTransformPoint(worldBL);
        worldBR = transform.InverseTransformPoint(worldBR);
        worldTL = transform.InverseTransformPoint(worldTL);
        worldTR = transform.InverseTransformPoint(worldTR);

        // 위쪽 띠 (Top)
        if (fireTop != null)
        {
            Vector3 midTop = (worldTL + worldTR) * 0.5f;
            float length = (worldTR - worldTL).magnitude;

            fireTop.localPosition = midTop;
            var s = fireTop.localScale;
            s.x = length;           // 가로 길이
            s.y = edgeThickness;    // 두께
            fireTop.localScale = s;
        }

        // 아래쪽 띠 (Bottom)
        if (fireBottom != null)
        {
            Vector3 midBottom = (worldBL + worldBR) * 0.5f;
            float length = (worldBR - worldBL).magnitude;

            fireBottom.localPosition = midBottom;
            var s = fireBottom.localScale;
            s.x = length;
            s.y = edgeThickness;
            fireBottom.localScale = s;
        }

        // 왼쪽 띠 (Left)
        if (fireLeft != null)
        {
            Vector3 midLeft = (worldBL + worldTL) * 0.5f;
            float length = (worldTL - worldBL).magnitude;

            fireLeft.localPosition = midLeft;
            var s = fireLeft.localScale;
            s.x = edgeThickness;
            s.y = length;
            fireLeft.localScale = s;
        }

        // 오른쪽 띠 (Right)
        if (fireRight != null)
        {
            Vector3 midRight = (worldBR + worldTR) * 0.5f;
            float length = (worldTR - worldBR).magnitude;

            fireRight.localPosition = midRight;
            var s = fireRight.localScale;
            s.x = edgeThickness;
            s.y = length;
            fireRight.localScale = s;
        }
    }
}
