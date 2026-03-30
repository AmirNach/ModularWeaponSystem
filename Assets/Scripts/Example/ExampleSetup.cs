using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace WeaponSystem.Example
{
    public static class ExampleSetup
    {
#if UNITY_EDITOR
        private const string CONFIG_PATH = "Assets/Configs";
        private const string SCENE_PATH = "Assets/Scenes";

        [MenuItem("WeaponSystem/Create Example Configs")]
        public static void CreateExampleConfigs()
        {
            if (!AssetDatabase.IsValidFolder(CONFIG_PATH))
                AssetDatabase.CreateFolder("Assets", "Configs");

            var standardAmmo = CreateAsset<AmmoConfig>("Ammo_Standard");
            standardAmmo.ammoType = AmmoType.Standard;
            standardAmmo.penetratesBuildings = false;
            standardAmmo.penetratesVegetation = true;
            standardAmmo.timeToDestroy = 0.5f;
            standardAmmo.damageMultiplier = 1f;
            standardAmmo.hitChanceByRange = new[] { 0.95f, 0.88f, 0.75f, 0.6f, 0.4f, 0.25f };

            var apAmmo = CreateAsset<AmmoConfig>("Ammo_AP");
            apAmmo.ammoType = AmmoType.AP;
            apAmmo.penetratesBuildings = true;
            apAmmo.penetratesVegetation = true;
            apAmmo.timeToDestroy = 0.3f;
            apAmmo.damageMultiplier = 1.5f;
            apAmmo.hitChanceByRange = new[] { 0.90f, 0.82f, 0.70f, 0.55f, 0.35f, 0.20f };

            var heAmmo = CreateAsset<AmmoConfig>("Ammo_HE");
            heAmmo.ammoType = AmmoType.HE;
            heAmmo.penetratesBuildings = false;
            heAmmo.penetratesVegetation = false;
            heAmmo.timeToDestroy = 0.1f;
            heAmmo.damageMultiplier = 2f;
            heAmmo.hitChanceByRange = new[] { 0.85f, 0.75f, 0.60f, 0.45f, 0.30f, 0.15f };

            var accuracy = CreateAsset<AccuracyConfig>("Accuracy_Rifle");
            accuracy.baseHitChance = 0.85f;
            accuracy.rangeDropoffCurve = AnimationCurve.Linear(0f, 1f, 2000f, 0.15f);
            accuracy.movementPenaltyCurve = AnimationCurve.Linear(0f, 1f, 8f, 0.25f);
            accuracy.postureModifiers = new PostureModifier[]
            {
                new PostureModifier { posture = TargetPosture.Standing, multiplier = 1.0f },
                new PostureModifier { posture = TargetPosture.Crouching, multiplier = 0.65f },
                new PostureModifier { posture = TargetPosture.Prone, multiplier = 0.35f }
            };
            accuracy.targetTypeModifiers = new TargetTypeModifier[]
            {
                new TargetTypeModifier { targetType = TargetType.Infantry, multiplier = 0.8f },
                new TargetTypeModifier { targetType = TargetType.LightVehicle, multiplier = 1.0f },
                new TargetTypeModifier { targetType = TargetType.HeavyArmor, multiplier = 1.3f },
                new TargetTypeModifier { targetType = TargetType.Aircraft, multiplier = 0.35f },
                new TargetTypeModifier { targetType = TargetType.Structure, multiplier = 1.5f }
            };

            var feedback = CreateAsset<FeedbackConfig>("Feedback_Default");
            feedback.showHitIndicator = true;
            feedback.showKillIndicator = true;
            feedback.systemDestroyedAlert = true;
            feedback.hitMarkerDuration = 0.25f;

            var weapon = CreateAsset<WeaponConfig>("Weapon_AssaultRifle");
            weapon.weaponName = "M4A1 Assault Rifle";
            weapon.fireMode = FireMode.Automatic;
            weapon.burstCount = 3;
            weapon.ammoCapacity = 30;
            weapon.maxMagazines = 5;
            weapon.reloadTime = 2.5f;
            weapon.canReloadWhileMoving = true;
            weapon.canReloadWhileShooting = false;
            weapon.ammoSwitchTime = 1.5f;
            weapon.ammoTypes = new[] { standardAmmo, apAmmo, heAmmo };
            weapon.accuracyConfig = accuracy;
            weapon.feedbackConfig = feedback;

            EditorUtility.SetDirty(standardAmmo);
            EditorUtility.SetDirty(apAmmo);
            EditorUtility.SetDirty(heAmmo);
            EditorUtility.SetDirty(accuracy);
            EditorUtility.SetDirty(feedback);
            EditorUtility.SetDirty(weapon);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[WeaponSystem] Example configs created at: " + CONFIG_PATH);
            Selection.activeObject = weapon;
        }

        [MenuItem("WeaponSystem/Setup Test Scene")]
        public static void SetupTestScene()
        {
            var weapon = AssetDatabase.LoadAssetAtPath<WeaponConfig>(CONFIG_PATH + "/Weapon_AssaultRifle.asset");
            if (weapon == null)
            {
                Debug.LogWarning("[WeaponSystem] Run 'Create Example Configs' first!");
                return;
            }

            var inputAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
                "Assets/Scripts/Example/WeaponInputActions.inputactions");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var weaponGO = new GameObject("Weapon");
            var controller = weaponGO.AddComponent<WeaponController>();

            var controllerSO = new SerializedObject(controller);
            controllerSO.FindProperty("config").objectReferenceValue = weapon;
            if (inputAsset != null)
                controllerSO.FindProperty("inputActions").objectReferenceValue = inputAsset;
            controllerSO.ApplyModifiedProperties();

            if (!AssetDatabase.IsValidFolder(SCENE_PATH))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            EditorSceneManager.SaveScene(scene, SCENE_PATH + "/WeaponTestScene.unity");
            Selection.activeGameObject = weaponGO;

            Debug.Log("[WeaponSystem] Test scene saved at: " + SCENE_PATH + "/WeaponTestScene.unity — Press Play!");
        }

        private static T CreateAsset<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, $"{CONFIG_PATH}/{name}.asset");
            return asset;
        }
#endif
    }
}
