using System;
using System.Collections;
using UnityEngine;

namespace WeaponSystem
{
    /// <summary>
    /// Handles operator feedback — hit markers, kill indicators, system damage alerts.
    /// Pure logic; actual UI rendering is left to subscribers / external systems.
    /// </summary>
    [Serializable]
    public class FeedbackHandler
    {
        // --- Config ---
        private FeedbackConfig _config;

        // --- Events (subscribe from UI / HUD layer) ---
        public event Action OnHitMarkerShow;
        public event Action OnHitMarkerHide;
        public event Action OnKillIndicator;
        public event Action OnSystemDamaged;

        // -----------------------------------------------------------------
        // Initialization
        // -----------------------------------------------------------------

        public void Initialize(FeedbackConfig config)
        {
            _config = config;
        }

        // -----------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------

        /// <summary>Shows a hit marker for the configured duration.</summary>
        public IEnumerator ShowHitMarker()
        {
            if (_config == null || !_config.showHitIndicator)
                yield break;

            OnHitMarkerShow?.Invoke();
            yield return new WaitForSeconds(_config.hitMarkerDuration);
            OnHitMarkerHide?.Invoke();
        }

        /// <summary>Shows a kill indicator if enabled in config.</summary>
        public void ShowKillIndicator()
        {
            if (_config != null && _config.showKillIndicator)
                OnKillIndicator?.Invoke();
        }

        /// <summary>Notifies that this weapon system took damage/was destroyed.</summary>
        public void OnSystemDamagedNotify()
        {
            if (_config != null && _config.systemDestroyedAlert)
                OnSystemDamaged?.Invoke();
        }
    }
}
