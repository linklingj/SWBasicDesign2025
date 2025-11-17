using System;
using UnityEngine;

[Serializable]
public class BlockDamageEffect : IBlockEffect
{
    public string EffectDescription => $"공격력 +{damageAmount}";
    [SerializeField] private int damageAmount;

    public void ApplyEffect(PlayerStats stats)
    {
        stats.damageIncrease += damageAmount;
    }
}
