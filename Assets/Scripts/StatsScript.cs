﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Script contains static readonly variables for more robust code
 */
public class StatsScript : MonoBehaviour
{
    public static StatsScript statsScript;
    [Header("Tags written out in strings")]
    public static readonly string PlayerProjectile = "PlayerArrow";
    public static readonly string BulletHellDamageTag = "BulletHellObject";
    public static readonly string TargetCircle = "TargetCircle";
    public static readonly string QuarterCircle = "QuarterCircle";
    public static readonly string RotatingWalls = "RotatingWalls";
    public static readonly string Boss = "Boss1";

    [Header("Float base stats")]
    public static readonly float ProjectileBaseDamage = 0f;

    private void Awake()
    {
        statsScript = this;
    }
}
