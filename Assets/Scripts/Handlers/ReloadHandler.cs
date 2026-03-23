using System;
using System.Collections;
using UnityEngine;

namespace WeaponSystem
{
    /// <summary>
    /// Handles reload logic — timing, magazine count, constraints.
    /// Implements IReloadable. Works with FiringHandler to refill ammo.
    /// </summary>
    [Serializable]
    public class ReloadHandler : IReloadable
    {
        // --- Config ---
        private WeaponConfig _config;
        private FiringHandler _firingHandler;

        // --- Runtime state ---
        private int _remainingMagazines;
        private bool _isReloading;

        /// <summary>Raised when a reload completes successfully.</summary>
        public event Action OnReloadComplete;

        // -----------------------------------------------------------------
        // Initialization
        // -----------------------------------------------------------------

        public void Initialize(WeaponConfig config, FiringHandler firingHandler)
        {
            _config = config;
            _firingHandler = firingHandler;
            _remainingMagazines = _config.maxMagazines;
            _isReloading = false;
        }

        // -----------------------------------------------------------------
        // IReloadable
        // -----------------------------------------------------------------

        public void Reload()
        {
            // Actual coroutine is started by WeaponController
        }

        public bool CanReload()
        {
            if (_config == null || _isReloading || _remainingMagazines <= 0)
                return false;

            if (_firingHandler.GetCurrentAmmo() >= _config.ammoCapacity)
                return false; // already full

            return true;
        }

        public bool IsReloading => _isReloading;

        // -----------------------------------------------------------------
        // Coroutine (driven by WeaponController)
        // -----------------------------------------------------------------

        /// <summary>
        /// Coroutine that performs the reload over time.
        /// Must be started via MonoBehaviour.StartCoroutine.
        /// </summary>
        public IEnumerator ReloadCoroutine(bool isMoving)
        {
            if (!CanReload()) yield break;

            // Constraint checks
            if (isMoving && !_config.canReloadWhileMoving) yield break;
            if (_firingHandler.IsFiring && !_config.canReloadWhileFiring) yield break;

            _isReloading = true;
            yield return new WaitForSeconds(_config.reloadTime);

            _remainingMagazines--;
            _firingHandler.RefillAmmo();
            _isReloading = false;

            OnReloadComplete?.Invoke();
        }

        // -----------------------------------------------------------------
        // Queries
        // -----------------------------------------------------------------

        public int RemainingMagazines => _remainingMagazines;
    }
}
