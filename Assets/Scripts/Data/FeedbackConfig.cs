using UnityEngine;

namespace WeaponSystem
{
    /// <summary>
    /// Configuration for operator feedback — hit markers, kill indicators, damage alerts.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFeedbackConfig", menuName = "WeaponSystem/FeedbackConfig")]
    public class FeedbackConfig : ScriptableObject
    {
        [Header("Indicators")]
        public bool showHitIndicator = true;
        public bool showKillIndicator = true;
        public bool systemDestroyedAlert = true;

        [Header("Timing")]
        [Min(0f)]
        public float hitMarkerDuration = 0.3f;
    }
}
