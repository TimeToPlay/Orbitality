using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace DefaultNamespace
{
    public class AISystem
    {
        private CelestialObject _sun;
        private PlanetController _player;
        private List<PlanetController> _enemies;

        public AISystem(CelestialObject sun, PlanetController player, List<PlanetController> enemies)
        {
            _sun = sun;
            _player = player;
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
                    if (hit2D.collider != null)
                    {
                        if (hit2D.collider.gameObject.CompareTag("Sun"))
                        {
                            break;
                        }
                        if (hit2D.collider.gameObject.CompareTag("Planet"))
                        {
                            Observable.TimerFrame(Random.Range(0,5)).Subscribe(_ => enemy.Shoot());
                        }
                    }
                }
            }
        }
        
        public Vector3[] CalculateForecastTrajectory(CelestialObject sun, PlanetController planetController)
        {
            Vector3[] positions = new Vector3[100];
            positions[0] = planetController.GetMuzzle().position;
            positions[1] = planetController.GetMuzzle().position + planetController.transform.up * 5;
            var lasPower = Vector3.zero;
            var speed = 0f;
            var time = 0f;
            for (int i = 2; i < 100; i++)
            {
                var dir = (positions[i - 1] - positions[i - 2]).normalized;
                var gravityPowerMagnitude = sun.GetGravityModifier() / Mathf.Pow(Vector3.Distance(sun.transform.position, positions[i-1]), 2);
                time += 0.016f;
                speed = 27 + planetController.GetCurrentRocketSettings().acceleration * time;
                var gravityPower = (sun.transform.position - positions[i-1]).normalized * gravityPowerMagnitude;

                var delta = (dir * (speed) + gravityPower) * 0.016f;
                lasPower = delta;
                positions[i] = positions[i - 1] + delta;
            }
            return positions;
        }
    }
}