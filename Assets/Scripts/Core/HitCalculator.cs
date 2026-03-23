using UnityEngine;

namespace WeaponSystem
{
    /// <summary>
    /// Static utility — all hit chance and penetration math lives here.
    /// No state, no MonoBehaviour. Pure functions.
    /// </summary>
    public static class HitCalculator
    {
        /// <summary>
        /// Calculates final hit chance by combining all modifiers multiplicatively.
        /// </summary>
        /// <param name="accuracy">Accuracy config with base chance and curves</param>
        /// <param name="ammo">Active ammo config (hit chance by range)</param>
        /// <param name="range">Distance to target in meters</param>
        /// <param name="posture">Target posture</param>
        /// <param name="shooterSpeed">Shooter movement speed in m/s</param>
        /// <returns>Final hit probability clamped 0-1</returns>
        public static float CalcHitChance(
            AccuracyConfig accuracy,
            AmmoConfig ammo,
            float range,
            TargetPosture posture,
            float shooterSpeed)
        {
            if (accuracy == null || ammo == null)
                return 0f;

            float baseChance = accuracy.baseHitChance;
            float ammoRangeMod = ammo.GetHitChanceAtRange(range);
            float rangeDropoff = accuracy.rangeDropoffCurve.Evaluate(range);
            float movementPenalty = accuracy.movementPenaltyCurve.Evaluate(shooterSpeed);
            float postureMod = accuracy.GetPostureModifier(posture);

            float finalChance = baseChance * ammoRangeMod * rangeDropoff * movementPenalty * postureMod;
            return Mathf.Clamp01(finalChance);
        }

        /// <summary>
        /// Overload that also factors in target type.
        /// </summary>
        public static float CalcHitChance(
            AccuracyConfig accuracy,
            AmmoConfig ammo,
            float range,
            TargetPosture posture,
            float shooterSpeed,
            TargetType targetType)
        {
            float base01 = CalcHitChance(accuracy, ammo, range, posture, shooterSpeed);
            float targetMod = accuracy != null ? accuracy.GetTargetTypeModifier(targetType) : 1f;
            return Mathf.Clamp01(base01 * targetMod);
        }

        /// <summary>
        /// Returns whether the given ammo can penetrate the given obstacle.
        /// </summary>
        public static bool EvalPenetration(AmmoConfig ammo, ObstacleType obstacle)
        {
            if (ammo == null)
                return false;

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
