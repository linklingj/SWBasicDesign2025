using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RewarkBlocks", menuName = "Scriptable Objects/RewarkBlocks")]
public class RewardBlocks : ScriptableObject
{
    [System.Serializable]
    public struct RarityBlockEntry
    {
        [GUIColor(nameof(GetRarityColor))] [EnumPaging]
        public Rarity rarity;
        private Color GetRarityColor => rarity.ToColor();

        [FoldoutGroup("$rarity"), LabelText("주사위 리스트")]
        public List<GameObject> blockList;
    }
    
    [Title("등급별 보상 블록 테이블", "- 에서 사용", TitleAlignments.Centered)]
    [TableList(AlwaysExpanded = true, ShowIndexLabels = false)]
    public List<RarityBlockEntry> DiceByRarity = new ();
    
    public GameObject GetBlockByRarity(Rarity rarity)
    {
        var entry = DiceByRarity.Find(e => e.rarity == rarity);
        if (entry.blockList == null || entry.blockList.Count == 0)
            return null;
        var block = entry.blockList[Random.Range(0, entry.blockList.Count)];
        return block;
    }
}
