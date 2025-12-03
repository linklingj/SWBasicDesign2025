using System;
using UnityEngine;

[Serializable]
public class PlayerStats {
    public int healthIncrease = 0;
    public int damageIncrease = 0;
    public float fireRateIncrease = 0f;
    public float speedIncrease = 0f;
    public int jumpIncrease = 0;
    public int extraJumpCount = 0;
    public float bulletSizeMultiplier = 1f;
    public float finalRevengeMultiplier = 1;
    public bool reflectOnWalls = false;
}