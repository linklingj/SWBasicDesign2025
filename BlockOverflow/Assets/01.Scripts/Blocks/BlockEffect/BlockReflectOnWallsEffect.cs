using System;
using UnityEngine;

[Serializable]
public class BlockReflectOnWallEffect : IBlockEffect
{
    public string EffectDescription => "총알이 벽에 반사";

    public void ApplyEffect(PlayerStats stats)
    {
        stats.reflectOnWalls = true;
    }
}
