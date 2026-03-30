using System;
using System.Collections;
using UnityEngine;

namespace WeaponSystem
{
    [Serializable]
    public class FeedbackHandler
    {
        private FeedbackConfig _config;
        private Func<IEnumerator, Coroutine> _startCoroutine;

        public event Action HitMarkerShown;
        public event Action HitMarkerHidden;
        public event Action KillConfirmed;
        public event Action SystemDamaged;

        public void Initialize(FeedbackConfig config, Func<IEnumerator, Coroutine> startCoroutine)
        {
            _config = config;
            _startCoroutine = startCoroutine;
        }

        public void ShowHitMarker()
        {
            if (_config == null || !_config.showHitIndicator) return;
            _startCoroutine(HitMarkerCoroutine());
        }

        public void ShowKillIndicator()
        {
            if (_config != null && _config.showKillIndicator)
                KillConfirmed?.Invoke();
        }

        public void NotifySystemDamaged()
        {
            if (_config != null && _config.systemDestroyedAlert)
                SystemDamaged?.Invoke();
        }

        private IEnumerator HitMarkerCoroutine()
        {
            HitMarkerShown?.Invoke();
            yield return new WaitForSeconds(_config.hitMarkerDuration);
            HitMarkerHidden?.Invoke();
        }
    }
}
