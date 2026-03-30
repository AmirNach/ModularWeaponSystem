using System;
using System.Collections;
using UnityEngine;

namespace WeaponSystem
{
    [Serializable]
    public class ReloadHandler : IReloadable
    {
        private WeaponConfig _config;
        private ShootingHandler _shootingHandler;
        private Func<IEnumerator, Coroutine> _startCoroutine;

        private int _remainingMagazines;
        private bool _isReloading;

        public event Action ReloadCompleted;

        // -----------------------------------------------------------------
        // Initialization
        // -----------------------------------------------------------------

        public void Initialize(WeaponConfig config, ShootingHandler shootingHandler,
            Func<IEnumerator, Coroutine> startCoroutine)
        {
            _config = config;
            _shootingHandler = shootingHandler;
            _startCoroutine = startCoroutine;
            _remainingMagazines = _config.maxMagazines;
            _isReloading = false;
        }

        // -----------------------------------------------------------------
        // IReloadable
        // -----------------------------------------------------------------

        public void Reload() => Reload(false);

        public bool CanReload()
        {
            if (_config == null || _isReloading || _remainingMagazines <= 0)
                return false;

            if (_shootingHandler.GetCurrentAmmo() >= _config.ammoCapacity)
                return false;

            return true;
        }

        public bool IsReloading => _isReloading;

        // -----------------------------------------------------------------
        // Reload with movement constraint
        // -----------------------------------------------------------------

        public void Reload(bool isMoving)
        {
            if (!CanReload()) return;
            if (isMoving && !_config.canReloadWhileMoving) return;
            if (_shootingHandler.IsShooting && !_config.canReloadWhileShooting) return;

            _startCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            _isReloading = true;
            yield return new WaitForSeconds(_config.reloadTime);

            _remainingMagazines--;
            _shootingHandler.RefillAmmo();
            _isReloading = false;

            ReloadCompleted?.Invoke();
        }

        // -----------------------------------------------------------------
        // Queries
        // -----------------------------------------------------------------

        public int RemainingMagazines => _remainingMagazines;
    }
}
