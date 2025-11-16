using UnityEngine;

public class BlockHealthEffect : IBlockEffect
{
    public string EffectDescription => $"체력 +{healthAmount}";
    [SerializeField] private int healthAmount;

    public void ApplyEffect(PlayerStats stats)
    {
        stats.healthIncrease += healthAmount;
    }      
}
