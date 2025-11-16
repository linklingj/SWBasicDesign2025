using UnityEngine;

public class DamageTextSpawner : MonoBehaviour
{
    public static DamageTextSpawner Instance { get; private set; }

    [SerializeField] private DamageText damageTextPrefab; // 프리팹 (Project 창의 프리팹!)
    [SerializeField] private Canvas worldCanvas;          // Render Mode = Screen Space - Overlay

    private RectTransform _canvasRect;
    private Camera _cam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (worldCanvas != null)
            _canvasRect = worldCanvas.transform as RectTransform;

        _cam = Camera.main;

        if (worldCanvas != null && worldCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogWarning("DamageTextSpawner: 이 버전은 Screen Space - Overlay 기준 코드임!");
        }
    }

    public void ShowDamage(float amount, Vector3 worldPos)
    {
        if (damageTextPrefab == null || worldCanvas == null)
        {
            Debug.LogWarning("DamageTextSpawner: prefab 또는 canvas가 비어 있음");
            return;
        }

        // 풀에서 하나 꺼내오기
        GameObject obj = ObjectPoolManager.Instance.Get(damageTextPrefab.gameObject);
        if (obj == null) return;

        DamageText dt = obj.GetComponent<DamageText>();
        if (dt == null) return;

        // 캔버스 자식으로 붙이기
        dt.transform.SetParent(worldCanvas.transform, false);

        RectTransform dtRect = dt.GetComponent<RectTransform>();

        // 1) 월드 기준 위치에 살짝 랜덤 오프셋
        Vector3 worldOffset = worldPos + new Vector3(
            Random.Range(0.5f, 0.5f),
            Random.Range(0.5f, 1f),
            0f
        );

        // 2) 월드 → 스크린 좌표로 변환
        if (_cam == null) _cam = Camera.main;
        Vector3 screenPos = _cam.WorldToScreenPoint(worldOffset);
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPos,
            null,        // Overlay 모드는 카메라 null
            out localPoint
        );

        dtRect.anchoredPosition = localPoint;

        // 4) 텍스트 세팅
        dt.Setup(Mathf.RoundToInt(amount));
    }
}
