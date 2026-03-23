using System;
using UnityEngine;

namespace WeaponSystem
{
    /// <summary>
    /// Handles all firing logic — semi-auto, automatic, burst.
    /// Implements IWeapon. Stateless config; all runtime state is internal.
    /// </summary>
    [Serializable]
    public class FiringHandler : IWeapon
    {
        // --- Config (injected once) ---
        private WeaponConfig _config;
        private AmmoConfig _activeAmmo;

        // --- Runtime state ---
        private int _currentAmmo;
        private int _burstRemaining;
        private float _nextFireTime;
        private bool _isFiring;

        /// <summary>Raised every time a round is fired. Subscribers get the active AmmoConfig.</summary>
        public event Action<AmmoConfig> OnFired;

        // -----------------------------------------------------------------
        // Initialization
        // -----------------------------------------------------------------

        public void Initialize(WeaponConfig config)
        {
            _config = config;

            // Default to first ammo type
            _activeAmmo = (_config.ammoTypes != null && _config.ammoTypes.Length > 0)
                ? _config.ammoTypes[0]
                : null;

            _currentAmmo = _config.ammoCapacity;
            _burstRemaining = 0;
            _isFiring = false;
        }

        // -----------------------------------------------------------------
        // IWeapon
        // -----------------------------------------------------------------

        public void Fire()
        {
            if (!CanFire()) return;

            _isFiring = true;

            switch (_config.fireMode)
            {
                case FireMode.SemiAuto:
                    FireOnce();
                    _isFiring = false;
                    break;

                case FireMode.Burst:
                    _burstRemaining = _config.burstCount;
                    FireOnce();
                    _burstRemaining--;
                    break;

                case FireMode.Automatic:
                    FireOnce();
                    break;
            }
        }

        public void StopFire()
        {
            _isFiring = false;
            _burstRemaining = 0;
        }

        public bool CanFire()
        {
            return _config != null
                && _currentAmmo > 0
                && Time.time >= _nextFireTime;
        }

        public int GetCurrentAmmo() => _currentAmmo;

        // -----------------------------------------------------------------
        // Tick (called by WeaponController.Update)
        // -----------------------------------------------------------------

        /// <summary>
        /// Must be called every frame by the owning WeaponController.
        /// Handles automatic / burst continuation.
        /// </summary>
        public void Tick()
        {
            if (_config == null) return;

            // Continue burst
            if (_config.fireMode == FireMode.Burst && _burstRemaining > 0 && CanFire())
            {
                FireOnce();
                _burstRemaining--;
                if (_burstRemaining <= 0)
                    _isFiring = false;
            }

            // Continue automatic
            if (_config.fireMode == FireMode.Automatic && _isFiring && CanFire())
            {
                FireOnce();
            }
        }

        // -----------------------------------------------------------------
        // Ammo management
        // -----------------------------------------------------------------

        /// <summary>Refills magazine to capacity (called by ReloadHandler).</summary>
        public void RefillAmmo()
        {
            _currentAmmo = _config.ammoCapacity;
        }

        /// <summary>Switches active ammo type if the weapon supports it.</summary>
        public bool SwitchAmmo(AmmoConfig newAmmo)
        {
            if (newAmmo == null) return false;
            _activeAmmo = newAmmo;
            return true;
        }

        public AmmoConfig ActiveAmmo => _activeAmmo;
        public bool IsFiring => _isFiring;

        // -----------------------------------------------------------------
        // Internal
        // -----------------------------------------------------------------

        private void FireOnce()
        {
            _currentAmmo--;
            _nextFireTime = Time.time + 0.1f; // min interval between rounds
            OnFired?.Invoke(_activeAmmo);
        }
    }
}
