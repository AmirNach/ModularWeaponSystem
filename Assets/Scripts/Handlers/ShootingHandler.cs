using System;
using UnityEngine;

namespace WeaponSystem
{
    [Serializable]
    public class ShootingHandler : IWeapon
    {
        private WeaponMainConfig _config;
        private AmmoConfig _activeAmmo;
        private int _currentAmmo;
        private int _burstRemaining;
        private float _nextShootTime;
        private bool _isShooting;

        public event Action<AmmoConfig> Shot;

        public AmmoConfig ActiveAmmo => _activeAmmo;
        public bool IsShooting => _isShooting;

        public void Initialize(WeaponMainConfig config)
        {
            _config = config;
            _activeAmmo = null;
            _currentAmmo = 0;
            _burstRemaining = 0;
            _isShooting = false;

            if (_config == null) return;

            _activeAmmo = (_config.ammoTypes != null && _config.ammoTypes.Length > 0)
                ? _config.ammoTypes[0]
                : null;
            _currentAmmo = _config.ammoCapacity;
        }

        public void Shoot()
        {
            if (!CanShoot()) return;

            _isShooting = true;

            switch (_config.fireMode)
            {
                case FireMode.SemiAuto:
                    ShootOnce();
                    _isShooting = false;
                    break;
                case FireMode.Burst:
                    _burstRemaining = _config.burstCount;
                    ShootOnce();
                    _burstRemaining--;
                    break;
                case FireMode.Automatic:
                    ShootOnce();
                    break;
            }
        }

        public void StopShoot()
        {
            _isShooting = false;
            _burstRemaining = 0;
        }

        public bool CanShoot()
        {
            return _config != null
                && _activeAmmo != null
                && _currentAmmo > 0
                && Time.time >= _nextShootTime;
        }

        public int GetCurrentAmmo() => _currentAmmo;

        /// <summary>Called every frame by WeaponController to continue burst/auto.</summary>
        public void Tick()
        {
            if (_config == null) return;

            if (_config.fireMode == FireMode.Burst && _burstRemaining > 0 && CanShoot())
            {
                ShootOnce();
                _burstRemaining--;
                if (_burstRemaining <= 0)
                    _isShooting = false;
            }

            if (_config.fireMode == FireMode.Automatic && _isShooting && CanShoot())
                ShootOnce();
        }

        public void RefillAmmo() => _currentAmmo = _config.ammoCapacity;

        public bool SwitchAmmoType(AmmoType type)
        {
            if (_config == null || _config.ammoTypes == null) return false;

            foreach (var ammo in _config.ammoTypes)
            {
                if (ammo != null && ammo.ammoType == type)
                {
                    _activeAmmo = ammo;
                    return true;
                }
            }

            return false;
        }

        public bool HasAmmoType(AmmoType type)
        {
            if (_config == null || _config.ammoTypes == null) return false;

            foreach (var ammo in _config.ammoTypes)
                if (ammo != null && ammo.ammoType == type) return true;

            return false;
        }

        private void ShootOnce()
        {
            _currentAmmo--;
            _nextShootTime = Time.time + (1f / _config.fireRate);
            Shot?.Invoke(_activeAmmo);
        }
    }
}
