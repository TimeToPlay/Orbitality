using System;
using System.Collections;
using System.Collections.Generic;
using SO;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

public class PlanetController : CelestialObject, IPoolable<PlanetState, IMemoryPool>
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
        _rocketFactory.Create(muzzleTransform.position, transform.rotation, currentRocketSettings.rocketType);
        currentState.rocketAmmo[currentRocketSettings.rocketType]--;
        StartCoroutine(StartCooldown());
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
        _currentHud.Despawn();
        _pool.Despawn(this);
    }

    public Transform GetMuzzle()
    {
        return muzzleTransform;
    }

    /*public Vector3[] CalculateForecastTrajectory(CelestialObject sun)
    {
        Vector3[] positions = new Vector3[100];
        positions[0] = GetMuzzle().position;
        positions[1] = GetMuzzle().position + transform.up * 5;
        var lasPower = Vector3.zero;
        var speed = 0f;
        var time = 0f;
        for (int i = 2; i < 100; i++)
        {
            var dir = (positions[i - 1] - positions[i - 2]).normalized;
            var gravityPowerMagnitude = sun.GetGravityModifier() / Mathf.Pow(Vector3.Distance(sun.transform.position, positions[i-1]), 2);
            time += 0.016f;
            speed = 27 + currentRocketSettings.acceleration * time;
            var gravityPower = (sun.transform.position - positions[i-1]).normalized * gravityPowerMagnitude;

            var delta = (dir * (speed) + gravityPower) * 0.016f;
            lasPower = delta;
            positions[i] = positions[i - 1] + delta;
        }
        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);
        return positions;
    }*/
    public SettingsSO.RocketSettings GetCurrentRocketSettings()
    {
        return currentRocketSettings;
    }

    public void SetRocketType(RocketType nextRocketType)
    {
        currentRocketSettings = _rocketSettingsList.Find(rocket => rocket.rocketType == nextRocketType);
    }
}

[Serializable]
public class PlanetState
{
    public int hp;
    public Dictionary<RocketType, int> rocketAmmo = new Dictionary<RocketType, int>();
    public float angleToSun;
    public SettingsSO.PlanetSettings settings;
    public bool isPlayer;
    public List<AmmoInfo> ammoInfoList;
}

[Serializable]
public class AmmoInfo
{
    public string RocketType;
    public int ammo;
}
