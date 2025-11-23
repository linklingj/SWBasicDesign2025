using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterSelectManager : SerializedMonoBehaviour {
    [SerializeField] private int index;
    [SerializeField] PlayerCustomize customization;
    [SerializeField] private Canvas canvas;
    
    [SerializeField] private Dictionary<int, Sprite> hatSprites;
    
    [SerializeField] List<Image> colorPreviews;
    [SerializeField] List<Image> hatpreviews;

    [SerializeField] private Material bg;

    private void Awake()
    {
        for (int i = 0; i < colorPreviews.Count; i++)
        {
            colorPreviews[i].color = ColorGet.getCustomizeColor(i);
        }

        AnimateHats();
        DOVirtual.DelayedCall(1, AnimateColorPreviews);
        bg.DOColor(new Color(0.8f,0.8f,0.8f), "_Color", 0f);
    }

    public void StartGame()
    {
        GameManager.Instance.BattleStart();
    }
    
    private void AnimateColorPreviews()
    {
        if (customization == null || canvas == null || colorPreviews == null || colorPreviews.Count == 0)
            return;

        // 캔버스 RectTransform
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 월드(스프라이트) 위치 → 스크린 좌표
        Camera worldCam = Camera.main;
        Vector3 screenPos = worldCam != null
            ? worldCam.WorldToScreenPoint(customization.transform.position)
            : customization.transform.position;

        // 스크린 → 캔버스 로컬 좌표
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        float radius = 700;              // 캔버스 상에서의 반원 반지름 (픽셀 단위)
        float totalAngle = 120;          // 전체 반원 각도
        float delayBetween = 0.07f;       // 하나씩 퍼지는 딜레이

        int count = colorPreviews.Count;

        for (int i = 0; i < count; i++)
        {
            Image img = colorPreviews[i];
            if (img == null) continue;

            RectTransform rt = img.rectTransform;


            // target angle: 반원 위쪽 방향
            float t = (count == 1) ? 0.5f : (float)i / (count - 1);
            float angle = Mathf.Lerp(-totalAngle * 0.5f, totalAngle * 0.5f, t);
            float rad = angle * Mathf.Deg2Rad;

            // 최종 이동할 위치 (캔버스 로컬 좌표)
            Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            Vector2 targetPos = rt.anchoredPosition + offset;

            Color c = img.color;
            c.a = 0f;
            img.color = c;

            // DOTween 애니메이션
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(i * delayBetween);               // 순차적 딜레이
            seq.Append(img.DOFade(1f, 0.2f));                   // 부드러운 페이드 인
            seq.Join(rt.DOAnchorPos(targetPos, 0.4f)
                     .SetEase(Ease.OutBack));                   // 반원 위치로 튕기듯 이동
        }
    }
    
    private void AnimateHats()
    {
        if (customization == null || canvas == null || colorPreviews == null || colorPreviews.Count == 0)
            return;

        // 캔버스 RectTransform
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 월드(스프라이트) 위치 → 스크린 좌표
        Camera worldCam = Camera.main;
        Vector3 screenPos = worldCam != null
            ? worldCam.WorldToScreenPoint(customization.transform.position)
            : customization.transform.position;

        // 스크린 → 캔버스 로컬 좌표
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        float radius = 500;              // 캔버스 상에서의 반원 반지름 (픽셀 단위)
        float totalAngle = 160;          // 전체 반원 각도
        float delayBetween = 0.07f;       // 하나씩 퍼지는 딜레이

        int count = hatpreviews.Count;

        for (int i = 0; i < count; i++)
        {
            Image img = hatpreviews[i];
            if (img == null) continue;

            RectTransform rt = img.rectTransform;


            // target angle: 반원 위쪽 방향
            float t = (count == 1) ? 0.5f : (float)i / (count - 1);
            float angle = Mathf.Lerp(-totalAngle * 0.5f, totalAngle * 0.5f, t);
            float rad = angle * Mathf.Deg2Rad;

            // 최종 이동할 위치 (캔버스 로컬 좌표)
            Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            Vector2 targetPos = rt.anchoredPosition + offset;

            Color c = img.color;
            c.a = 0f;
            img.color = c;

            // DOTween 애니메이션
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(i * delayBetween);               // 순차적 딜레이
            seq.Append(img.DOFade(1f, 0.2f));                   // 부드러운 페이드 인
            seq.Join(rt.DOAnchorPos(targetPos, 0.4f)
                .SetEase(Ease.OutBack));                   // 반원 위치로 튕기듯 이동
        }
    }
    
    public void SelectColor(int colorIndex)
    {
        Color color = ColorGet.getCustomizeColor(colorIndex);
        SelectColor(color);
    }
    
    public void SelectHat(int hatIndex)
    {
        GameManager.Instance.GetPlayerData(out var playerData, index);
        playerData.customization.hatSprite = hatSprites[hatIndex];
        customization.Init(playerData.customization);
        if (hatIndex == 0)
            customization.SetHat(false);
        else
            customization.SetHat(true);
    }
    
    [Button]
    public void SelectColor(Color color)
    {
        GameManager.Instance.GetPlayerData(out var playerData, index);
        playerData.customization.playerColor = color;
        customization.Init(playerData.customization);
        customization.SetColor(true);
        //set background shader input color
        bg.DOColor(Color.Lerp(color, Color.white, 0.5f), "_Color", 1f);
    }
}
