using UnityEngine;

namespace WeaponSystem
{
    /// <summary>
    /// Configuration for a single ammo type.
    /// Create one asset per ammo type (AP, HE, etc.) and assign to WeaponConfig.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAmmoConfig", menuName = "WeaponSystem/AmmoConfig")]
    public class AmmoConfig : ScriptableObject
    {
        [Header("Type")]
        public AmmoType ammoType = AmmoType.Standard;

        [Header("Penetration")]
        public bool penetratesBuildings;
        public bool penetratesVegetation;

        [Header("Ballistics")]
        [Tooltip("X = distance (m), Y = time to impact (s)")]
        public AnimationCurve timeToImpactCurve = AnimationCurve.Linear(0f, 0f, 2000f, 3f);

        [Tooltip("Seconds from hit to target destruction")]
        [Min(0f)]
        public float timeToDestroy = 0.5f;

        [Header("Accuracy")]
        [Tooltip("Hit chance per 100m interval. Index 0 = 0-100m, index 1 = 100-200m, etc.")]
        [Range(0f, 1f)]
        public float[] hitChanceByRange = { 0.95f, 0.85f, 0.7f, 0.5f, 0.3f };

        [Header("Damage")]
        [Min(0f)]
        public float damageMultiplier = 1f;

        /// <summary>
        /// Returns hit chance for a given range, clamped to the last entry if out of bounds.
        /// </summary>
        public float GetHitChanceAtRange(float rangeMeters)
        {
            if (hitChanceByRange == null || hitChanceByRange.Length == 0)
                return 0f;

            int index = Mathf.FloorToInt(rangeMeters / 100f);
            index = Mathf.Clamp(index, 0, hitChanceByRange.Length - 1);
            return hitChanceByRange[index];
        }
    }
}
