using System;
using UnityEngine;

[Serializable]
public class BlockFireRateEffect : IBlockEffect
{
    public string EffectDescription => $"연사속도 +{fireRateAmount}";
    [SerializeField] private int fireRateAmount = 1;

    public void ApplyEffect(PlayerStats stats)
    {
        stats.fireRateIncrease += fireRateAmount;
    }
}
