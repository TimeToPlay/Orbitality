using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using SO;
using UnityEngine;
using Utils;
using Zenject;

public class PlanetController : CelestialObject, IPoolable<PlanetModel, IMemoryPool>, IDisposable
{
    private PlanetModel planetModel;
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
    private SettingsSO.GameSettings _gameSettings;

    public bool IsDead => _isDead;

    [Inject]
    void Construct(RocketController.Factory rocketFactory,
        Hud.Factory hudFactory,
        List<SettingsSO.RocketSettings> rocketSettingList,
        SettingsSO.GameSettings gameSettings
    )
    {
        _rocketFactory = rocketFactory;
        _hudFactory = hudFactory;
        _rocketSettingsList = rocketSettingList;
        _currentRocketSettings = rocketSettingList[0];
        _gameSettings = gameSettings;
    }

    void Update()
    {
        var positionX = planetModel.Settings.orbitRadius * Mathf.Cos(_currentAngleToSun);
        var positionY = planetModel.Settings.orbitRadius * Mathf.Sin(_currentAngleToSun);
        var sign = planetModel.Settings.clockwise ? 1 : -1;
        _currentAngleToSun += planetModel.Settings.solarAngularVelocity * Time.deltaTime * sign;
        planetModel.CurrentAngleToSun = _currentAngleToSun;
        transform.position = new Vector3(positionX, positionY, transform.position.z);
        var selfRotationDelta = planetModel.Settings.selfRotationVelocity * Time.deltaTime;
        transform.Rotate(Vector3.forward, planetModel.Settings.clockwise ? selfRotationDelta : -selfRotationDelta);
        _currentHud.TransformWorldPosition(transform.position);
    }

    public void Shoot()
    {
        if (_isCooldown || _isDead || planetModel.GetAmmoByType(_currentRocketSettings.rocketType) <= 0) return;
        var rocket = _rocketFactory.Create();
        var state = rocket.GetCurrentState();
        state.RocketType = _currentRocketSettings.rocketType;
        state.UpdateState(muzzleTransform.position, transform.rotation, ROCKET_START_VELOCITY);
        rocket.Configure(state);
        planetModel.DecrementAmmoByType(_currentRocketSettings.rocketType);
        StartCoroutine(StartCooldown());
    }

    public int GetCurrentAmmo()
    {
        return planetModel.GetAmmoByType(_currentRocketSettings.rocketType);
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

    public class Factory : PlaceholderFactory<PlanetModel, PlanetController>
    {
    }

    public void OnDespawned()
    {
        transform.position = new Vector3(-1000, -1000, 0);
    }

    public void OnSpawned(PlanetModel model, IMemoryPool pool)
    {
        planetModel = model;
        _pool = pool;
        _currentAngleToSun = model.CurrentAngleToSun;
        transform.localScale = new Vector3(model.Settings.planetScale, model.Settings.planetScale,
            model.Settings.planetScale);
        _currentHud = _hudFactory.Create();
        _currentRocketSettings = _rocketSettingsList[0];
        _isDead = false;
        planetModel.SetNickname(model.Nickname);
        GetComponent<MeshRenderer>().material.color = model.Color;
        if (string.IsNullOrEmpty(model.Nickname))
        {
            planetModel.SetNickname(NickGenerator.GetRandomNickname());
        }

        _currentHud.Configure(planetModel.Nickname, model.IsPlayer, _gameSettings.initialPlanetHP, model.Hp);
    }

    public override float GetGravityModifier()
    {
        return transform.localScale.x * 210;
    }

    public override void ReceiveDamage(int damage)
    {
        planetModel.ApplyDamage(damage);
        _currentHud.SetNewHp(planetModel.Hp);
        if (planetModel.Hp <= 0)
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
        return planetModel.Settings.orbitRadius;
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