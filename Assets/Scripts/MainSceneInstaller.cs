using Controllers;
using Models;
using UnityEngine;
using Views;
using Zenject;

public class MainSceneInstaller : MonoInstaller
{
    [SerializeField] private RocketViewController _rocketPrefab;
    [SerializeField] private PlanetViewController _planetPrefab;
    [SerializeField] private Hud _hudPrefab;
    [SerializeField] private GameObject _sunGameObject;


    [SerializeField] private Transform RocketsRoot;
    [SerializeField] private Transform PlanetRoot;
    [SerializeField] private Transform HudRoot;

    public override void InstallBindings()
    {
        Container.Bind<LocalStorageHelper>().AsSingle().NonLazy();
        Container.Bind<LocalSaveController>().AsSingle().NonLazy();
        Container.Bind<AISystem>().AsSingle().NonLazy();
        Container.Bind<GameController>().AsSingle().NonLazy();
        Container.Bind<RocketAmmoPanel>().FromComponentInHierarchy().AsSingle();
        Container.Bind<Camera>().FromComponentInHierarchy().AsSingle();
        Container.Bind<MainMenuView>().FromComponentInHierarchy().AsSingle();
        Container.Bind<CelestialObject>().WithId("sun").FromComponentOn(_sunGameObject).AsSingle();

        Container.BindFactory<RocketViewController, RocketViewController.Factory>()
            .FromPoolableMemoryPool<RocketViewController, RocketControllerPool>(poolBinder => poolBinder
                .WithInitialSize(40)
                .FromComponentInNewPrefab(_rocketPrefab)
                .UnderTransform(RocketsRoot));

        Container.BindFactory<PlanetModel, PlanetViewController, PlanetViewController.Factory>()
            .FromPoolableMemoryPool<PlanetModel, PlanetViewController, PlanetControllerPool>(poolBinder => poolBinder
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

class RocketControllerPool : MonoPoolableMemoryPool<IMemoryPool, RocketViewController>
{
}

class PlanetControllerPool : MonoPoolableMemoryPool<PlanetModel, IMemoryPool, PlanetViewController>
{
}

class HudPool : MonoPoolableMemoryPool<IMemoryPool, Hud>
{
}