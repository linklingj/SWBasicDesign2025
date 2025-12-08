using System;
using UnityEngine;

[Serializable]
public class BlockUltDamage : IBlockEffect
{
    public string EffectDescription => $"궁극기 데미지 +{ultDamage}";
    [SerializeField] private int ultDamage;

    public void ApplyEffect(PlayerStats stats)
    {
        stats.ultDamageIncrease += ultDamage;
    }
}
