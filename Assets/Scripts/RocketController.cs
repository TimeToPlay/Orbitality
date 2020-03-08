using System;
using System.Collections.Generic;
using Models;
using SO;
using UniRx;
using UniRx.Triggers;
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
    private RocketState _rocketState = new RocketState();

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

    public void OnSpawned(IMemoryPool pool)
    {
        _pool = pool;
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

    public RocketState GetCurrentState()
    {
        return _rocketState;
    }

    private void Update()
    {
        var dir = _rigidbody2D.velocity.normalized;
        var angle = Mathf.Atan2(dir.y, dir.x);
        _pivotTransform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg - 90);
        _rocketState.posX = transform.position.x;
        _rocketState.posY = transform.position.y;
        _rocketState.velocity = _rigidbody2D.velocity.magnitude;
        _rocketState.rotationZ = transform.rotation.eulerAngles.z;
        if (!_renderer.isVisible)
        {
           Dispose();         
        }
    }

    public void Configure(RocketState state)
    {
        _rocketState = state;
        transform.position = new Vector3(_rocketState.posX, _rocketState.posY);
        transform.rotation = Quaternion.Euler(0,0, _rocketState.rotationZ);
        _currentSettings = _rocketSettings.Find(setting => setting.rocketType == _rocketState.GetRocketType());
        _rigidbody2D.rotation = 0;
        _rigidbody2D.velocity = transform.up * _rocketState.velocity;
        switch (_rocketState.GetRocketType())
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

    public void SetOnDisposeListener(Action onDispose)
    {
        _onDisposeListener = onDispose;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        var damageReceiver = other.gameObject.GetComponent<IDamageReceiver>();
        damageReceiver?.ReceiveDamage(_currentSettings.damage);
        if (isActiveAndEnabled) Dispose();
    }

    public class Factory : PlaceholderFactory<RocketController>{}

    public void ReceiveDamage(int damage)
    {
    }

    public void Dispose()
    {
        _onDisposeListener?.Invoke();
        _pool.Despawn(this);
    }
}
