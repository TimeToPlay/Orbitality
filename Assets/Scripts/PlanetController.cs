using System.Collections;
using System.Collections.Generic;
using SO;
using UnityEngine;
using Zenject;

public class PlanetController : MonoBehaviour, IPoolable<SettingsSO.PlanetSettings, IMemoryPool>
{
    private SettingsSO.PlanetSettings currentSettings;
    
    private float currentAngleToSun = 0;
    private RocketController.Factory _rocketFactory;
    [SerializeField] private Transform muzzleTransform;

    [Inject]
    void Construct(RocketController.Factory rocketFactory)
    {
        _rocketFactory = rocketFactory;
    }
    void Start()
    {
        
    }

    void Update()
    {
        var positionX = currentSettings.orbitRadius * Mathf.Cos(currentAngleToSun);
        var positionY = currentSettings.orbitRadius * Mathf.Sin(currentAngleToSun);
        currentAngleToSun += currentSettings.solarAngularVelocity * Time.deltaTime;
        transform.position = new Vector3(positionX, positionY, transform.position.z);
        var selfRotationDelta = currentSettings.selfRotationVelocity * Time.deltaTime;
        transform.Rotate(Vector3.forward, currentSettings.clockwise ? selfRotationDelta : -selfRotationDelta);
    }

    public void Shoot()
    {
        _rocketFactory.Create(muzzleTransform.position, muzzleTransform.rotation, RocketType.Fast);
    }
    public class Factory : PlaceholderFactory<SettingsSO.PlanetSettings, PlanetController>{}

    public void OnDespawned()
    {
        
    }

    public void OnSpawned(SettingsSO.PlanetSettings settings, IMemoryPool pool)
    {
        currentSettings = settings;
        transform.localScale = new Vector3(settings.planetScale, settings.planetScale,settings.planetScale);
        currentAngleToSun = Random.Range(0, Mathf.PI * 2);
    }
}
