using UnityEngine;

namespace WeaponSystem
{
    /// <summary>
    /// The single MonoBehaviour entry point for one weapon.
    /// Orchestrates all handlers — no game logic lives here, only wiring.
    ///
    /// Usage:
    ///   1. Attach to a GameObject.
    ///   2. Assign a WeaponConfig in the Inspector.
    ///   3. Call Fire() / Reload() from your input system.
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        // -----------------------------------------------------------------
        // Inspector
        // -----------------------------------------------------------------

        [Header("Configuration")]
        [SerializeField] private WeaponConfig config;

        // -----------------------------------------------------------------
        // Handlers (internal, created at runtime — no manual wiring needed)
        // -----------------------------------------------------------------

        private FiringHandler _firingHandler;
        private ReloadHandler _reloadHandler;
        private FeedbackHandler _feedbackHandler;

        // -----------------------------------------------------------------
        // Public accessors (read-only references for external systems)
        // -----------------------------------------------------------------

        public FiringHandler Firing => _firingHandler;
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

        private void Update()
        {
            _firingHandler?.Tick();
        }

        // -----------------------------------------------------------------
        // Initialization
        // -----------------------------------------------------------------

        /// <summary>
        /// (Re-)initializes the weapon with a new config.
        /// Can be called at runtime to swap weapon configs on the fly.
        /// </summary>
        public void Initialize(WeaponConfig newConfig)
        {
            config = newConfig;
            if (config == null)
            {
                Debug.LogWarning($"[WeaponController] No WeaponConfig assigned on {gameObject.name}.");
                return;
            }

            // Create handlers
            _firingHandler = new FiringHandler();
            _reloadHandler = new ReloadHandler();
            _feedbackHandler = new FeedbackHandler();

            // Initialize with config
            _firingHandler.Initialize(config);
            _reloadHandler.Initialize(config, _firingHandler);
            _feedbackHandler.Initialize(config.feedbackConfig);

            // Wire feedback to firing
            _firingHandler.OnFired += HandleRoundFired;
        }

        // -----------------------------------------------------------------
        // Public API (call these from your input / AI system)
        // -----------------------------------------------------------------

        public void Fire() => _firingHandler?.Fire();
        public void StopFire() => _firingHandler?.StopFire();
        public bool CanFire() => _firingHandler?.CanFire() ?? false;
        public int GetCurrentAmmo() => _firingHandler?.GetCurrentAmmo() ?? 0;

        /// <summary>
        /// Starts a reload. Pass isMoving = true if the shooter is currently moving.
        /// </summary>
        public void Reload(bool isMoving = false)
        {
            if (_reloadHandler != null && _reloadHandler.CanReload())
                StartCoroutine(_reloadHandler.ReloadCoroutine(isMoving));
        }

        /// <summary>
        /// Switches to a different ammo type by AmmoType enum.
        /// Returns false if the weapon doesn't carry that ammo type.
        /// </summary>
        public bool SwitchAmmoType(AmmoType type)
        {
            if (config.ammoTypes == null) return false;

            foreach (var ammo in config.ammoTypes)
            {
                if (ammo != null && ammo.ammoType == type)
                    return _firingHandler.SwitchAmmo(ammo);
            }

            Debug.LogWarning($"[WeaponController] Ammo type {type} not available on {config.weaponName}.");
            return false;
        }

        // -----------------------------------------------------------------
        // Feedback wiring
        // -----------------------------------------------------------------

        /// <summary>Called by FiringHandler each time a round is fired (hit marker, etc.).</summary>
        public void NotifyHit()
        {
            if (_feedbackHandler != null)
                StartCoroutine(_feedbackHandler.ShowHitMarker());
        }

        public void NotifyKill() => _feedbackHandler?.ShowKillIndicator();
        public void NotifySystemDamaged() => _feedbackHandler?.OnSystemDamagedNotify();

        // -----------------------------------------------------------------
        // Internal
        // -----------------------------------------------------------------

        private void HandleRoundFired(AmmoConfig ammo)
        {
            // Hook point: play VFX/SFX, spawn projectile, etc.
            // External systems can also subscribe to Firing.OnFired directly.
        }

        private void OnDestroy()
        {
            if (_firingHandler != null)
                _firingHandler.OnFired -= HandleRoundFired;
        }
    }
}
