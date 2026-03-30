using NUnit.Framework;
using UnityEngine;

namespace WeaponSystem.Tests
{
    [TestFixture]
    public class HitCalculatorTests
    {
        private AccuracyConfig _accuracy;
        private AmmoConfig _ammo;

        [SetUp]
        public void SetUp()
        {
            _accuracy = ScriptableObject.CreateInstance<AccuracyConfig>();
            _accuracy.baseHitChance = 0.8f;
            _accuracy.rangeDropoffCurve = AnimationCurve.Linear(0f, 1f, 2000f, 0.2f);
            _accuracy.movementPenaltyCurve = AnimationCurve.Linear(0f, 1f, 10f, 0.3f);
            _accuracy.postureModifiers = new[]
            {
                new PostureModifier { posture = TargetPosture.Standing, multiplier = 1.0f },
                new PostureModifier { posture = TargetPosture.Crouching, multiplier = 0.7f },
                new PostureModifier { posture = TargetPosture.Prone, multiplier = 0.4f }
            };
            _accuracy.targetTypeModifiers = new[]
            {
                new TargetTypeModifier { targetType = TargetType.Infantry, multiplier = 0.8f },
                new TargetTypeModifier { targetType = TargetType.HeavyArmor, multiplier = 1.3f },
                new TargetTypeModifier { targetType = TargetType.Aircraft, multiplier = 0.4f }
            };

            _ammo = ScriptableObject.CreateInstance<AmmoConfig>();
            _ammo.ammoType = AmmoType.Standard;
            _ammo.hitChanceByRange = new[] { 0.95f, 0.85f, 0.7f, 0.5f, 0.3f };
            _ammo.penetratesBuildings = false;
            _ammo.penetratesVegetation = true;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_accuracy);
            Object.DestroyImmediate(_ammo);
        }

        [Test]
        public void CalcHitChance_ReturnsZero_WhenAccuracyNull()
        {
            float result = HitCalculator.CalcHitChance(null, _ammo, 100f, TargetPosture.Standing, 0f);
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void CalcHitChance_ReturnsZero_WhenAmmoNull()
        {
            float result = HitCalculator.CalcHitChance(_accuracy, null, 100f, TargetPosture.Standing, 0f);
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void CalcHitChance_HigherAtCloseRange()
        {
            float close = HitCalculator.CalcHitChance(_accuracy, _ammo, 50f, TargetPosture.Standing, 0f);
            float far = HitCalculator.CalcHitChance(_accuracy, _ammo, 1500f, TargetPosture.Standing, 0f);
            Assert.Greater(close, far);
        }

        [Test]
        public void CalcHitChance_DecreasesWithMovement()
        {
            float stationary = HitCalculator.CalcHitChance(_accuracy, _ammo, 100f, TargetPosture.Standing, 0f);
            float moving = HitCalculator.CalcHitChance(_accuracy, _ammo, 100f, TargetPosture.Standing, 5f);
            Assert.Greater(stationary, moving);
        }

        [Test]
        public void CalcHitChance_ProneHarderToHitThanStanding()
        {
            float standing = HitCalculator.CalcHitChance(_accuracy, _ammo, 100f, TargetPosture.Standing, 0f);
            float prone = HitCalculator.CalcHitChance(_accuracy, _ammo, 100f, TargetPosture.Prone, 0f);
            Assert.Greater(standing, prone);
        }

        [Test]
        public void CalcHitChance_ResultIsClamped01()
        {
            float result = HitCalculator.CalcHitChance(_accuracy, _ammo, 0f, TargetPosture.Standing, 0f);
            Assert.LessOrEqual(result, 1f);
            Assert.GreaterOrEqual(result, 0f);
        }

        [Test]
        public void CalcHitChance_WithTargetType_AppliesModifier()
        {
            float infantry = HitCalculator.CalcHitChance(_accuracy, _ammo, 100f, TargetPosture.Standing, 0f, TargetType.Infantry);
            float armor = HitCalculator.CalcHitChance(_accuracy, _ammo, 100f, TargetPosture.Standing, 0f, TargetType.HeavyArmor);
            Assert.Greater(armor, infantry);
        }

        [Test]
        public void CalcHitChance_Aircraft_HardestToHit()
        {
            float aircraft = HitCalculator.CalcHitChance(_accuracy, _ammo, 100f, TargetPosture.Standing, 0f, TargetType.Aircraft);
            float infantry = HitCalculator.CalcHitChance(_accuracy, _ammo, 100f, TargetPosture.Standing, 0f, TargetType.Infantry);
            Assert.Less(aircraft, infantry);
        }

        // Penetration tests

        [Test]
        public void EvalPenetration_ReturnsFalse_WhenAmmoNull()
        {
            Assert.IsFalse(HitCalculator.EvalPenetration(null, ObstacleType.Building));
        }

        [Test]
        public void EvalPenetration_Building_MatchesPenetratesBuildings()
        {
            Assert.IsFalse(HitCalculator.EvalPenetration(_ammo, ObstacleType.Building));
            Assert.IsFalse(HitCalculator.EvalPenetration(_ammo, ObstacleType.HardCover));
        }

        [Test]
        public void EvalPenetration_Vegetation_MatchesPenetratesVegetation()
        {
            Assert.IsTrue(HitCalculator.EvalPenetration(_ammo, ObstacleType.Vegetation));
            Assert.IsTrue(HitCalculator.EvalPenetration(_ammo, ObstacleType.SoftCover));
        }

        [Test]
        public void EvalPenetration_AP_PenetratesBuildings()
        {
            var apAmmo = ScriptableObject.CreateInstance<AmmoConfig>();
            apAmmo.penetratesBuildings = true;
            apAmmo.penetratesVegetation = true;

            Assert.IsTrue(HitCalculator.EvalPenetration(apAmmo, ObstacleType.Building));
            Assert.IsTrue(HitCalculator.EvalPenetration(apAmmo, ObstacleType.HardCover));

            Object.DestroyImmediate(apAmmo);
        }
    }
}
