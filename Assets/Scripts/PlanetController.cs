using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using SO;
using UnityEngine;
using Utils;
using Zenject;

public class PlanetController : CelestialObject, IPoolable<PlanetState, IMemoryPool>, IDisposable
{
    private PlanetState _planetState;
    private float _currentAngleToSun;
    private RocketController.Factory _rocketFactory;
    [SerializeField] private Transform muzzleTransform;
    private IMemoryPool _pool;
    private Hud.Factory _hudFactory;
    private Hud _currentHud;
    private bool _isCooldown;
    private float _cooldown;
    private SettingsSO.RocketSettings _currentRocketSettings;
    private List<SettingsSO.RocketSettings> _rocketSettingsList;
    private bool _isDead;
    public const float ROCKET_START_VELOCITY = 27;
    public Action onDieEvent;

    public bool IsDead => _isDead;

    [Inject]
    void Construct(RocketController.Factory rocketFactory,
        Hud.Factory hudFactory,
        List<SettingsSO.RocketSettings> rocketSettingList
        )
    {
        _rocketFactory = rocketFactory;
        _hudFactory = hudFactory;
        _rocketSettingsList = rocketSettingList;
        _currentRocketSettings = rocketSettingList[0];
    }
    void Update()
    {
        var positionX = _planetState.settings.orbitRadius * Mathf.Cos(_currentAngleToSun);
        var positionY = _planetState.settings.orbitRadius * Mathf.Sin(_currentAngleToSun);
        var sign = _planetState.settings.clockwise ? 1 : -1;
        _currentAngleToSun += _planetState.settings.solarAngularVelocity * Time.deltaTime * sign;
        _planetState.currentAngleToSun = _currentAngleToSun;
        transform.position = new Vector3(positionX, positionY, transform.position.z);
        var selfRotationDelta = _planetState.settings.selfRotationVelocity * Time.deltaTime;
        transform.Rotate(Vector3.forward, _planetState.settings.clockwise ? selfRotationDelta : -selfRotationDelta);
        _currentHud.TransformWorldPosition(transform.position);
    }

    public void Shoot()
    {
        if (_isCooldown || _isDead || _planetState.rocketAmmo[_currentRocketSettings.rocketType] <= 0) return;
        var rocket = _rocketFactory.Create();
        var state = rocket.GetCurrentState();
        state.rocketType = _currentRocketSettings.rocketType.ToString();
        state.posX = muzzleTransform.position.x;
        state.posY = muzzleTransform.position.y;
        state.rotationZ = transform.rotation.eulerAngles.z;
        state.velocity = ROCKET_START_VELOCITY;
        rocket.Configure(state);
        _planetState.rocketAmmo[_currentRocketSettings.rocketType]--;
        StartCoroutine(StartCooldown());
    }
    public int GetCurrentAmmo()
    {
        return _planetState.rocketAmmo[_currentRocketSettings.rocketType];
    }

    private IEnumerator StartCooldown()
    {
        _cooldown = _currentRocketSettings.cooldown;
        _isCooldown = true;
        while (_cooldown >= 0)
        {
            yield return null;
            _cooldown -= Time.deltaTime;
            _currentHud.SetCooldown(_cooldown / _currentRocketSettings.cooldown);
        }

        _isCooldown = false;
        _cooldown = 0;
        _currentHud.SetCooldown(_cooldown / _currentRocketSettings.cooldown);
    }

    public class Factory : PlaceholderFactory<PlanetState, PlanetController>{}

    public void OnDespawned()
    {
        transform.position = new Vector3(-1000,-1000,0);
    }

    public void OnSpawned(PlanetState state, IMemoryPool pool)
    {
        _planetState = state;
        _pool = pool;
        _currentAngleToSun = state.currentAngleToSun;
        transform.localScale = new Vector3(state.settings.planetScale, state.settings.planetScale,state.settings.planetScale);
        _currentHud = _hudFactory.Create();
        _currentRocketSettings = _rocketSettingsList[0];
        _isDead = false;
        _planetState.nickname = state.nickname;
        GetComponent<MeshRenderer>().material.color = state.color;
        if (string.IsNullOrEmpty(state.nickname))
        {
            var nickname = NickGenerator.GetRandomNickname();
            _planetState.nickname = nickname;
        }
        _currentHud.Configure(_planetState.nickname, state.isPlayer, state.hp, state.hp);
    }

    public override float GetGravityModifier()
    {
        return transform.localScale.x * 210;
    }

    public override void ReceiveDamage(int damage)
    {
        _planetState.hp -= damage;
        _currentHud.SetNewHp(_planetState.hp);
        if (_planetState.hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;
        onDieEvent?.Invoke();
        Dispose();
    }

    public Transform GetMuzzle()
    {
        return muzzleTransform;
    }

    public SettingsSO.RocketSettings GetCurrentRocketSettings()
    {
        return _currentRocketSettings;
    }

    public void SetRocketType(RocketType nextRocketType)
    {
        _currentRocketSettings = _rocketSettingsList.Find(rocket => rocket.rocketType == nextRocketType);
    }

    public double GetOrbit()
    {
        return _planetState.settings.orbitRadius;
    }
    public void Dispose()
    {
        if (!isActiveAndEnabled) return;
        StopAllCoroutines();
        _isCooldown = false;
        _currentHud.Despawn();
        _pool.Despawn(this);
    }
}




