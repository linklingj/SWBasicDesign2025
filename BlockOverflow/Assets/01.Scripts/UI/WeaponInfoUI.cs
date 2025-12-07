using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WeaponInfoUI : SerializedMonoBehaviour {
    [SerializeField] private Dictionary<WeaponType, WeaponData> weaponDatas;
    [SerializeField] private Dictionary<WeaponType, BulletData> bulletDatas;
    
    [SerializeField] Slider damageSlider;
    [SerializeField] Slider fireRateSlider;
    [SerializeField] Slider rangeSlider;
    [SerializeField] Slider bulletSpeedSlider;
    
    [SerializeField] GameObject weaponInfoPanel;

    private void Start()
    {
        weaponInfoPanel.SetActive(false);
    }
    

    public void SetWeaponInfo(WeaponType weaponType)
    {
        weaponInfoPanel.SetActive(true);
        damageSlider.DOValue(weaponDatas[weaponType].damage / 35f, 0.3f).SetEase(Ease.OutQuad);
        fireRateSlider.DOValue(weaponDatas[weaponType].fireRate / 10f, 0.3f).SetEase(Ease.OutQuad);
        rangeSlider.DOValue(bulletDatas[weaponType].range / 30f, 0.3f).SetEase(Ease.OutQuad);
        bulletSpeedSlider.DOValue(bulletDatas[weaponType].bulletSpeed / 50f, 0.3f).SetEase(Ease.OutQuad);
    }
    
    public void ShowWeaponInfo(WeaponType weaponType)
    {
        SetWeaponInfo(weaponType);
        weaponInfoPanel.SetActive(true);
    }
    
    public void HideWeaponInfo()
    {
        weaponInfoPanel.SetActive(false);
    }
}
