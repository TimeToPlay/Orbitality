using System.Collections.Generic;
using SO;
using UnityEngine;
using Zenject;

public class RocketController : MonoBehaviour, IPoolable<Vector3, Quaternion, RocketType, IMemoryPool>
{
    private List<SettingsSO.RocketSetting> _rocketSettings;
    private SettingsSO.RocketSetting currentSetting;
    [Inject]
    void Construct(List<SettingsSO.RocketSetting> rocketSettings)
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
        currentSetting = _rocketSettings.Find(setting => setting.rocketType == rocketType);
        Configure(currentSetting);
    }

    private void Configure(SettingsSO.RocketSetting rocketSetting)
    {
        throw new System.NotImplementedException();
    }

    public class Factory : PlaceholderFactory<Vector3, Quaternion, RocketType, RocketController>{}
}
