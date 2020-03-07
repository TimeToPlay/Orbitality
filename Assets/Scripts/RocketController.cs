using System;
using System.Collections.Generic;
using SO;
using UnityEngine;
using Zenject;

public class RocketController : MonoBehaviour, IPoolable<Vector3, Quaternion, RocketType, IMemoryPool>
{
    private List<SettingsSO.RocketSettings> _rocketSettings;
    private SettingsSO.RocketSettings _currentSettings;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Transform rendererTransform;
    private GameController _gameController;

    [Inject]
    void Construct(List<SettingsSO.RocketSettings> rocketSettings,
        GameController gameController)
    {
        _rocketSettings = rocketSettings;
        _gameController = gameController;
    }
    public void OnDespawned()
    {
        
    }

    public void OnSpawned(Vector3 initialPos, Quaternion initialRot, RocketType rocketType, IMemoryPool pool)
    {
        transform.position = initialPos;
        transform.rotation = Quaternion.Euler(0,0, initialRot.eulerAngles.z - 180);
        _currentSettings = _rocketSettings.Find(setting => setting.rocketType == rocketType);
        Configure(_currentSettings);
        _rigidbody2D.rotation = 0;
        _rigidbody2D.velocity = transform.up * 10;
    }

    private void FixedUpdate()
    {
        _rigidbody2D.AddForce(transform.up * (_currentSettings.acceleration * Time.fixedDeltaTime));
        foreach (var celestialObject in _gameController.GetAllCelestialObjects())
        {
            var celestialObjPos = celestialObject.transform.position;
            var position = transform.position;
            var dist = Vector3.Distance(celestialObjPos, position);
            var gravityPowerMagnitude = celestialObject.GetGravityModifier() / Mathf.Pow(dist, 2);
            var gravityPower = (celestialObjPos - position).normalized * gravityPowerMagnitude;
            _rigidbody2D.AddForce(gravityPower);
        }
    }

    private void Update()
    {
        var dir = _rigidbody2D.velocity.normalized;
        var angle = Mathf.Atan2(dir.y, dir.x);
        Debug.Log("angle " + angle);
        rendererTransform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg - 90);
    }

    private void Configure(SettingsSO.RocketSettings rocketSettings)
    {
    }

    public class Factory : PlaceholderFactory<Vector3, Quaternion, RocketType, RocketController>{}
}
