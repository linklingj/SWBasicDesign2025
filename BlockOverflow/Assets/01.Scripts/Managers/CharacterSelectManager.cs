using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

public class CharacterSelectManager : SerializedMonoBehaviour {
    [SerializeField] private int index;
    [SerializeField] PlayerCustomize customization;
    [SerializeField] private Canvas canvas;
    
    [SerializeField] private Dictionary<int, Sprite> hatSprites;
    
    [SerializeField] List<Image> colorPreviews;
    [SerializeField] List<Image> hatPreviews;
    [SerializeField] List<Image> weaponPreviews;

    [SerializeField] private Material bg;

    [SerializeField] private Image nextButton;
    [SerializeField] private TextUIElement nextButtonText;
    
    [SerializeField] CharacterSelectManager otherPlayerSelectManager;
    [SerializeField] public GameObject startButton;

    [SerializeField] private WeaponController playerWeapon;
    [SerializeField] private Weapon weaponObj;
    [SerializeField] private Dictionary<WeaponType, WeaponData> weaponData;
    
    [SerializeField] private RectTransform CharacterSelectUI;

    public int state = 0;
    private void Awake()
    {
        for (int i = 0; i < colorPreviews.Count; i++)
        {
            colorPreviews[i].color = ColorGet.getCustomizeColor(i);
        }

        AnimateHats();
        foreach (var item in colorPreviews)
            item.DOFade(0f, 0f);
        foreach (var item in weaponPreviews)
            item.DOFade(0f, 0f);
        DOVirtual.DelayedCall(0.5f, AnimateColorPreviews, false);
        bg.DOColor(new Color(0.8f,0.8f,0.8f), "_Color", 0f);
        bg.SetVector("_Tiling", new Vector2(10,10));
    }

    private void Start()
    {
        if (index == 1)
        {
            CharacterSelectUI.gameObject.SetActive(true);
            CharacterSelectUI.localScale = Vector3.zero;
            CharacterSelectUI.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
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

        int count = hatPreviews.Count;

        for (int i = 0; i < count; i++)
        {
            Image img = hatPreviews[i];
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
    
    private void AnimateWeapons()
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

        float radius = 400;              // 캔버스 상에서의 반원 반지름 (픽셀 단위)
        float totalAngle = 90;          // 전체 반원 각도
        float delayBetween = 0.07f;       // 하나씩 퍼지는 딜레이

        int count = weaponPreviews.Count;

        for (int i = 0; i < count; i++)
        {
            Image img = weaponPreviews[i];
            if (img == null) continue;

            RectTransform rt = img.rectTransform;


            float t = (count == 1) ? 0.5f : (float)i / (count - 1);
            float angle = Mathf.Lerp(-totalAngle * 0.5f, totalAngle * 0.5f, t);
            angle += 90;
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
    
    public void SelectWeapon(int weaponIndex)
    {
        WeaponType weaponType = (WeaponType)weaponIndex;
        GameManager.Instance.GetPlayerData(out var playerData, index);
        playerData.selectedWeaponType = weaponType;
        customization.Init(playerData.customization);
        playerWeapon.SetWeapon(weaponData[weaponType], index);
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

    public void NextButton()
    {
        if (state == 2) return;
        
        if (state == 0)
        {
            colorPreviews.ForEach(item => item.DOFade(0f, 0.3f));
            hatPreviews.ForEach(item => item.DOFade(0f, 0.3f));
            nextButtonText.SetText("준비 완료");
            nextButton.DOColor(Color.red, 0.5f);
            weaponObj.gameObject.SetActive(true);
            playerWeapon.enabled = true;
            playerWeapon.SetWeapon(weaponData[WeaponType.Rifle], index);
            AnimateWeapons();
        }

        if (state == 1)
        {
            nextButton.gameObject.SetActive(false);
            if (otherPlayerSelectManager.state == 2)
            {
                startButton.SetActive(true);
                startButton.transform.localScale = Vector3.zero;
                startButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            }
        }
        state++;
    }
}
