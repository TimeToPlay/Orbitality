using System;
using System.Collections.Generic;
using SO;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

public class RocketController : MonoBehaviour, IPoolable<Vector3, Quaternion, RocketType, IMemoryPool>, IDamageReceiver
{
    private List<SettingsSO.RocketSettings> _rocketSettings;
    private SettingsSO.RocketSettings _currentSettings;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Transform _pivotTransform;
    [SerializeField] private Renderer _renderer;
    private GameController _gameController;
    private IMemoryPool _pool;

    [Inject]
    void Construct(List<SettingsSO.RocketSettings> rocketSettings,
        GameController gameController)
    {
        _rocketSettings = rocketSettings;
        _gameController = gameController;
    }

    private void Start()
    {
        
        this.OnCollisionEnterAsObservable().Subscribe(collision =>
        {
            Debug.Log("collision " + collision.collider.gameObject.name);
        }).AddTo(this);
    }

    public void OnDespawned()
    {
        transform.position = new Vector3(-1000,-1000);
    }

    public void OnSpawned(Vector3 initialPos, Quaternion initialRot, RocketType rocketType, IMemoryPool pool)
    {
        transform.position = initialPos;
        transform.rotation = Quaternion.Euler(0,0, initialRot.eulerAngles.z);
        _currentSettings = _rocketSettings.Find(setting => setting.rocketType == rocketType);
        Configure(_currentSettings);
        _rigidbody2D.rotation = 0;
        _rigidbody2D.velocity = transform.up * 27;
        _pool = pool;
        switch (rocketType)
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
            default:
                throw new ArgumentOutOfRangeException(nameof(rocketType), rocketType, null);
        }
    }

    private void ChangeColor(Color color)
    {
        foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
        {
           renderer.material.color = color;
        }
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
        _pivotTransform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg - 90);
        if (!_renderer.isVisible)
        {
            _pool.Despawn(this);            
        }
    }

    private void Configure(SettingsSO.RocketSettings rocketSettings)
    {
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        var damageReceiver = other.gameObject.GetComponent<IDamageReceiver>();
        damageReceiver?.ReceiveDamage(_currentSettings.damage);
        if (isActiveAndEnabled) _pool.Despawn(this);
    }

    public class Factory : PlaceholderFactory<Vector3, Quaternion, RocketType, RocketController>{}

    public void ReceiveDamage(int damage)
    {
    }
}
