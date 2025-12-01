using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BlocksUI : MonoBehaviour
{
    [SerializeField] TextUIElement titleText;
    [SerializeField] TextUIElement inventoryText;
    
    [SerializeField] GameObject upgradePanelPrefab;
    [SerializeField] GameObject panelParent;

    [SerializeField] BlockAnimatorData blockAnimatorData;
    
    [Serializable]
    class RewardUIElements
    {
        public RectTransform rewardTransform;
        public TextUIElement rewardText;
    }
    [SerializeField] List<RewardUIElements> rewards;

    [SerializeField] private GameObject blockAppearEffect;
    
    
    List<GameObject> upgradePanels = new List<GameObject>();
    
    public void SetTitleText(int n)
    {
        titleText.SetText($"플레이어 {n} 업그레이드 블록 선택");
        inventoryText.SetText($"P{n} 인벤토리");
    }
    
    public void SetUpgradeText(List<Block> blocks)
    {
        foreach (var p in upgradePanels)
            Destroy(p);
        
        foreach (var b in blocks)
        {
            GameObject o = Instantiate(upgradePanelPrefab, panelParent.transform);
            upgradePanels.Add(o);
            o.GetComponentInChildren<TextUIElement>().SetText(b.blockEffect.EffectDescription);
        }
    }

    public void SetRewardText(int index, Block block)
    {
        Vector3 genPos = blockAnimatorData.showPosition + new Vector3(blockAnimatorData.generateSpread*(index-1),0,0);
        ObjectPoolManager.Instance.Get(blockAppearEffect, genPos);
        rewards[index].rewardTransform.transform.position = genPos + new Vector3(0,-1.5f,0);
        rewards[index].rewardTransform.gameObject.SetActive(true);
        rewards[index].rewardTransform.localScale = Vector3.zero;
        rewards[index].rewardTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        rewards[index].rewardText.SetText(block.blockEffect.EffectDescription);
    }
}
