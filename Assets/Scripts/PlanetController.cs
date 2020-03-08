using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using SO;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;
using Random = UnityEngine.Random;

public class PlanetController : CelestialObject, IPoolable<PlanetState, IMemoryPool>, IDisposable
{
    private PlanetState currentState;
    
    private float currentAngleToSun = 0;
    private RocketController.Factory _rocketFactory;
    [SerializeField] private Transform muzzleTransform;
    private IMemoryPool _pool;
    private Hud.Factory _hudFactory;
    private Hud _currentHud;
    private bool isCooldown;
    private float cooldown;
    private SettingsSO.RocketSettings currentRocketSettings;
    private List<SettingsSO.RocketSettings> _rocketSettingsList;
    private bool isDead;
    public const float ROCKET_START_VELOCITY = 27;
    public Action onDieEvent;

    public bool IsDead => isDead;

    [Inject]
    void Construct(RocketController.Factory rocketFactory,
        Hud.Factory hudFactory,
        List<SettingsSO.RocketSettings> rocketSettingList)
    {
        _rocketFactory = rocketFactory;
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
        currentState.currentAngleToSun = currentAngleToSun;
        transform.position = new Vector3(positionX, positionY, transform.position.z);
        var selfRotationDelta = currentState.settings.selfRotationVelocity * Time.deltaTime;
        transform.Rotate(Vector3.forward, currentState.settings.clockwise ? selfRotationDelta : -selfRotationDelta);
        _currentHud.TransformWorldPosition(transform.position);
    }

    public void Shoot()
    {
        if (isCooldown || isDead || currentState.rocketAmmo[currentRocketSettings.rocketType] <= 0) return;
        
        var rocket = _rocketFactory.Create();
        var state = rocket.GetCurrentState();
        state.rocketType = currentRocketSettings.rocketType.ToString();
        state.posX = muzzleTransform.position.x;
        state.posY = muzzleTransform.position.y;
        state.rotationZ = transform.rotation.eulerAngles.z;
        state.velocity = ROCKET_START_VELOCITY;
        rocket.Configure(state);
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
        currentAngleToSun = state.currentAngleToSun;
        transform.localScale = new Vector3(state.settings.planetScale, state.settings.planetScale,state.settings.planetScale);
        _currentHud = _hudFactory.Create();
        currentRocketSettings = _rocketSettingsList[0];
        isDead = false;
        currentState.nickname = state.nickname;
        GetComponent<MeshRenderer>().material.color = state.color;
        if (string.IsNullOrEmpty(state.nickname))
        {
            var nickname = NickGenerator.GetRandomNickname();
            currentState.nickname = nickname;
        }
        _currentHud.Configure(currentState.nickname, state.isPlayer, state.hp, state.hp);
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
        onDieEvent?.Invoke();
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
        if (!isActiveAndEnabled) return;
        StopAllCoroutines();
        isCooldown = false;
        _currentHud.Despawn();
        _pool.Despawn(this);
    }
}




