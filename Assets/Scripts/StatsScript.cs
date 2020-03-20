using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsScript : MonoBehaviour
{
    public static StatsScript statsScript;

    public static readonly float PlayerArrowDamage = 15f;
    public static readonly float BullethellDamage = 5f;

    private void Awake()
    {
        statsScript = this;
    }
}
