using NUnit.Framework;
using UnityEngine;

namespace WeaponSystem.Tests
{
    [TestFixture]
    public class ShootingHandlerTests
    {
        private WeaponMainConfig _config;
        private AmmoConfig _standardAmmo;
        private AmmoConfig _apAmmo;
        private ShootingHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _standardAmmo = ScriptableObject.CreateInstance<AmmoConfig>();
            _standardAmmo.ammoType = AmmoType.Standard;
            _standardAmmo.damageMultiplier = 1f;

            _apAmmo = ScriptableObject.CreateInstance<AmmoConfig>();
            _apAmmo.ammoType = AmmoType.AP;
            _apAmmo.damageMultiplier = 1.5f;

            _config = ScriptableObject.CreateInstance<WeaponMainConfig>();
            _config.weaponName = "TestWeapon";
            _config.fireMode = FireMode.SemiAuto;
            _config.burstCount = 3;
            _config.fireRate = 10f;
            _config.ammoCapacity = 30;
            _config.maxMagazines = 5;
            _config.ammoTypes = new[] { _standardAmmo, _apAmmo };

            _handler = new ShootingHandler();
            _handler.Initialize(_config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_standardAmmo);
            Object.DestroyImmediate(_apAmmo);
        }

        [Test]
        public void Initialize_SetsAmmoToCapacity()
        {
            Assert.AreEqual(30, _handler.GetCurrentAmmo());
        }

        [Test]
        public void Initialize_SetsFirstAmmoTypeAsActive()
        {
            Assert.AreEqual(_standardAmmo, _handler.ActiveAmmo);
        }

        [Test]
        public void CanShoot_ReturnsTrueWhenReady()
        {
            Assert.IsTrue(_handler.CanShoot());
        }

        [Test]
        public void CanShoot_ReturnsFalseWhenNoAmmo()
        {
            for (int i = 0; i < 30; i++)
                _handler.Shoot();

            Assert.IsFalse(_handler.CanShoot());
        }

        [Test]
        public void CanShoot_ReturnsFalseWithNullConfig()
        {
            var handler = new ShootingHandler();
            handler.Initialize(null);
            Assert.IsFalse(handler.CanShoot());
        }

        [Test]
        public void Shoot_SemiAuto_DecrementsAmmo()
        {
            _handler.Shoot();
            Assert.AreEqual(29, _handler.GetCurrentAmmo());
        }

        [Test]
        public void Shoot_SemiAuto_StopsShootingImmediately()
        {
            _handler.Shoot();
            Assert.IsFalse(_handler.IsShooting);
        }

        [Test]
        public void Shoot_Automatic_StaysShooting()
        {
            _config.fireMode = FireMode.Automatic;
            _handler.Initialize(_config);

            _handler.Shoot();
            Assert.IsTrue(_handler.IsShooting);
        }

        [Test]
        public void StopShoot_StopsFiring()
        {
            _config.fireMode = FireMode.Automatic;
            _handler.Initialize(_config);

            _handler.Shoot();
            _handler.StopShoot();
            Assert.IsFalse(_handler.IsShooting);
        }

        [Test]
        public void Shoot_InvokesShot_Event()
        {
            AmmoConfig firedAmmo = null;
            _handler.Shot += ammo => firedAmmo = ammo;

            _handler.Shoot();

            Assert.IsNotNull(firedAmmo);
            Assert.AreEqual(AmmoType.Standard, firedAmmo.ammoType);
        }

        [Test]
        public void Shoot_DoesNothing_WhenCannotShoot()
        {
            // Drain all ammo
            for (int i = 0; i < 30; i++)
                _handler.Shoot();

            int ammoBefore = _handler.GetCurrentAmmo();
            _handler.Shoot();
            Assert.AreEqual(ammoBefore, _handler.GetCurrentAmmo());
        }

        [Test]
        public void RefillAmmo_RestoresToCapacity()
        {
            _handler.Shoot();
            _handler.RefillAmmo();
            Assert.AreEqual(30, _handler.GetCurrentAmmo());
        }

        [Test]
        public void SwitchAmmoType_SwitchesToValidType()
        {
            bool result = _handler.SwitchAmmoType(AmmoType.AP);
            Assert.IsTrue(result);
            Assert.AreEqual(_apAmmo, _handler.ActiveAmmo);
        }

        [Test]
        public void SwitchAmmoType_ReturnsFalseForUnavailableType()
        {
            bool result = _handler.SwitchAmmoType(AmmoType.Smoke);
            Assert.IsFalse(result);
            Assert.AreEqual(_standardAmmo, _handler.ActiveAmmo);
        }

        [Test]
        public void SwitchAmmoType_ReturnsFalseWhenNoAmmoTypes()
        {
            _config.ammoTypes = null;
            _handler.Initialize(_config);

            Assert.IsFalse(_handler.SwitchAmmoType(AmmoType.Standard));
        }

        [Test]
        public void Shoot_Burst_SetsBurstRemaining()
        {
            _config.fireMode = FireMode.Burst;
            _config.burstCount = 3;
            _handler.Initialize(_config);

            _handler.Shoot();
            // First round fires immediately, burstRemaining = burstCount - 1 = 2
            Assert.AreEqual(29, _handler.GetCurrentAmmo());
        }

        [Test]
        public void Initialize_WithEmptyAmmoTypes_SetsActiveAmmoNull()
        {
            _config.ammoTypes = new AmmoConfig[0];
            _handler.Initialize(_config);

            Assert.IsNull(_handler.ActiveAmmo);
            Assert.IsFalse(_handler.CanShoot());
        }

        [Test]
        public void Initialize_CanBeCalledMultipleTimes()
        {
            _handler.Shoot(); // 29 ammo
            _handler.Initialize(_config); // re-init
            Assert.AreEqual(30, _handler.GetCurrentAmmo());
        }
    }
}
