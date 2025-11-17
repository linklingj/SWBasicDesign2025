using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarSliderUI : MonoBehaviour
{
    [Header("플레이어(오브젝트)")]
    [SerializeField] private Transform player;           // 플레이어 오브젝트

    private PlayerHealth playerHealth;                   // 체력바
    private Slider slider;

    [Header("머리 위 오프셋")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.5f, 0);

    [Header("체력바 크기 설정")]
    [SerializeField] private float baseWidth = 100f; // MaxHealth 100 기준 너비
    [SerializeField] private float height = 20f;

    [Header("데미지 피드백")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Color damageFlashColor = Color.white;
    [SerializeField] private float flashDuration = 0.15f;
    private Color originalColor;

    private Camera cam;
    private RectTransform rectTransform;

    private void Awake()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        
        if (slider == null)
            slider = GetComponent<Slider>();
        
        if (fillImage == null)
            fillImage = slider.fillRect.GetComponent<Image>();
        if (fillImage != null)
            originalColor = fillImage.color;
    }

    private void Start()
    {
        TrySetupFromPlayer();
    }

    private void OnValidate()
    {
        if (slider == null) 
            slider = GetComponent<Slider>();
        
        TrySetupFromPlayer();
    }


    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        TrySetupFromPlayer();
    }

    private void TrySetupFromPlayer()
    {
        if (player == null)
            return;

        // PlayerHealth 자동 찾기
        playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = player.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInChildren<PlayerHealth>();
        }

        // PlayerHealth 찾았으면 슬라이더 초기화
        if (playerHealth != null && slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = playerHealth.FinalMaxHealth;
            slider.value = playerHealth.CurrentHealth;

            if (rectTransform != null)
            {
                float scale = playerHealth.FinalMaxHealth / 100f;
                float newWidth = baseWidth * scale;
                rectTransform.sizeDelta = new Vector2(newWidth, height);
                rectTransform.pivot = new Vector2(0.5f, 0.5f); // 중심 고정
            }
        }
    }

    private void LateUpdate()
    {
        if (player == null || cam == null) return;

        // 플레이어 머리 위 화면좌표로 이동
        Vector3 worldPos = player.position + worldOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        rectTransform.position = screenPos;
    }

    private void Update()
    {
        if (playerHealth == null || slider == null) return;

        // 체력 감소
        slider.value = Mathf.Lerp(slider.value,
                                  playerHealth.CurrentHealth,
                                  Time.deltaTime * 10f);

        // 데미지 피드백
        if (playerHealth.TookDamageThisFrame)
        {
            StartCoroutine(FlashDamage());
        }

        if (playerHealth.IsDead)
        {
            gameObject.SetActive(false);
        }
        
    }
    
    private IEnumerator FlashDamage()
    {
        if (fillImage == null) yield break;

        fillImage.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        fillImage.color = originalColor;
    }
}
