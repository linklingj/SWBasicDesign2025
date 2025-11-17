using System.Collections.Generic;
using UnityEngine;

public class BlocksUI : MonoBehaviour
{
    [SerializeField] TextUIElement titleText;
    [SerializeField] TextUIElement inventoryText;
    
    [SerializeField] GameObject upgradePanelPrefab;
    [SerializeField] GameObject panelParent;
    
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
}
