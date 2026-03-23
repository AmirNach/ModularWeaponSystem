using System.Collections.Generic;
using UnityEngine;

namespace WeaponSystem
{
    /// <summary>
    /// Configuration for accuracy modifiers — posture, movement, range dropoff, target type.
    /// One per weapon (referenced by WeaponConfig).
    /// </summary>
    [CreateAssetMenu(fileName = "NewAccuracyConfig", menuName = "WeaponSystem/AccuracyConfig")]
    public class AccuracyConfig : ScriptableObject
    {
        [Header("Base")]
        [Range(0f, 1f)]
        public float baseHitChance = 0.8f;

        [Header("Range")]
        [Tooltip("X = distance (m), Y = accuracy multiplier (0-1)")]
        public AnimationCurve rangeDropoffCurve = AnimationCurve.Linear(0f, 1f, 2000f, 0.2f);

        [Header("Movement")]
        [Tooltip("X = speed (m/s), Y = accuracy multiplier (0-1)")]
        public AnimationCurve movementPenaltyCurve = AnimationCurve.Linear(0f, 1f, 10f, 0.3f);

        [Header("Posture Modifiers")]
        [Tooltip("Multiplier per target posture (lower = harder to hit)")]
        public PostureModifier[] postureModifiers = new PostureModifier[]
        {
            new PostureModifier { posture = TargetPosture.Standing, multiplier = 1.0f },
            new PostureModifier { posture = TargetPosture.Crouching, multiplier = 0.7f },
            new PostureModifier { posture = TargetPosture.Prone, multiplier = 0.4f }
        };

        [Header("Target Type Modifiers")]
        [Tooltip("Multiplier per target type")]
        public TargetTypeModifier[] targetTypeModifiers = new TargetTypeModifier[]
        {
            new TargetTypeModifier { targetType = TargetType.Infantry, multiplier = 0.8f },
            new TargetTypeModifier { targetType = TargetType.LightVehicle, multiplier = 1.0f },
            new TargetTypeModifier { targetType = TargetType.HeavyArmor, multiplier = 1.2f },
            new TargetTypeModifier { targetType = TargetType.Aircraft, multiplier = 0.4f },
            new TargetTypeModifier { targetType = TargetType.Structure, multiplier = 1.5f }
        };

        // Cached lookups — built on first access
        private Dictionary<TargetPosture, float> _postureMap;
        private Dictionary<TargetType, float> _targetTypeMap;

        /// <summary>
        /// Returns the accuracy multiplier for a given posture (defaults to 1 if not configured).
        /// </summary>
        public float GetPostureModifier(TargetPosture posture)
        {
            EnsurePostureMap();
            return _postureMap.TryGetValue(posture, out float mod) ? mod : 1f;
        }

        /// <summary>
        /// Returns the accuracy multiplier for a given target type (defaults to 1 if not configured).
        /// </summary>
        public float GetTargetTypeModifier(TargetType type)
        {
            EnsureTargetTypeMap();
            return _targetTypeMap.TryGetValue(type, out float mod) ? mod : 1f;
        }

        private void EnsurePostureMap()
        {
            if (_postureMap != null) return;
            _postureMap = new Dictionary<TargetPosture, float>();
            foreach (var pm in postureModifiers)
                _postureMap[pm.posture] = pm.multiplier;
        }

        private void EnsureTargetTypeMap()
        {
            if (_targetTypeMap != null) return;
            _targetTypeMap = new Dictionary<TargetType, float>();
            foreach (var tm in targetTypeModifiers)
                _targetTypeMap[tm.targetType] = tm.multiplier;
        }

        private void OnValidate()
        {
            // Invalidate caches when edited in Inspector
            _postureMap = null;
            _targetTypeMap = null;
        }
    }

    /// <summary>
    /// Serializable pair: TargetPosture -> multiplier.
    /// Used instead of Dictionary for Inspector support.
    /// </summary>
    [System.Serializable]
    public struct PostureModifier
    {
        public TargetPosture posture;
        [Range(0f, 2f)] public float multiplier;
    }

    /// <summary>
    /// Serializable pair: TargetType -> multiplier.
    /// </summary>
    [System.Serializable]
    public struct TargetTypeModifier
    {
        public TargetType targetType;
        [Range(0f, 3f)] public float multiplier;
    }
}
