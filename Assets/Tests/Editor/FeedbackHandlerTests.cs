using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace WeaponSystem.Tests
{
    [TestFixture]
    public class FeedbackHandlerTests
    {
        private FeedbackConfig _feedbackConfig;
        private FeedbackHandler _handler;
        private IEnumerator _lastCoroutine;

        [SetUp]
        public void SetUp()
        {
            _feedbackConfig = ScriptableObject.CreateInstance<FeedbackConfig>();
            _feedbackConfig.showHitIndicator = true;
            _feedbackConfig.showKillIndicator = true;
            _feedbackConfig.systemDestroyedAlert = true;
            _feedbackConfig.hitMarkerDuration = 0.3f;

            _handler = new FeedbackHandler();
            _lastCoroutine = null;
            _handler.Initialize(_feedbackConfig, co =>
            {
                _lastCoroutine = co;
                return null;
            });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_feedbackConfig);
        }

        [Test]
        public void ShowHitMarker_StartsCoroutine_WhenEnabled()
        {
            _handler.ShowHitMarker();
            Assert.IsNotNull(_lastCoroutine);
        }

        [Test]
        public void ShowHitMarker_DoesNothing_WhenDisabled()
        {
            _feedbackConfig.showHitIndicator = false;
            _handler.ShowHitMarker();
            Assert.IsNull(_lastCoroutine);
        }

        [Test]
        public void ShowHitMarker_FiresHitMarkerShownEvent()
        {
            bool shown = false;
            _handler.HitMarkerShown += () => shown = true;

            _handler.ShowHitMarker();
            _lastCoroutine?.MoveNext(); // step into coroutine

            Assert.IsTrue(shown);
        }

        [Test]
        public void ShowHitMarker_FiresHitMarkerHiddenOnCompletion()
        {
            bool hidden = false;
            _handler.HitMarkerHidden += () => hidden = true;

            _handler.ShowHitMarker();
            _lastCoroutine?.MoveNext(); // HitMarkerShown
            _lastCoroutine?.MoveNext(); // HitMarkerHidden (past yield)

            Assert.IsTrue(hidden);
        }

        [Test]
        public void ShowKillIndicator_FiresEvent_WhenEnabled()
        {
            bool killed = false;
            _handler.KillConfirmed += () => killed = true;

            _handler.ShowKillIndicator();
            Assert.IsTrue(killed);
        }

        [Test]
        public void ShowKillIndicator_DoesNothing_WhenDisabled()
        {
            _feedbackConfig.showKillIndicator = false;
            bool killed = false;
            _handler.KillConfirmed += () => killed = true;

            _handler.ShowKillIndicator();
            Assert.IsFalse(killed);
        }

        [Test]
        public void NotifySystemDamaged_FiresEvent_WhenEnabled()
        {
            bool damaged = false;
            _handler.SystemDamaged += () => damaged = true;

            _handler.NotifySystemDamaged();
            Assert.IsTrue(damaged);
        }

        [Test]
        public void NotifySystemDamaged_DoesNothing_WhenDisabled()
        {
            _feedbackConfig.systemDestroyedAlert = false;
            bool damaged = false;
            _handler.SystemDamaged += () => damaged = true;

            _handler.NotifySystemDamaged();
            Assert.IsFalse(damaged);
        }
    }
}
