using System.Collections;
using System.Collections.Generic;
using SO;
using UnityEngine;
using Zenject;

public class MainSceneInstaller : MonoInstaller
{
    [SerializeField]
    private RocketController _rocketPrefab;
    

    [SerializeField]
    private Transform RocketsRoot;
    public override void InstallBindings()
    {
        Container.Bind<GameController>().FromComponentInHierarchy().AsSingle();

        Container.BindFactory<Vector3, Quaternion, RocketType, RocketController, RocketController.Factory>()
            .FromPoolableMemoryPool<Vector3, Quaternion, RocketType, RocketController, MinerPresenterPool>(poolBinder => poolBinder
                .WithInitialSize(40)
                .FromComponentInNewPrefab(_rocketPrefab)
                .UnderTransform(RocketsRoot));
    }
}
class MinerPresenterPool : MonoPoolableMemoryPool<Vector3, Quaternion, RocketType, IMemoryPool, RocketController>
{
}
