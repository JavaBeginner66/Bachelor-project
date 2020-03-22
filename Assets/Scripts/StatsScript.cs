using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsScript : MonoBehaviour
{
    public static StatsScript statsScript;
    [Header("Tags written out in strings")]
    public static readonly string ProjectileBaseDamageTag = "PlayerArrow";
    public static readonly string BulletHellDamageTag = "BulletHellObject";
    public static readonly string Boss1 = "Boss1";

    [Header("Float base stats")]
    public static readonly float ProjectileBaseDamage = 10f;
    public static readonly float BullethellDamage = 5f;

    private void Awake()
    {
        statsScript = this;
    }
}
