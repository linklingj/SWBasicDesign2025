using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RewardBlockWeightTable", menuName = "Scriptable Objects/RewardBlockWeightTable")]
public class RewardBlockWeightTable : ScriptableObject {
    [Serializable]
    public class RankWeightTable {
        public List<RankWeight> table = new List<RankWeight>();
    }
    
    [Serializable]
    public class RankWeight
    {
        [TableColumnWidth(120, false)]
        [LabelText("등급")]
        [EnumPaging][GUIColor(nameof(GetRarityColor))]
        public Rarity Rank;
        private Color GetRarityColor => Rank.ToColor();

        [TableColumnWidth(140, false)]
        [LabelText("확률(%)")]
        [Tooltip("각 Stage에 대한 등급별 가중치 목록, 확률의 개념이지만 합이 100%가 아니어도 됨")]
        [MinValue(0), SuffixLabel("%", overlay: true)]
        public float Weight;   // % 개념이지만 합 100이 아니어도 됨(가중치로 해석)
    }

    [Title("주사위별 등급 확률 테이블", "- 에서 사용", TitleAlignments.Centered)]
    [SerializeField]
    [TableList(ShowIndexLabels = false, AlwaysExpanded = true, DrawScrollView = false)]
    private List<RankWeightTable> table = new();

    public Rarity GetBlockRarity(int blockNum, List<RankWeight> additive = null)
    {
        if (blockNum < 0 || blockNum >= table.Count) return Rarity.Common;

        var rows = table[blockNum];
        
        // 총 가중치 합 계산
        float totalWeight = 0f;
        foreach (var row in rows.table)
            totalWeight += row.Weight;

        //랜덤 값 생성 및 가중치에 따라 등급 선택
        float rand = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;
        foreach (var row in rows.table)
        {
            cumulative += row.Weight;
            if (rand <= cumulative)
                return row.Rank;
        }
        
        // Fallback (should not happen)
        return rows.table[0].Rank;
    }
}
