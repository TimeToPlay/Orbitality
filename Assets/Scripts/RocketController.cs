using System;
using System.Collections.Generic;
using Models;
using SO;
using UnityEngine;
using Zenject;

public class RocketController : MonoBehaviour, IPoolable<IMemoryPool>, IDamageReceiver, IDisposable
{
    private List<SettingsSO.RocketSettings> _rocketSettings;
    private SettingsSO.RocketSettings _currentSettings;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Transform _pivotTransform;
    [SerializeField] private Renderer _renderer;
    private GameController _gameController;
    private IMemoryPool _pool;
    private Action _onDisposeListener;
    private RocketModel rocketModel = new RocketModel();

    [Inject]
    void Construct(
        List<SettingsSO.RocketSettings> rocketSettings,
        GameController gameController
    )
    {
        _rocketSettings = rocketSettings;
        _gameController = gameController;
    }

    public void OnDespawned()
    {
        transform.position = new Vector3(-1000, -1000);
    }

    public void OnSpawned(IMemoryPool pool)
    {
        _pool = pool;
    }

    private void ChangeColor(Color color)
    {
        foreach (var rend in GetComponentsInChildren<MeshRenderer>())
        {
            rend.material.color = color;
        }
    }

    private void FixedUpdate()
    {
        _rigidbody2D.AddForce(transform.up * (_currentSettings.acceleration * Time.fixedDeltaTime));
        foreach (var celestialObject in _gameController.CelestialObjects)
        {
            var celestialObjPos = celestialObject.transform.position;
            var position = transform.position;
            var dist = Vector3.Distance(celestialObjPos, position);
            var gravityPowerMagnitude = celestialObject.GetGravityModifier() / Mathf.Pow(dist, 2);
            var gravityPower = (celestialObjPos - position).normalized * gravityPowerMagnitude;
            _rigidbody2D.AddForce(gravityPower);
        }
    }

    public RocketModel GetCurrentState()
    {
        return rocketModel;
    }

    private void Update()
    {
        var dir = _rigidbody2D.velocity.normalized;
        var angle = Mathf.Atan2(dir.y, dir.x);
        _pivotTransform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg - 90);
        rocketModel.UpdateState(transform, _rigidbody2D.velocity.magnitude);
        if (!_renderer.isVisible)
        {
            Dispose();
        }
    }

    public void Configure(RocketModel model)
    {
        rocketModel = model;
        transform.position = new Vector3(rocketModel.PosX, rocketModel.PosY);
        transform.rotation = Quaternion.Euler(0, 0, rocketModel.RotationZ);
        _currentSettings = _rocketSettings.Find(setting => setting.rocketType == rocketModel.RocketType);
        _rigidbody2D.rotation = 0;
        _rigidbody2D.velocity = transform.up * rocketModel.Velocity;
        switch (rocketModel.RocketType)
        {
            case RocketType.Normal:
                ChangeColor(Color.grey);
                break;
            case RocketType.Fast:
                ChangeColor(Color.yellow);
                break;
            case RocketType.Deadly:
                ChangeColor(Color.red);
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        var damageReceiver = other.gameObject.GetComponent<IDamageReceiver>();
        damageReceiver?.ReceiveDamage(_currentSettings.damage);
        if (isActiveAndEnabled) Dispose();
    }

    public class Factory : PlaceholderFactory<RocketController>
    {
    }

    public void ReceiveDamage(int damage)
    {
    }

    public void Dispose()
    {
        _onDisposeListener?.Invoke();
        _pool.Despawn(this);
    }
}