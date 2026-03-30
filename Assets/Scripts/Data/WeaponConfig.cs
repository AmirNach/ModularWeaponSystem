using UnityEngine;

namespace WeaponSystem
{
    [CreateAssetMenu(fileName = "NewWeaponConfig", menuName = "WeaponSystem/WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Identity")]
        public string weaponName = "New Weapon";

        [Header("Firing")]
        public FireMode fireMode = FireMode.SemiAuto;
        [Tooltip("Rounds per burst (only used when fireMode = Burst)")]
        [Min(1)] public int burstCount = 3;

        [Header("Ammunition")]
        [Min(1)] public int ammoCapacity = 30;
        [Min(1)] public int maxMagazines = 5;

        [Header("Reloading")]
        [Min(0f)] public float reloadTime = 2.5f;
        public bool canReloadWhileMoving = true;
        public bool canReloadWhileShooting;

        [Header("Ammo Types")]
        [Tooltip("Index 0 is the default ammo type")]
        public AmmoConfig[] ammoTypes;
        [Min(0f)] public float ammoSwitchTime = 1.5f;

        [Header("Sub-Configs")]
        public AccuracyConfig accuracyConfig;
        public FeedbackConfig feedbackConfig;
    }
}
