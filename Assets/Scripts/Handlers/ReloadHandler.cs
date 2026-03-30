using System;
using System.Collections;
using UnityEngine;

namespace WeaponSystem
{
    [Serializable]
    public class ReloadHandler : IReloadable
    {
        private WeaponMainConfig _config;
        private ShootingHandler _shootingHandler;
        private Func<IEnumerator, Coroutine> _startCoroutine;
        private int _remainingMagazines;
        private bool _isReloading;

        public event Action ReloadStarted;
        public event Action ReloadCompleted;

        public bool IsReloading => _isReloading;
        public int RemainingMagazines => _remainingMagazines;

        public void Initialize(WeaponMainConfig config, ShootingHandler shootingHandler,
            Func<IEnumerator, Coroutine> startCoroutine)
        {
            _config = config;
            _shootingHandler = shootingHandler;
            _startCoroutine = startCoroutine;
            _remainingMagazines = _config.maxMagazines;
            _isReloading = false;
        }

        public void Reload() => Reload(false);

        public void Reload(bool isMoving)
        {
            if (!CanReload()) return;
            if (isMoving && !_config.canReloadWhileMoving) return;
            if (_shootingHandler.IsShooting && !_config.canReloadWhileShooting) return;

            _startCoroutine(ReloadCoroutine());
        }

        public bool CanReload()
        {
            return _config != null
                && !_isReloading
                && _remainingMagazines > 0
                && _shootingHandler.GetCurrentAmmo() < _config.ammoCapacity;
        }

        private IEnumerator ReloadCoroutine()
        {
            _isReloading = true;
            ReloadStarted?.Invoke();

            yield return new WaitForSeconds(_config.reloadTime);

            _remainingMagazines--;
            _shootingHandler.RefillAmmo();
            _isReloading = false;

            ReloadCompleted?.Invoke();
        }
    }
}
