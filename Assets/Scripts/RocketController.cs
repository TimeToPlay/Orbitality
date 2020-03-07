using System.Collections.Generic;
using SO;
using UnityEngine;
using Zenject;

public class RocketController : MonoBehaviour, IPoolable<Vector3, Quaternion, RocketType, IMemoryPool>
{
    private List<SettingsSO.RocketSettings> _rocketSettings;
    private SettingsSO.RocketSettings _currentSettings;
    [Inject]
    void Construct(List<SettingsSO.RocketSettings> rocketSettings)
    {
        _rocketSettings = rocketSettings;
    }
    public void OnDespawned()
    {
        
    }

    public void OnSpawned(Vector3 initialPos, Quaternion initialRot, RocketType rocketType, IMemoryPool pool)
    {
        transform.position = initialPos;
        transform.rotation = initialRot;
        _currentSettings = _rocketSettings.Find(setting => setting.rocketType == rocketType);
        Configure(_currentSettings);
    }

    private void Configure(SettingsSO.RocketSettings rocketSettings)
    {
    }

    public class Factory : PlaceholderFactory<Vector3, Quaternion, RocketType, RocketController>{}
}
