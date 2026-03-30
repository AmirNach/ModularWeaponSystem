using UnityEngine;

namespace WeaponSystem
{
    /// <summary>Static utility for hit chance and penetration calculations.</summary>
    public static class HitCalculator
    {
        public static float CalcHitChance(
            AccuracyConfig accuracy, AmmoConfig ammo,
            float range, TargetPosture posture, float shooterSpeed)
        {
            if (accuracy == null || ammo == null)
                return 0f;

            float result = accuracy.baseHitChance
                * ammo.GetHitChanceAtRange(range)
                * accuracy.rangeDropoffCurve.Evaluate(range)
                * accuracy.movementPenaltyCurve.Evaluate(shooterSpeed)
                * accuracy.GetPostureModifier(posture);

            return Mathf.Clamp01(result);
        }

        public static float CalcHitChance(
            AccuracyConfig accuracy, AmmoConfig ammo,
            float range, TargetPosture posture, float shooterSpeed, TargetType targetType)
        {
            float baseChance = CalcHitChance(accuracy, ammo, range, posture, shooterSpeed);
            float targetMod = accuracy != null ? accuracy.GetTargetTypeModifier(targetType) : 1f;
            return Mathf.Clamp01(baseChance * targetMod);
        }

        public static bool EvalPenetration(AmmoConfig ammo, ObstacleType obstacle)
        {
            if (ammo == null) return false;

            switch (obstacle)
            {
                case ObstacleType.Building:
                case ObstacleType.HardCover:
                    return ammo.penetratesBuildings;
                case ObstacleType.Vegetation:
                case ObstacleType.SoftCover:
                    return ammo.penetratesVegetation;
                default:
                    return false;
            }
        }
    }
}
