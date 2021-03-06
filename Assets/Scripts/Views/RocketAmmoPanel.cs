﻿using System;
using SO;
using UnityEngine;

/// <summary>
/// View to present player's ammo count
/// </summary>
public class RocketAmmoPanel : MonoBehaviour
{
    [SerializeField] private RocketAmmoUI normalRocket;
    [SerializeField] private RocketAmmoUI fastRocket;
    [SerializeField] private RocketAmmoUI deadlyRocket;

    public void SetCurrentRocketType(RocketType type)
    {
        normalRocket.SelectEnabled(false);
        fastRocket.SelectEnabled(false);
        deadlyRocket.SelectEnabled(false);
        switch (type)
        {
            case RocketType.Normal:
                normalRocket.SelectEnabled(true);
                break;
            case RocketType.Fast:
                fastRocket.SelectEnabled(true);
                break;
            case RocketType.Deadly:
                deadlyRocket.SelectEnabled(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void SetRocketAmmo(RocketType type, int ammo)
    {
        switch (type)
        {
            case RocketType.Normal:
                normalRocket.SetAmmo(ammo);
                break;
            case RocketType.Fast:
                fastRocket.SetAmmo(ammo);
                break;
            case RocketType.Deadly:
                deadlyRocket.SetAmmo(ammo);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}