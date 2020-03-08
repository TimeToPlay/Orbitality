using System;
using DefaultNamespace;
using SO;
using UnityEngine;

public class RocketAmmoPanel: MonoBehaviour
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