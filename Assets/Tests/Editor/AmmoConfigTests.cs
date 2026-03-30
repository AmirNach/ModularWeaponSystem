using NUnit.Framework;
using UnityEngine;

namespace WeaponSystem.Tests
{
    [TestFixture]
    public class AmmoConfigTests
    {
        private AmmoConfig _ammo;

        [SetUp]
        public void SetUp()
        {
            _ammo = ScriptableObject.CreateInstance<AmmoConfig>();
            _ammo.hitChanceByRange = new[] { 0.95f, 0.85f, 0.70f, 0.50f, 0.30f };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_ammo);
        }

        [Test]
        public void GetHitChanceAtRange_ReturnsFirstBucket_ForCloseRange()
        {
            Assert.AreEqual(0.95f, _ammo.GetHitChanceAtRange(50f));
        }

        [Test]
        public void GetHitChanceAtRange_ReturnsCorrectBucket()
        {
            Assert.AreEqual(0.85f, _ammo.GetHitChanceAtRange(150f));
            Assert.AreEqual(0.70f, _ammo.GetHitChanceAtRange(250f));
        }

        [Test]
        public void GetHitChanceAtRange_ClampsToLastBucket_ForExtremeRange()
        {
            Assert.AreEqual(0.30f, _ammo.GetHitChanceAtRange(9999f));
        }

        [Test]
        public void GetHitChanceAtRange_ReturnsZero_WhenArrayEmpty()
        {
            _ammo.hitChanceByRange = new float[0];
            Assert.AreEqual(0f, _ammo.GetHitChanceAtRange(100f));
        }

        [Test]
        public void GetHitChanceAtRange_ReturnsZero_WhenArrayNull()
        {
            _ammo.hitChanceByRange = null;
            Assert.AreEqual(0f, _ammo.GetHitChanceAtRange(100f));
        }

        [Test]
        public void GetHitChanceAtRange_ZeroRange_ReturnsFirstBucket()
        {
            Assert.AreEqual(0.95f, _ammo.GetHitChanceAtRange(0f));
        }

        [Test]
        public void GetHitChanceAtRange_ExactBoundary_ReturnsCorrectBucket()
        {
            // At exactly 100m, index = floor(100/100) = 1
            Assert.AreEqual(0.85f, _ammo.GetHitChanceAtRange(100f));
        }
    }
}
