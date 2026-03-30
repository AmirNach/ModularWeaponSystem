using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace WeaponSystem
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WeaponConfig config;

#if ENABLE_INPUT_SYSTEM
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
#endif

        private ShootingHandler _shootingHandler;
        private ReloadHandler _reloadHandler;
        private FeedbackHandler _feedbackHandler;

#if ENABLE_INPUT_SYSTEM
        private InputActionMap _weaponMap;
#endif

        public ShootingHandler Shooting => _shootingHandler;
        public ReloadHandler Reloading => _reloadHandler;
        public FeedbackHandler Feedback => _feedbackHandler;
        public WeaponConfig Config => config;

        // -----------------------------------------------------------------
        // Lifecycle
        // -----------------------------------------------------------------

        private void Awake()
        {
            Initialize(config);
        }

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            BindInput();
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            UnbindInput();
#endif
        }

        private void Update()
        {
            _shootingHandler?.Tick();
        }

        // -----------------------------------------------------------------
        // Initialization
        // -----------------------------------------------------------------

        public void Initialize(WeaponConfig newConfig)
        {
            config = newConfig;
            if (config == null)
            {
                Debug.LogWarning($"[WeaponController] No WeaponConfig assigned on {gameObject.name}.");
                return;
            }

            _shootingHandler = new ShootingHandler();
            _reloadHandler = new ReloadHandler();
            _feedbackHandler = new FeedbackHandler();

            _shootingHandler.Initialize(config);
            _reloadHandler.Initialize(config, _shootingHandler, StartCoroutine);
            _feedbackHandler.Initialize(config.feedbackConfig, StartCoroutine);

            _shootingHandler.Shot += HandleRoundShot;
            _reloadHandler.ReloadCompleted += HandleReloadCompleted;
        }

        // -----------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------

        public void Shoot() => _shootingHandler?.Shoot();
        public void StopShoot() => _shootingHandler?.StopShoot();
        public bool CanShoot() => _shootingHandler?.CanShoot() ?? false;
        public int GetCurrentAmmo() => _shootingHandler?.GetCurrentAmmo() ?? 0;

        public void Reload(bool isMoving = false) => _reloadHandler?.Reload(isMoving);

        public bool SwitchAmmoType(AmmoType type)
        {
            if (_shootingHandler == null) return false;

            if (!_shootingHandler.SwitchAmmoType(type))
            {
                Debug.LogWarning($"[WeaponController] Ammo type {type} not available on {config.weaponName}.");
                return false;
            }

            return true;
        }

        // -----------------------------------------------------------------
        // Feedback API
        // -----------------------------------------------------------------

        public void NotifyHit() => _feedbackHandler?.ShowHitMarker();
        public void NotifyKill() => _feedbackHandler?.ShowKillIndicator();
        public void NotifySystemDamaged() => _feedbackHandler?.NotifySystemDamaged();

        // -----------------------------------------------------------------
        // Input binding (optional, only when InputActionAsset is assigned)
        // -----------------------------------------------------------------

#if ENABLE_INPUT_SYSTEM
        private void BindInput()
        {
            if (inputActions == null) return;

            _weaponMap = inputActions.FindActionMap("Weapon");
            if (_weaponMap == null) return;

            var shoot = _weaponMap.FindAction("Shoot");
            var reload = _weaponMap.FindAction("Reload");

            if (shoot != null)
            {
                shoot.started += _ => Shoot();
                shoot.canceled += _ => StopShoot();
            }

            if (reload != null)
                reload.performed += _ => Reload();

            _weaponMap.Enable();
        }

        private void UnbindInput()
        {
            _weaponMap?.Disable();
        }
#endif

        // -----------------------------------------------------------------
        // Internal
        // -----------------------------------------------------------------

        private void HandleRoundShot(AmmoConfig ammo)
        {
            Debug.Log($"[{config.weaponName}] Shot — {ammo.ammoType} | Ammo: {_shootingHandler.GetCurrentAmmo()}/{config.ammoCapacity}");
        }

        private void HandleReloadCompleted()
        {
            Debug.Log($"[{config.weaponName}] Reload complete — Magazines: {_reloadHandler.RemainingMagazines}");
        }

        private void OnDestroy()
        {
            if (_shootingHandler != null)
                _shootingHandler.Shot -= HandleRoundShot;
            if (_reloadHandler != null)
                _reloadHandler.ReloadCompleted -= HandleReloadCompleted;
        }
    }
}
