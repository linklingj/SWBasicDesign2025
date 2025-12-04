using System;
using UnityEngine;

[Serializable]
public class BlockBigBulletEffect : IBlockEffect
{
    public string EffectDescription => $"총알 크기 {bulletSizeMultiplier}배 증가";
    [SerializeField] private float bulletSizeMultiplier;

    public void ApplyEffect(PlayerStats stats)
    {
        stats.bulletSizeMultiplier *= bulletSizeMultiplier;
    }
}
