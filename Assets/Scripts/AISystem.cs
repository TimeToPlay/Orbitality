using System;
using System.Collections.Generic;
using Controllers;
using SO;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

/// <summary>
/// Decides what type of ammo use, and calculates approximately rocket trajectories 
/// </summary>
public class AISystem
{
    private const double DISTANCE_TO_SHOT_DEADLY_WEAPON = 15;
    private const int MAX_FRAME_TRESHOLD_FOR_RANDOM_SHOOT = 5;
    private const int TRAJECTORY_POINT_COUNT = 100;
    [Inject(Id = "sun")] private CelestialObject _sun;
    private List<PlanetViewController> _enemies;
    Vector3[] _trajectoryPoints = new Vector3[TRAJECTORY_POINT_COUNT];

    public void RegisterEnemies(List<PlanetViewController> enemies)
    {
        _enemies = enemies;
    }

    public void MakeDecisions()
    {
        foreach (var enemy in _enemies)
        {
            var trajectory = CalculateForecastTrajectory(_sun, enemy);
            for (int i = 1; i < trajectory.Length; i++)
            {
                var hit2D = Physics2D.Raycast(trajectory[i - 1], (trajectory[i] - trajectory[i - 1]).normalized);
                if (hit2D.collider)
                {
                    if (hit2D.collider.gameObject.CompareTag("Sun"))
                    {
                        break;
                    }

                    if (hit2D.collider.gameObject.CompareTag("Planet"))
                    {
                        ChooseRocketType(enemy, hit2D.collider.transform);
                        Observable.TimerFrame(Random.Range(0, MAX_FRAME_TRESHOLD_FOR_RANDOM_SHOOT))
                            .Subscribe(_ => enemy.Shoot());
                    }
                }
            }
        }
    }

    private void ChooseRocketType(PlanetViewController enemy, Transform target)
    {
        var dist = Vector3.Distance(enemy.transform.position, target.position);
        if (dist > enemy.GetOrbit())
        {
            enemy.SetRocketType(RocketType.Fast);
        }
        else if (dist < DISTANCE_TO_SHOT_DEADLY_WEAPON)
        {
            enemy.SetRocketType(RocketType.Deadly);
        }
        else
        {
            enemy.SetRocketType(RocketType.Normal);
        }

        SwitchToNextNonEmptyAmmo(enemy);
    }

    private void SwitchToNextNonEmptyAmmo(PlanetViewController planet)
    {
        if (planet.GetCurrentAmmo() > 0) return;
        var rocketTypeInt = (int) planet.GetCurrentRocketSettings().rocketType;
        for (var i = 0; i < Enum.GetValues(typeof(RocketType)).Length; i++)
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

    #region BLACK MAGIC FUCKERY

    /// <summary>
    /// Calculates approximately trajectory for rockets
    /// </summary>
    /// <param name="sun"></param>
    /// <param name="planetController"></param>
    /// <returns></returns>
    private Vector3[] CalculateForecastTrajectory(CelestialObject sun, PlanetViewController planetController)
    {
        _trajectoryPoints[0] = planetController.GetMuzzle().position;
        _trajectoryPoints[1] = planetController.GetMuzzle().position + planetController.transform.up;
        var time = 0f;
        var expectedDeltaTime = 0.016f;
        for (int i = 2; i < TRAJECTORY_POINT_COUNT; i++)
        {
            var dir = (_trajectoryPoints[i - 1] - _trajectoryPoints[i - 2]).normalized;
            var sunPosition = sun.transform.position;
            var gravityPowerMagnitude =
                sun.GetGravityModifier() / Mathf.Pow(Vector3.Distance(sunPosition, _trajectoryPoints[i - 1]), 2);
            time += expectedDeltaTime;
            var speed = PlanetViewController.ROCKET_START_VELOCITY +
                        planetController.GetCurrentRocketSettings().acceleration * time;
            var gravityPower = (sunPosition - _trajectoryPoints[i - 1]).normalized * gravityPowerMagnitude;

            var delta = (dir * (speed) + gravityPower) * expectedDeltaTime;
            _trajectoryPoints[i] = _trajectoryPoints[i - 1] + delta;
        }

        return _trajectoryPoints;
    }

    #endregion
}