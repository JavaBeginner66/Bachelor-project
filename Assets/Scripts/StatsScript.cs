using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsScript : MonoBehaviour
{
    public static StatsScript statsScript;
    [Header("Tags written out in strings")]
    public static readonly string PlayerProjectile = "PlayerArrow";
    public static readonly string BulletHellDamageTag = "BulletHellObject";
    public static readonly string Boss = "Boss1";

    [Header("Float base stats")]
    public static readonly float ProjectileBaseDamage = 0f;

    private void Awake()
    {
        statsScript = this;
    }
}
