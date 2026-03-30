using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace WeaponSystem.Tests
{
    [TestFixture]
    public class ReloadHandlerTests
    {
        private WeaponMainConfig _config;
        private AmmoConfig _ammo;
        private ShootingHandler _shootingHandler;
        private ReloadHandler _reloadHandler;

        // Tracks coroutines started (we can't actually run them in edit mode)
        private IEnumerator _lastCoroutine;

        [SetUp]
        public void SetUp()
        {
            _ammo = ScriptableObject.CreateInstance<AmmoConfig>();
            _ammo.ammoType = AmmoType.Standard;

            _config = ScriptableObject.CreateInstance<WeaponMainConfig>();
            _config.weaponName = "TestWeapon";
            _config.fireMode = FireMode.SemiAuto;
            _config.fireRate = 10f;
            _config.ammoCapacity = 30;
            _config.maxMagazines = 3;
            _config.reloadTime = 2.5f;
            _config.canReloadWhileMoving = true;
            _config.canReloadWhileShooting = false;
            _config.ammoTypes = new[] { _ammo };

            _shootingHandler = new ShootingHandler();
            _shootingHandler.Initialize(_config);

            _reloadHandler = new ReloadHandler();
            _lastCoroutine = null;
            _reloadHandler.Initialize(_config, _shootingHandler, co =>
            {
                _lastCoroutine = co;
                return null;
            });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_ammo);
        }

        [Test]
        public void Initialize_SetsRemainingMagazines()
        {
            Assert.AreEqual(3, _reloadHandler.RemainingMagazines);
        }

        [Test]
        public void IsReloading_FalseByDefault()
        {
            Assert.IsFalse(_reloadHandler.IsReloading);
        }

        [Test]
        public void CanReload_FalseWhenAmmoFull()
        {
            Assert.IsFalse(_reloadHandler.CanReload());
        }

        [Test]
        public void CanReload_TrueWhenAmmoPartial()
        {
            _shootingHandler.Shoot(); // 29 ammo
            Assert.IsTrue(_reloadHandler.CanReload());
        }

        [Test]
        public void CanReload_FalseWhenNoMagazinesLeft()
        {
            _config.maxMagazines = 0;
            _reloadHandler.Initialize(_config, _shootingHandler, co => { _lastCoroutine = co; return null; });

            _shootingHandler.Shoot();
            Assert.IsFalse(_reloadHandler.CanReload());
        }

        [Test]
        public void Reload_StartsCoroutineWhenValid()
        {
            _shootingHandler.Shoot(); // need partial ammo
            _reloadHandler.Reload();
            Assert.IsNotNull(_lastCoroutine);
        }

        [Test]
        public void Reload_DoesNothingWhenAmmoFull()
        {
            _reloadHandler.Reload();
            Assert.IsNull(_lastCoroutine);
        }

        [Test]
        public void Reload_BlockedWhileMoving_WhenConfigDisallows()
        {
            _config.canReloadWhileMoving = false;
            _reloadHandler.Initialize(_config, _shootingHandler, co => { _lastCoroutine = co; return null; });

            _shootingHandler.Shoot();
            _reloadHandler.Reload(isMoving: true);
            Assert.IsNull(_lastCoroutine);
        }

        [Test]
        public void Reload_AllowedWhileMoving_WhenConfigAllows()
        {
            _config.canReloadWhileMoving = true;
            _reloadHandler.Initialize(_config, _shootingHandler, co => { _lastCoroutine = co; return null; });

            _shootingHandler.Shoot();
            _reloadHandler.Reload(isMoving: true);
            Assert.IsNotNull(_lastCoroutine);
        }

        [Test]
        public void Reload_BlockedWhileShooting_WhenConfigDisallows()
        {
            _config.fireMode = FireMode.Automatic;
            _config.canReloadWhileShooting = false;
            _shootingHandler.Initialize(_config);
            _reloadHandler.Initialize(_config, _shootingHandler, co => { _lastCoroutine = co; return null; });

            _shootingHandler.Shoot(); // sets IsShooting = true for Automatic
            _reloadHandler.Reload();
            Assert.IsNull(_lastCoroutine);
        }

        [Test]
        public void ReloadCoroutine_FiresStartedEvent()
        {
            bool started = false;
            _reloadHandler.ReloadStarted += () => started = true;

            _shootingHandler.Shoot();
            _reloadHandler.Reload();

            // Manually step the coroutine to trigger ReloadStarted
            if (_lastCoroutine != null)
                _lastCoroutine.MoveNext();

            Assert.IsTrue(started);
        }

        [Test]
        public void ReloadCoroutine_SetsIsReloading()
        {
            _shootingHandler.Shoot();
            _reloadHandler.Reload();

            if (_lastCoroutine != null)
                _lastCoroutine.MoveNext(); // enters coroutine, sets _isReloading = true

            Assert.IsTrue(_reloadHandler.IsReloading);
        }

        [Test]
        public void ReloadCoroutine_CompletionRefillsAmmoAndFiresEvent()
        {
            bool completed = false;
            _reloadHandler.ReloadCompleted += () => completed = true;

            _shootingHandler.Shoot(); // 29 ammo
            _reloadHandler.Reload();

            // Step through coroutine: first MoveNext enters, second completes (past yield)
            if (_lastCoroutine != null)
            {
                _lastCoroutine.MoveNext(); // starts reload
                _lastCoroutine.MoveNext(); // completes reload (skips WaitForSeconds in edit mode)
            }

            Assert.IsTrue(completed);
            Assert.AreEqual(30, _shootingHandler.GetCurrentAmmo());
            Assert.IsFalse(_reloadHandler.IsReloading);
            Assert.AreEqual(2, _reloadHandler.RemainingMagazines);
        }
    }
}
