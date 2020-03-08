using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using SO;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

public class PlanetController : CelestialObject, IPoolable<PlanetState, IMemoryPool>, IDisposable
{
    private PlanetState currentState;
    
    private float currentAngleToSun = 0;
    private RocketController.Factory _rocketFactory;
    [SerializeField] private Transform muzzleTransform;
    private SettingsSO.GameSettings _gameSetttings;
    private IMemoryPool _pool;
    private Hud.Factory _hudFactory;
    private Hud _currentHud;
    private bool isCooldown;
    private float cooldown;
    private SettingsSO.RocketSettings currentRocketSettings;
    private List<SettingsSO.RocketSettings> _rocketSettingsList;
    private bool isDead;
    [SerializeField] private LineRenderer _lineRenderer;
    private List<RocketController> activeRockets = new List<RocketController>();
    private List<RocketController> removeList = new List<RocketController>();

    [Inject]
    void Construct(RocketController.Factory rocketFactory,
        SettingsSO.GameSettings gameSettings,
        Hud.Factory hudFactory,
        List<SettingsSO.RocketSettings> rocketSettingList)
    {
        _rocketFactory = rocketFactory;
        _gameSetttings = gameSettings;
        _hudFactory = hudFactory;
        _rocketSettingsList = rocketSettingList;
        currentRocketSettings = rocketSettingList[0];
    }
    void Update()
    {
        var positionX = currentState.settings.orbitRadius * Mathf.Cos(currentAngleToSun);
        var positionY = currentState.settings.orbitRadius * Mathf.Sin(currentAngleToSun);
        var sign = currentState.settings.clockwise ? 1 : -1;
        currentAngleToSun += currentState.settings.solarAngularVelocity * Time.deltaTime * sign;
        transform.position = new Vector3(positionX, positionY, transform.position.z);
        var selfRotationDelta = currentState.settings.selfRotationVelocity * Time.deltaTime;
        transform.Rotate(Vector3.forward, currentState.settings.clockwise ? selfRotationDelta : -selfRotationDelta);
        _currentHud.TransformWorldPosition(transform.position);
    }

    public void Shoot()
    {
        if (isCooldown || isDead || currentState.rocketAmmo[currentRocketSettings.rocketType] <= 0) return;
        var rocket = _rocketFactory.Create(muzzleTransform.position, transform.rotation, currentRocketSettings.rocketType);
        if (!activeRockets.Contains(rocket))
        {
            activeRockets.Add(rocket);
        }

        currentState.rocketAmmo[currentRocketSettings.rocketType]--;
        StartCoroutine(StartCooldown());
    }

    public void DisposeAllRockets()
    {
        foreach (var rocket in activeRockets)
        {
            if (rocket.isActiveAndEnabled)
            {
                rocket.Dispose();
            }
        }
    }

    public int GetCurrentAmmo()
    {
        return currentState.rocketAmmo[currentRocketSettings.rocketType];
    }

    private IEnumerator StartCooldown()
    {
        cooldown = currentRocketSettings.cooldown;
        isCooldown = true;
        while (cooldown >= 0)
        {
            yield return null;
            cooldown -= Time.deltaTime;
            _currentHud.SetCooldown(cooldown / currentRocketSettings.cooldown);
        }

        isCooldown = false;
        cooldown = 0;
        _currentHud.SetCooldown(cooldown / currentRocketSettings.cooldown);
    }

    public class Factory : PlaceholderFactory<PlanetState, PlanetController>{}

    public void OnDespawned()
    {
        transform.position = new Vector3(-1000,-1000,0);
    }

    public void OnSpawned(PlanetState state, IMemoryPool pool)
    {
        currentState = state;
        _pool = pool;
        transform.localScale = new Vector3(state.settings.planetScale, state.settings.planetScale,state.settings.planetScale);
        currentAngleToSun = Random.Range(0, Mathf.PI * 2);
        _currentHud = _hudFactory.Create();
        currentRocketSettings = _rocketSettingsList[0];
        _currentHud.Configure("Wayko", state.isPlayer, state.hp);
    }

    public override float GetGravityModifier()
    {
        return transform.localScale.x * 210;
    }

    public override void ReceiveDamage(int damage)
    {
        currentState.hp -= damage;
        _currentHud.SetNewHp(currentState.hp);
        if (currentState.hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Dispose();
    }

    public Transform GetMuzzle()
    {
        return muzzleTransform;
    }

    public SettingsSO.RocketSettings GetCurrentRocketSettings()
    {
        return currentRocketSettings;
    }

    public void SetRocketType(RocketType nextRocketType)
    {
        currentRocketSettings = _rocketSettingsList.Find(rocket => rocket.rocketType == nextRocketType);
    }

    public double GetOrbit()
    {
        return currentState.settings.orbitRadius;
    }

    public PlanetState GetCurrentState()
    {
        return currentState;
    }

    public void Dispose()
    {
        StopAllCoroutines();
        isCooldown = false;
        _currentHud.Despawn();
        _pool.Despawn(this);
    }
}




