using System.Collections;
using System.Collections.Generic;
using SO;
using UnityEngine;
using Zenject;

public class MainSceneInstaller : MonoInstaller
{
    [SerializeField] private RocketController _rocketPrefab;
    [SerializeField] private PlanetController _planetPrefab;
    

    [SerializeField] private Transform RocketsRoot;
    [SerializeField] private Transform PlanetRoot;
    public override void InstallBindings()
    {
        Container.Bind<GameController>().FromComponentInHierarchy().AsSingle();

        Container.BindFactory<Vector3, Quaternion, RocketType, RocketController, RocketController.Factory>()
            .FromPoolableMemoryPool<Vector3, Quaternion, RocketType, RocketController, RocketControllerPool>(poolBinder => poolBinder
                .WithInitialSize(40)
                .FromComponentInNewPrefab(_rocketPrefab)
                .UnderTransform(RocketsRoot));
        
        Container.BindFactory<SettingsSO.PlanetSettings, PlanetController, PlanetController.Factory>()
            .FromPoolableMemoryPool<SettingsSO.PlanetSettings, PlanetController, PlanetControllerPool>(poolBinder => poolBinder
                .WithInitialSize(10)
                .FromComponentInNewPrefab(_planetPrefab)
                .UnderTransform(PlanetRoot));
    }
}
class RocketControllerPool : MonoPoolableMemoryPool<Vector3, Quaternion, RocketType, IMemoryPool, RocketController>{}
class PlanetControllerPool : MonoPoolableMemoryPool<SettingsSO.PlanetSettings, IMemoryPool, PlanetController>{}
