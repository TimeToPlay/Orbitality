using Models;
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
        Container.Bind<LocalStorageHelper>().AsSingle().NonLazy();
        Container.Bind<LocalSaveController>().AsSingle().NonLazy();
        Container.Bind<GameController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<RocketAmmoPanel>().FromComponentInHierarchy().AsSingle();
        Container.Bind<Camera>().FromComponentInHierarchy().AsSingle();

        Container.BindFactory<RocketController, RocketController.Factory>()
            .FromPoolableMemoryPool<RocketController, RocketControllerPool>(poolBinder => poolBinder
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
class RocketControllerPool : MonoPoolableMemoryPool<IMemoryPool, RocketController>{}
class PlanetControllerPool : MonoPoolableMemoryPool<PlanetState, IMemoryPool, PlanetController>{}
class HudPool : MonoPoolableMemoryPool<IMemoryPool, Hud>{}
