using System;
using UnityEngine;

[Serializable]
public class BlockFinalRevengeEffect : IBlockEffect
{
    public string EffectDescription => $"플레이어의 체력이 30% 이하이면 공격력이 {damageMultiplier}배";
    [SerializeField] private float damageMultiplier;

    public void ApplyEffect(PlayerStats stats)
    {
        stats.finalRevengeMultiplier = damageMultiplier;
    }
}
