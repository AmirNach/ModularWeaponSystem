using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WeaponSystem
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WeaponMainConfig config;

        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;

        private ShootingHandler _shootingHandler;
        private ReloadHandler _reloadHandler;
        private FeedbackHandler _feedbackHandler;
        private InputActionMap _weaponMap;

        private Action<InputAction.CallbackContext> _onShootStarted;
        private Action<InputAction.CallbackContext> _onShootCanceled;
        private Action<InputAction.CallbackContext> _onReloadPerformed;

        private bool _isSwitchingAmmo;

        public ShootingHandler Shooting => _shootingHandler;
        public ReloadHandler Reloading => _reloadHandler;
        public FeedbackHandler Feedback => _feedbackHandler;
        public WeaponMainConfig Config => config;
        public bool IsSwitchingAmmo => _isSwitchingAmmo;

        private void Awake() => Initialize(config);

        private void Update()
        {
            _shootingHandler?.Tick();
            HandleAmmoSwitchInput();
        }

        private void OnEnable() => BindInput();
        private void OnDisable() => UnbindInput();

        /// <summary>Initializes or re-initializes with a new config at runtime.</summary>
        public void Initialize(WeaponMainConfig newConfig)
        {
            if (_shootingHandler != null)
                _shootingHandler.Shot -= HandleRoundShot;
            if (_reloadHandler != null)
            {
                _reloadHandler.ReloadStarted -= HandleReloadStarted;
                _reloadHandler.ReloadCompleted -= HandleReloadCompleted;
            }

            _isSwitchingAmmo = false;

            config = newConfig;
            if (config == null)
            {
                Debug.LogWarning($"[WeaponController] No config assigned on {gameObject.name}.");
                return;
            }

            _shootingHandler = new ShootingHandler();
            _reloadHandler   = new ReloadHandler();
            _feedbackHandler = new FeedbackHandler();

            _shootingHandler.Initialize(config);
            _reloadHandler.Initialize(config, _shootingHandler, StartCoroutine);
            _feedbackHandler.Initialize(config.feedbackConfig, StartCoroutine);

            _shootingHandler.Shot        += HandleRoundShot;
            _reloadHandler.ReloadStarted += HandleReloadStarted;
            _reloadHandler.ReloadCompleted += HandleReloadCompleted;
        }

        // ── Public API ────────────────────────────────────────────────

        public void Shoot()       => _shootingHandler?.Shoot();
        public void StopShoot()   => _shootingHandler?.StopShoot();
        public bool CanShoot()    => _shootingHandler?.CanShoot() ?? false;
        public int GetCurrentAmmo() => _shootingHandler?.GetCurrentAmmo() ?? 0;
        public void Reload(bool isMoving = false) => _reloadHandler?.Reload(isMoving);

        /// <summary>
        /// Switches ammo by type. Respects ammoSwitchTime — blocks shooting during switch.
        /// </summary>
        public bool SwitchAmmoType(AmmoType type)
        {
            if (_shootingHandler == null || config == null) return false;
            if (_isSwitchingAmmo) return false;
            if (_shootingHandler.ActiveAmmo != null && _shootingHandler.ActiveAmmo.ammoType == type) return false;

            if (!_shootingHandler.HasAmmoType(type))
            {
                Debug.LogWarning($"[WeaponController] Ammo type {type} not available on {config.weaponName}.");
                return false;
            }

            StartCoroutine(AmmoSwitchCoroutine(type));
            return true;
        }

        /// <summary>Switches ammo by slot index (0-based). Called by number keys.</summary>
        public bool SwitchAmmoByIndex(int index)
        {
            if (config == null || config.ammoTypes == null) return false;
            if (index < 0 || index >= config.ammoTypes.Length) return false;
            if (config.ammoTypes[index] == null) return false;

            return SwitchAmmoType(config.ammoTypes[index].ammoType);
        }

        // ── Feedback API ──────────────────────────────────────────────

        public void NotifyHit()           => _feedbackHandler?.ShowHitMarker();
        public void NotifyKill()          => _feedbackHandler?.ShowKillIndicator();
        public void NotifySystemDamaged() => _feedbackHandler?.NotifySystemDamaged();

        // ── Ammo Switching ────────────────────────────────────────────

        private void HandleAmmoSwitchInput()
        {
            if (config == null || config.ammoTypes == null) return;

            int max = Mathf.Min(config.ammoTypes.Length, 9);
            for (int i = 0; i < max; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SwitchAmmoByIndex(i);
                    return;
                }
            }
        }

        private IEnumerator AmmoSwitchCoroutine(AmmoType type)
        {
            _isSwitchingAmmo = true;
            _shootingHandler.StopShoot();

            Debug.Log($"[{config.weaponName}] Switching to {type}... ({config.ammoSwitchTime}s)");

            yield return new WaitForSeconds(config.ammoSwitchTime);

            _shootingHandler.SwitchAmmoType(type);
            _isSwitchingAmmo = false;

            Debug.Log($"[{config.weaponName}] Ammo ready: {type}");
        }

        // ── Input ─────────────────────────────────────────────────────

        private void BindInput()
        {
            if (inputActions == null) return;

            _weaponMap = inputActions.FindActionMap("Weapon");
            if (_weaponMap == null) return;

            _onShootStarted   = _ => Shoot();
            _onShootCanceled  = _ => StopShoot();
            _onReloadPerformed = _ => Reload();

            var shoot  = _weaponMap.FindAction("Shoot");
            var reload = _weaponMap.FindAction("Reload");

            if (shoot != null)
            {
                shoot.started  += _onShootStarted;
                shoot.canceled += _onShootCanceled;
            }

            if (reload != null)
                reload.performed += _onReloadPerformed;

            _weaponMap.Enable();
        }

        private void UnbindInput()
        {
            if (_weaponMap == null) return;

            var shoot  = _weaponMap.FindAction("Shoot");
            var reload = _weaponMap.FindAction("Reload");

            if (shoot != null && _onShootStarted != null)
            {
                shoot.started  -= _onShootStarted;
                shoot.canceled -= _onShootCanceled;
            }

            if (reload != null && _onReloadPerformed != null)
                reload.performed -= _onReloadPerformed;

            _weaponMap.Disable();

            _onShootStarted    = null;
            _onShootCanceled   = null;
            _onReloadPerformed = null;
        }

        // ── Internal callbacks ────────────────────────────────────────

        private void HandleRoundShot(AmmoConfig ammo)
        {
            var ammoName = ammo != null ? ammo.ammoType.ToString() : "Unknown";
            Debug.Log($"[{config.weaponName}] Shot — {ammoName} | Ammo: {_shootingHandler.GetCurrentAmmo()}/{config.ammoCapacity}");
        }

        private void HandleReloadStarted()
        {
            Debug.Log($"[{config.weaponName}] Reloading... ({_reloadHandler.RemainingMagazines} magazines left)");
        }

        private void HandleReloadCompleted()
        {
            Debug.Log($"[{config.weaponName}] Reload complete — Ammo: {_shootingHandler.GetCurrentAmmo()}/{config.ammoCapacity}");
        }

        private void OnDestroy()
        {
            UnbindInput();

            if (_shootingHandler != null)
                _shootingHandler.Shot -= HandleRoundShot;

            if (_reloadHandler != null)
            {
                _reloadHandler.ReloadStarted   -= HandleReloadStarted;
                _reloadHandler.ReloadCompleted -= HandleReloadCompleted;
            }
        }
    }
}
