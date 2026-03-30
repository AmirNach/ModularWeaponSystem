using UnityEngine;

namespace WeaponSystem
{
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
        [Min(0f)] public float timeToDestroy = 0.5f;

        [Header("Accuracy")]
        [Tooltip("Hit chance per 100m interval. Index 0 = 0-100m, Index 1 = 100-200m, etc.")]
        public float[] hitChanceByRange = { 0.95f, 0.85f, 0.7f, 0.5f, 0.3f };

        [Header("Damage")]
        [Min(0f)] public float damageMultiplier = 1f;

        public float GetHitChanceAtRange(float rangeMeters)
        {
            if (hitChanceByRange == null || hitChanceByRange.Length == 0)
                return 0f;

            int index = Mathf.Clamp(Mathf.FloorToInt(rangeMeters / 100f), 0, hitChanceByRange.Length - 1);
            return hitChanceByRange[index];
        }
    }
}
