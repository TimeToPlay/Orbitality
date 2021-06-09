using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using SO;
using UniRx;
using UnityEngine;
using Utils;
using Views;
using Zenject;

namespace Controllers
{
    public class PlanetViewController : CelestialObject, IPoolable<PlanetModel, IMemoryPool>, IDisposable
    {
        public const float ROCKET_START_VELOCITY = 27;
        [SerializeField] private Transform muzzleTransform;
        public event Action OnDieEvent;
        public bool IsDead => _isDead;

        private PlanetModel _planetModel;
        private float _currentAngleToSun;
        private RocketViewController.Factory _rocketFactory;
        private IMemoryPool _pool;
        private Hud.Factory _hudFactory;
        private Hud _currentHud;
        private bool _isCooldown;
        private float _cooldown;
        private SettingsSO.RocketSettings _currentRocketSettings;
        private List<SettingsSO.RocketSettings> _rocketSettingsList;
        private bool _isDead;
        private SettingsSO.GameSettings _gameSettings;


        [Inject]
        void Construct(RocketViewController.Factory rocketFactory,
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
            ConfigureUpdatePosition();
        }

        public void Shoot()
        {
            if (_isCooldown || _isDead || _planetModel.GetAmmoByType(_currentRocketSettings.rocketType) <= 0) return;
            var rocket = _rocketFactory.Create();
            var state = rocket.GetCurrentState();
            state.RocketType = _currentRocketSettings.rocketType;
            state.UpdateState(muzzleTransform.position, transform.rotation, ROCKET_START_VELOCITY);
            rocket.Configure(state);
            _planetModel.DecrementAmmoByType(_currentRocketSettings.rocketType);
            StartCoroutine(StartCooldown());
        }

        public int GetCurrentAmmo()
        {
            return _planetModel.GetAmmoByType(_currentRocketSettings.rocketType);
        }

        public class Factory : PlaceholderFactory<PlanetModel, PlanetViewController>
        {
        }

        public void OnDespawned()
        {
            transform.position = new Vector3(-1000, -1000, 0);
        }

        public void OnSpawned(PlanetModel model, IMemoryPool pool)
        {
            _planetModel = model;
            _pool = pool;
            _currentAngleToSun = model.CurrentAngleToSun;
            transform.localScale = new Vector3(model.Settings.planetScale, model.Settings.planetScale,
                model.Settings.planetScale);
            _currentHud = _hudFactory.Create();
            _currentRocketSettings = _rocketSettingsList[0];
            _isDead = false;
            _planetModel.SetNickname(model.Nickname);
            GetComponent<MeshRenderer>().material.color = model.Color;
            if (string.IsNullOrEmpty(model.Nickname))
            {
                _planetModel.SetNickname(NickGenerator.GetRandomNickname());
            }

            _currentHud.Configure(_planetModel.Nickname, model.IsPlayer, _gameSettings.initialPlanetHP, model.Hp);
        }

        public override float GetGravityModifier()
        {
            return transform.localScale.x * 210;
        }

        public override void ReceiveDamage(int damage)
        {
            _planetModel.ApplyDamage(damage);
            _currentHud.SetNewHp(_planetModel.Hp);
            if (_planetModel.Hp <= 0)
            {
                Die();
            }
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
            return _planetModel.Settings.orbitRadius;
        }

        public void Dispose()
        {
            if (!isActiveAndEnabled) return;
            StopAllCoroutines();
            _isCooldown = false;
            _currentHud.Despawn();
            _pool.Despawn(this);
        }
        
        private void Die()
        {
            _isDead = true;
            OnDieEvent?.Invoke();
            Dispose();
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

        private void ConfigureUpdatePosition()
        {
            Observable.EveryUpdate().Subscribe(_ => { UpdatePosition(); }).AddTo(this);
        }
        
        private void UpdatePosition()
        {
            if (!isActiveAndEnabled) return;
            var positionX = _planetModel.Settings.orbitRadius * Mathf.Cos(_currentAngleToSun);
            var positionY = _planetModel.Settings.orbitRadius * Mathf.Sin(_currentAngleToSun);
            var sign = _planetModel.Settings.clockwise ? 1 : -1;
            _currentAngleToSun += _planetModel.Settings.solarAngularVelocity * Time.deltaTime * sign;
            _planetModel.CurrentAngleToSun = _currentAngleToSun;
            transform.position = new Vector3(positionX, positionY, transform.position.z);
            var selfRotationDelta = _planetModel.Settings.selfRotationVelocity * Time.deltaTime;
            transform.Rotate(Vector3.forward, _planetModel.Settings.clockwise ? selfRotationDelta : -selfRotationDelta);
            _currentHud.TransformWorldPosition(transform.position);
        }
    }
}