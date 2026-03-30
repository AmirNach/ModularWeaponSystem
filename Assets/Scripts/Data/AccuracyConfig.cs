using System.Collections.Generic;
using UnityEngine;

namespace WeaponSystem
{
    [CreateAssetMenu(fileName = "NewAccuracyConfig", menuName = "WeaponSystem/AccuracyConfig")]
    public class AccuracyConfig : ScriptableObject
    {
        [Header("Base")]
        [Range(0f, 1f)] public float baseHitChance = 0.8f;

        [Header("Range")]
        [Tooltip("X = distance (m), Y = multiplier (0-1)")]
        public AnimationCurve rangeDropoffCurve = AnimationCurve.Linear(0f, 1f, 2000f, 0.2f);

        [Header("Movement")]
        [Tooltip("X = speed (m/s), Y = multiplier (0-1)")]
        public AnimationCurve movementPenaltyCurve = AnimationCurve.Linear(0f, 1f, 10f, 0.3f);

        [Header("Posture Modifiers")]
        public PostureModifier[] postureModifiers = new PostureModifier[]
        {
            new PostureModifier { posture = TargetPosture.Standing, multiplier = 1.0f },
            new PostureModifier { posture = TargetPosture.Crouching, multiplier = 0.7f },
            new PostureModifier { posture = TargetPosture.Prone, multiplier = 0.4f }
        };

        [Header("Target Type Modifiers")]
        public TargetTypeModifier[] targetTypeModifiers = new TargetTypeModifier[]
        {
            new TargetTypeModifier { targetType = TargetType.Infantry, multiplier = 0.8f },
            new TargetTypeModifier { targetType = TargetType.LightVehicle, multiplier = 1.0f },
            new TargetTypeModifier { targetType = TargetType.HeavyArmor, multiplier = 1.2f },
            new TargetTypeModifier { targetType = TargetType.Aircraft, multiplier = 0.4f },
            new TargetTypeModifier { targetType = TargetType.Structure, multiplier = 1.5f }
        };

        private Dictionary<TargetPosture, float> _postureMap;
        private Dictionary<TargetType, float> _targetTypeMap;

        public float GetPostureModifier(TargetPosture posture)
        {
            EnsurePostureMap();
            return _postureMap.TryGetValue(posture, out float mod) ? mod : 1f;
        }

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
            _postureMap = null;
            _targetTypeMap = null;
        }
    }

    [System.Serializable]
    public struct PostureModifier
    {
        public TargetPosture posture;
        [Range(0f, 2f)] public float multiplier;
    }

    [System.Serializable]
    public struct TargetTypeModifier
    {
        public TargetType targetType;
        [Range(0f, 3f)] public float multiplier;
    }
}
