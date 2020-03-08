using System;
using System.Collections.Generic;
using SO;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class AISystem
{
    private CelestialObject _sun;
    private List<PlanetController> _enemies;

    public AISystem(CelestialObject sun, List<PlanetController> enemies)
    {
        _sun = sun;
        _enemies = enemies;
    }

    public void MakeDecisions()
    {
        foreach (var enemy in _enemies)
        {
            var trajectory = CalculateForecastTrajectory(_sun, enemy);
            for (int i = 1; i < trajectory.Length; i++)
            {
                var hit2D = Physics2D.Raycast(trajectory[i-1], (trajectory[i] - trajectory[i-1]).normalized);
                if (hit2D.collider)
                {
                    if (hit2D.collider.gameObject.CompareTag("Sun"))
                    {
                        break;
                    }
                    if (hit2D.collider.gameObject.CompareTag("Planet"))
                    {
                        ChooseRocketType(enemy, hit2D.collider.transform);
                        Observable.TimerFrame(Random.Range(0,5)).Subscribe(_ => enemy.Shoot());
                    }
                }
            }
        }
    }

    private void ChooseRocketType(PlanetController enemy, Transform target)
    {
        var dist = Vector3.Distance(enemy.transform.position, target.position);
        if (dist > enemy.GetOrbit())
        {
            enemy.SetRocketType(RocketType.Fast);
        } else if (dist < 15)
        {
            enemy.SetRocketType(RocketType.Deadly);
        }
        else
        {
            enemy.SetRocketType(RocketType.Normal);
        }
        SwitchToNextNonEmptyAmmo(enemy);
    }
        
    private void SwitchToNextNonEmptyAmmo(PlanetController planet)
    {
        if (planet.GetCurrentAmmo() > 0) return;
        var rocketTypeInt = (int) planet.GetCurrentRocketSettings().rocketType;
        for (int i = 0; i < Enum.GetValues(typeof(RocketType)).Length; i++)
        {
            rocketTypeInt++;
            if (rocketTypeInt >= Enum.GetValues(typeof(RocketType)).Length)
            {
                rocketTypeInt = 0;
            }
            planet.SetRocketType((RocketType) rocketTypeInt);
            if (planet.GetCurrentAmmo() <= 0)
            {
                continue;
            }
            break;
        }
    }

    public Vector3[] CalculateForecastTrajectory(CelestialObject sun, PlanetController planetController)
    {
        Vector3[] positions = new Vector3[100];
        positions[0] = planetController.GetMuzzle().position;
        positions[1] = planetController.GetMuzzle().position + planetController.transform.up;
        var time = 0f;
        for (int i = 2; i < 100; i++)
        {
            var dir = (positions[i - 1] - positions[i - 2]).normalized;
            var sunPosition = sun.transform.position;
            var gravityPowerMagnitude = sun.GetGravityModifier() / Mathf.Pow(Vector3.Distance(sunPosition, positions[i-1]), 2);
            time += 0.016f;
            var speed = PlanetController.ROCKET_START_VELOCITY + planetController.GetCurrentRocketSettings().acceleration * time;
            var gravityPower = (sunPosition - positions[i-1]).normalized * gravityPowerMagnitude;

            var delta = (dir * (speed) + gravityPower) * 0.016f;
            positions[i] = positions[i - 1] + delta;
        }
        return positions;
    }
}