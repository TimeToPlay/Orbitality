using System.Collections;
using System.Collections.Generic;
using SO;
using UnityEngine;
using Zenject;

public class MainSceneInstaller : MonoInstaller
{
    [SerializeField] private RocketController _rocketPrefab;
    [SerializeField] private PlanetController _planetPrefab;
    [SerializeField] private Hud _hudPrefab;
    

    [SerializeField] private Transform RocketsRoot;
    [SerializeField] private Transform PlanetRoot;
    [SerializeField] private Transform HudRoot;
    public override void InstallBindings()
    {
        Container.Bind<GameController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<RocketAmmoPanel>().FromComponentInHierarchy().AsSingle();

        Container.BindFactory<Vector3, Quaternion, RocketType, RocketController, RocketController.Factory>()
            .FromPoolableMemoryPool<Vector3, Quaternion, RocketType, RocketController, RocketControllerPool>(poolBinder => poolBinder
                .WithInitialSize(40)
                .FromComponentInNewPrefab(_rocketPrefab)
                .UnderTransform(RocketsRoot));
        
        Container.BindFactory<PlanetState, PlanetController, PlanetController.Factory>()
            .FromPoolableMemoryPool<PlanetState, PlanetController, PlanetControllerPool>(poolBinder => poolBinder
                .WithInitialSize(10)
                .FromComponentInNewPrefab(_planetPrefab)
                .UnderTransform(PlanetRoot));
        
        Container.BindFactory<Hud, Hud.Factory>()
            .FromPoolableMemoryPool<Hud, HudPool>(poolBinder => poolBinder
                .WithInitialSize(10)
                .FromComponentInNewPrefab(_hudPrefab)
                .UnderTransform(HudRoot));
    }
}
class RocketControllerPool : MonoPoolableMemoryPool<Vector3, Quaternion, RocketType, IMemoryPool, RocketController>{}
class PlanetControllerPool : MonoPoolableMemoryPool<PlanetState, IMemoryPool, PlanetController>{}
class HudPool : MonoPoolableMemoryPool<IMemoryPool, Hud>{}
