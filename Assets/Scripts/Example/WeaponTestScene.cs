#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace WeaponSystem.Example
{
    /// <summary>
    /// Drop-in test script with basic 3D visuals.
    /// Uses the New Input System via InputActionAsset.
    ///
    /// Controls (keyboard / gamepad):
    ///   LMB / Space / RT       — Fire
    ///   Release                 — StopFire (for auto mode)
    ///   R / X(gamepad)          — Reload
    ///   1-5 / D-Pad             — Switch ammo type
    ///   H                       — Simulate hit feedback
    ///   K                       — Simulate kill feedback
    ///   D                       — Simulate system damaged
    ///   T                       — Run HitCalculator test
    /// </summary>
    public class WeaponTestScene : MonoBehaviour
    {
        [Header("Assign a WeaponConfig asset here")]
        [SerializeField] private WeaponConfig weaponConfig;

        [Header("Input Actions Asset")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Test Parameters")]
        [SerializeField] private float testShooterSpeed = 0f;
        [SerializeField] private TargetPosture testPosture = TargetPosture.Standing;
        [SerializeField] private TargetType testTargetType = TargetType.Infantry;

        private WeaponController _controller;
        private string _log = "";

        // --- Input Actions ---
        private InputActionMap _weaponMap;
        private InputAction _fireAction;
        private InputAction _reloadAction;
        private InputAction[] _ammoActions;
        private InputAction _hitAction;
        private InputAction _killAction;
        private InputAction _damageAction;
        private InputAction _hitTestAction;

        // --- Visuals ---
        private Transform _gunPivot;
        private Transform _muzzlePoint;
        private Light _muzzleFlash;
        private float _muzzleFlashTimer;
        private float _recoilOffset;
        private Transform _target;
        private Renderer _targetRenderer;
        private Color _targetBaseColor;
        private float _hitFlashTimer;
        private float _killFlashTimer;
        private Transform _damageOverlay;
        private float _damageTimer;

        // -----------------------------------------------------------------
        // Setup
        // -----------------------------------------------------------------

        private void Start()
        {
            BuildScene();
            SetupInput();
            SetupWeapon();
        }

        private void SetupInput()
        {
            if (inputActions == null)
            {
                Log("<color=red>ERROR: No InputActionAsset assigned! Drag WeaponInputActions into the field.</color>");
                return;
            }

            _weaponMap = inputActions.FindActionMap("Weapon", throwIfNotFound: true);

            _fireAction = _weaponMap.FindAction("Fire");
            _reloadAction = _weaponMap.FindAction("Reload");
            _hitAction = _weaponMap.FindAction("SimulateHit");
            _killAction = _weaponMap.FindAction("SimulateKill");
            _damageAction = _weaponMap.FindAction("SimulateDamage");
            _hitTestAction = _weaponMap.FindAction("RunHitTest");

            _ammoActions = new InputAction[5];
            for (int i = 0; i < 5; i++)
                _ammoActions[i] = _weaponMap.FindAction($"SwitchAmmo{i + 1}");

            // Subscribe events
            _fireAction.started += _ => _controller?.Fire();
            _fireAction.canceled += _ => _controller?.StopFire();

            _reloadAction.performed += _ => OnReloadPressed();

            for (int i = 0; i < 5; i++)
            {
                int index = i; // capture for closure
                if (_ammoActions[i] != null)
                    _ammoActions[i].performed += _ => TrySwitchAmmo(index);
            }

            _hitAction.performed += _ => _controller?.NotifyHit();
            _killAction.performed += _ => _controller?.NotifyKill();
            _damageAction.performed += _ => _controller?.NotifySystemDamaged();
            _hitTestAction.performed += _ => RunHitCalcTest();

            _weaponMap.Enable();
        }

        private void OnReloadPressed()
        {
            if (_controller == null) return;

            if (_controller.Reloading.CanReload())
            {
                Log("Reloading...");
                _controller.Reload(isMoving: testShooterSpeed > 0.1f);
            }
            else
                Log("<color=#888>Can't reload</color>");
        }

        private void OnDestroy()
        {
            _weaponMap?.Disable();
        }

        private void BuildScene()
        {
            // --- Camera ---
            var cam = Camera.main;
            if (cam == null)
            {
                var camGO = new GameObject("MainCamera");
                cam = camGO.AddComponent<Camera>();
                camGO.tag = "MainCamera";
            }
            cam.transform.position = new Vector3(0f, 1.5f, -3f);
            cam.transform.rotation = Quaternion.Euler(5f, 0f, 0f);
            cam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            // --- Floor ---
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(3f, 1f, 3f);
            var floorMat = new Material(Shader.Find("Standard"));
            floorMat.color = new Color(0.25f, 0.25f, 0.28f);
            floor.GetComponent<Renderer>().material = floorMat;

            // --- Gun (simple primitives) ---
            _gunPivot = new GameObject("GunPivot").transform;
            _gunPivot.position = new Vector3(0.4f, 1.1f, -1.2f);
            _gunPivot.rotation = Quaternion.Euler(0f, 0f, 0f);

            var gunMat = new Material(Shader.Find("Standard"));
            gunMat.color = new Color(0.2f, 0.2f, 0.22f);

            // Body
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "GunBody";
            body.transform.SetParent(_gunPivot);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.08f, 0.1f, 0.6f);
            body.GetComponent<Renderer>().material = gunMat;

            // Barrel
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel";
            barrel.transform.SetParent(_gunPivot);
            barrel.transform.localPosition = new Vector3(0f, 0.02f, 0.45f);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            barrel.transform.localScale = new Vector3(0.03f, 0.2f, 0.03f);
            var barrelMat = new Material(Shader.Find("Standard"));
            barrelMat.color = new Color(0.3f, 0.3f, 0.32f);
            barrel.GetComponent<Renderer>().material = barrelMat;

            // Magazine
            var mag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mag.name = "Magazine";
            mag.transform.SetParent(_gunPivot);
            mag.transform.localPosition = new Vector3(0f, -0.1f, -0.05f);
            mag.transform.localScale = new Vector3(0.06f, 0.12f, 0.08f);
            mag.GetComponent<Renderer>().material = gunMat;

            // Grip
            var grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grip.name = "Grip";
            grip.transform.SetParent(_gunPivot);
            grip.transform.localPosition = new Vector3(0f, -0.11f, -0.18f);
            grip.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            grip.transform.localScale = new Vector3(0.06f, 0.12f, 0.05f);
            grip.GetComponent<Renderer>().material = gunMat;

            // Muzzle point + flash light
            var muzzleGO = new GameObject("MuzzlePoint");
            muzzleGO.transform.SetParent(_gunPivot);
            muzzleGO.transform.localPosition = new Vector3(0f, 0.02f, 0.65f);
            _muzzlePoint = muzzleGO.transform;

            _muzzleFlash = muzzleGO.AddComponent<Light>();
            _muzzleFlash.type = LightType.Point;
            _muzzleFlash.color = new Color(1f, 0.8f, 0.3f);
            _muzzleFlash.intensity = 0f;
            _muzzleFlash.range = 3f;

            // --- Target dummy ---
            var targetParent = new GameObject("TargetDummy");
            _target = targetParent.transform;
            _target.position = new Vector3(0f, 0f, 8f);

            var targetMat = new Material(Shader.Find("Standard"));
            targetMat.color = new Color(0.8f, 0.35f, 0.3f);
            targetMat.EnableKeyword("_EMISSION");

            var torso = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torso.name = "Torso";
            torso.transform.SetParent(_target);
            torso.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            torso.transform.localScale = new Vector3(0.4f, 0.9f, 0.4f);
            torso.GetComponent<Renderer>().material = targetMat;
            _targetRenderer = torso.GetComponent<Renderer>();
            _targetBaseColor = targetMat.color;

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(_target);
            head.transform.localPosition = new Vector3(0f, 2f, 0f);
            head.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            head.GetComponent<Renderer>().material = targetMat;

            var basePlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePlate.name = "Base";
            basePlate.transform.SetParent(_target);
            basePlate.transform.localPosition = Vector3.zero;
            basePlate.transform.localScale = new Vector3(0.6f, 0.02f, 0.6f);
            var baseMat = new Material(Shader.Find("Standard"));
            baseMat.color = new Color(0.4f, 0.4f, 0.4f);
            basePlate.GetComponent<Renderer>().material = baseMat;

            // --- Light ---
            var lightGO = new GameObject("SceneLight");
            var sceneLight = lightGO.AddComponent<Light>();
            sceneLight.type = LightType.Directional;
            sceneLight.color = new Color(0.9f, 0.85f, 0.8f);
            sceneLight.intensity = 0.8f;
            lightGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        private void SetupWeapon()
        {
            var weaponGO = new GameObject($"Weapon_{weaponConfig?.weaponName ?? "NULL"}");
            _controller = weaponGO.AddComponent<WeaponController>();

            if (weaponConfig == null)
            {
                Log("<color=red>ERROR: No WeaponConfig assigned!</color>");
                return;
            }

            _controller.Initialize(weaponConfig);

            // Subscribe events
            _controller.Firing.OnFired += OnFired;
            _controller.Reloading.OnReloadComplete += () =>
                Log($"<color=cyan>RELOAD COMPLETE</color> — Mags: {_controller.Reloading.RemainingMagazines}");
            _controller.Feedback.OnHitMarkerShow += () => _hitFlashTimer = 0.15f;
            _controller.Feedback.OnKillIndicator += () => _killFlashTimer = 0.4f;
            _controller.Feedback.OnSystemDamaged += () => _damageTimer = 0.6f;

            Log($"<color=white><b>{weaponConfig.weaponName}</b> ready</color>");
            Log($"  {weaponConfig.fireMode} | {weaponConfig.ammoCapacity} rounds | {weaponConfig.maxMagazines} mags");
            Log("  Space/LMB=Fire  R=Reload  1-3=Ammo  H=Hit  K=Kill  T=Test");
        }

        // -----------------------------------------------------------------
        // Fired callback
        // -----------------------------------------------------------------

        private void OnFired(AmmoConfig ammo)
        {
            _muzzleFlashTimer = 0.06f;
            _recoilOffset = -0.08f;
            Log($"<color=yellow>FIRE</color> [{ammo.ammoType}] — {_controller.GetCurrentAmmo()}/{weaponConfig.ammoCapacity}");
        }

        // -----------------------------------------------------------------
        // Update — visuals only (input is event-driven now)
        // -----------------------------------------------------------------

        private void Update()
        {
            if (_controller == null || weaponConfig == null) return;
            TickVisuals();
        }

        private void TickVisuals()
        {
            float dt = Time.deltaTime;

            _muzzleFlashTimer -= dt;
            _muzzleFlash.intensity = _muzzleFlashTimer > 0f ? 4f : 0f;

            _recoilOffset = Mathf.Lerp(_recoilOffset, 0f, dt * 20f);
            if (_gunPivot != null)
            {
                var pos = _gunPivot.localPosition;
                pos.z = -1.2f + _recoilOffset;
                _gunPivot.localPosition = pos;
            }

            if (_hitFlashTimer > 0f)
            {
                _hitFlashTimer -= dt;
                _targetRenderer.material.color = Color.white;
            }
            else if (_killFlashTimer > 0f)
            {
                _killFlashTimer -= dt;
                float t = _killFlashTimer / 0.4f;
                _targetRenderer.material.color = Color.Lerp(_targetBaseColor, Color.red, t);
            }
            else
            {
                _targetRenderer.material.color = _targetBaseColor;
            }

            _damageTimer -= dt;
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private void TrySwitchAmmo(int index)
        {
            if (weaponConfig.ammoTypes == null || index >= weaponConfig.ammoTypes.Length)
            {
                Log($"<color=#888>No ammo at index {index}</color>");
                return;
            }
            var ammo = weaponConfig.ammoTypes[index];
            if (_controller.SwitchAmmoType(ammo.ammoType))
                Log($"Ammo → <color=yellow>{ammo.name}</color> ({ammo.ammoType} | dmg x{ammo.damageMultiplier} | pen: bldg={ammo.penetratesBuildings} veg={ammo.penetratesVegetation})");
        }

        private void RunHitCalcTest()
        {
            if (weaponConfig.accuracyConfig == null || _controller.Firing.ActiveAmmo == null)
            {
                Log("<color=red>Missing config for HitCalc test</color>");
                return;
            }

            var acc = weaponConfig.accuracyConfig;
            var ammo = _controller.Firing.ActiveAmmo;

            Log("--- Hit Chance ---");
            float[] ranges = { 100f, 300f, 500f, 1000f, 2000f };
            foreach (float r in ranges)
            {
                float c = HitCalculator.CalcHitChance(acc, ammo, r, testPosture, testShooterSpeed, testTargetType);
                Log($"  {r}m → <b>{c:P0}</b>");
            }

            Log("--- Penetration ---");
            Log($"  Building: {HitCalculator.EvalPenetration(ammo, ObstacleType.Building)} | Vegetation: {HitCalculator.EvalPenetration(ammo, ObstacleType.Vegetation)}");
        }

        // -----------------------------------------------------------------
        // On-screen HUD
        // -----------------------------------------------------------------

        private const int MAX_LINES = 18;

        private void Log(string msg)
        {
            Debug.Log($"[WeaponTest] {msg}");
            _log += msg + "\n";
            var lines = _log.Split('\n');
            if (lines.Length > MAX_LINES)
                _log = string.Join("\n", lines, lines.Length - MAX_LINES, MAX_LINES);
        }

        private void OnGUI()
        {
            DrawAmmoHUD();
            DrawCrosshair();
            if (_hitFlashTimer > 0f) DrawHitMarker();
            if (_damageTimer > 0f) DrawDamageOverlay();
            DrawLogPanel();
        }

        private void DrawAmmoHUD()
        {
            if (_controller == null) return;

            float bx = Screen.width - 220;
            float by = Screen.height - 80;

            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(new Rect(bx - 10, by - 10, 220, 75), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var big = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, richText = true };
            big.normal.textColor = Color.white;
            var small = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true };
            small.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

            int ammo = _controller.GetCurrentAmmo();
            int cap = weaponConfig.ammoCapacity;
            string ammoColor = ammo > cap * 0.3f ? "white" : ammo > 0 ? "yellow" : "red";

            GUI.Label(new Rect(bx, by - 5, 200, 40),
                $"<color={ammoColor}>{ammo}</color> <color=#888>/ {cap}</color>", big);

            string activeAmmo = _controller.Firing?.ActiveAmmo?.name ?? "—";
            int mags = _controller.Reloading?.RemainingMagazines ?? 0;
            bool reloading = _controller.Reloading?.IsReloading ?? false;
            string reloadStr = reloading ? "  <color=cyan>[RELOADING]</color>" : "";

            GUI.Label(new Rect(bx, by + 30, 200, 20),
                $"{activeAmmo}  |  Mags: {mags}{reloadStr}", small);
        }

        private void DrawCrosshair()
        {
            float cx = Screen.width / 2f;
            float cy = Screen.height / 2f;
            float size = 12f;
            float thick = 2f;

            GUI.color = new Color(0.8f, 1f, 0.8f, 0.8f);
            var tex = Texture2D.whiteTexture;
            GUI.DrawTexture(new Rect(cx - size, cy - thick / 2, size * 2, thick), tex);
            GUI.DrawTexture(new Rect(cx - thick / 2, cy - size, thick, size * 2), tex);
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTexture(new Rect(cx - 1.5f, cy - 1.5f, 3, 3), tex);
            GUI.color = Color.white;
        }

        private void DrawHitMarker()
        {
            float cx = Screen.width / 2f;
            float cy = Screen.height / 2f;
            float len = 10f;
            float gap = 5f;
            float thick = 2f;

            GUI.color = new Color(1f, 1f, 1f, _hitFlashTimer / 0.15f);

            DrawRotatedLine(cx - gap, cy - gap, -len, -len, thick);
            DrawRotatedLine(cx + gap, cy - gap, len, -len, thick);
            DrawRotatedLine(cx - gap, cy + gap, -len, len, thick);
            DrawRotatedLine(cx + gap, cy + gap, len, len, thick);

            GUI.color = Color.white;
        }

        private void DrawRotatedLine(float x, float y, float dx, float dy, float thick)
        {
            int steps = 4;
            var tex = Texture2D.whiteTexture;
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / steps;
                GUI.DrawTexture(new Rect(x + dx * t, y + dy * t, thick, thick), tex);
            }
        }

        private void DrawDamageOverlay()
        {
            float alpha = Mathf.Clamp01(_damageTimer / 0.6f) * 0.35f;
            GUI.color = new Color(1f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawLogPanel()
        {
            float pw = 460;
            float ph = 320;

            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTexture(new Rect(8, 8, pw, ph), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var logStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontSize = 12
            };
            logStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            GUI.Label(new Rect(14, 12, pw - 12, ph - 8), _log, logStyle);
        }
    }
}
#else
// Fallback: If New Input System is not enabled, show a helpful error
namespace WeaponSystem.Example
{
    public class WeaponTestScene : UnityEngine.MonoBehaviour
    {
        private void Start()
        {
            UnityEngine.Debug.LogError(
                "[WeaponSystem] New Input System is required! " +
                "Go to Edit → Project Settings → Player → Active Input Handling → set to 'Both' or 'Input System Package (New)'");
        }
    }
}
#endif