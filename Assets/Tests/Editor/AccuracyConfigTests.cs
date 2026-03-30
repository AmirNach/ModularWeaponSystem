using NUnit.Framework;
using UnityEngine;

namespace WeaponSystem.Tests
{
    [TestFixture]
    public class AccuracyConfigTests
    {
        private AccuracyConfig _accuracy;

        [SetUp]
        public void SetUp()
        {
            _accuracy = ScriptableObject.CreateInstance<AccuracyConfig>();
            _accuracy.postureModifiers = new[]
            {
                new PostureModifier { posture = TargetPosture.Standing, multiplier = 1.0f },
                new PostureModifier { posture = TargetPosture.Crouching, multiplier = 0.7f },
                new PostureModifier { posture = TargetPosture.Prone, multiplier = 0.4f }
            };
            _accuracy.targetTypeModifiers = new[]
            {
                new TargetTypeModifier { targetType = TargetType.Infantry, multiplier = 0.8f },
                new TargetTypeModifier { targetType = TargetType.HeavyArmor, multiplier = 1.3f }
            };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_accuracy);
        }

        [Test]
        public void GetPostureModifier_ReturnsCorrectValue()
        {
            Assert.AreEqual(1.0f, _accuracy.GetPostureModifier(TargetPosture.Standing));
            Assert.AreEqual(0.7f, _accuracy.GetPostureModifier(TargetPosture.Crouching));
            Assert.AreEqual(0.4f, _accuracy.GetPostureModifier(TargetPosture.Prone));
        }

        [Test]
        public void GetTargetTypeModifier_ReturnsCorrectValue()
        {
            Assert.AreEqual(0.8f, _accuracy.GetTargetTypeModifier(TargetType.Infantry));
            Assert.AreEqual(1.3f, _accuracy.GetTargetTypeModifier(TargetType.HeavyArmor));
        }

        [Test]
        public void GetTargetTypeModifier_ReturnsDefault_ForMissingType()
        {
            // Aircraft is not configured, should return 1f
            Assert.AreEqual(1f, _accuracy.GetTargetTypeModifier(TargetType.Aircraft));
        }

        [Test]
        public void GetPostureModifier_CachesResults()
        {
            // First call builds the dictionary
            float first = _accuracy.GetPostureModifier(TargetPosture.Standing);
            // Second call uses cache
            float second = _accuracy.GetPostureModifier(TargetPosture.Standing);
            Assert.AreEqual(first, second);
        }
    }
}
