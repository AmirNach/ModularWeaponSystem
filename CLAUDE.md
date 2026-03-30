# Modular Weapon System - Unity Project

## Project Overview
A modular weapon system designed as a standalone, reusable module that will connect to other Unity projects in the future. The system must be clean, DRY, compact, easy to use, and easy to extend (add/remove weapons, configure settings in one place).

**Note:** Turret and Optics are handled by separate modular systems and are NOT part of this weapon module.

## Architecture Principles
- **Modular**: Plug-and-play into any Unity project
- **DRY**: No repeated code — use interfaces, base classes, and ScriptableObjects
- **Compact**: Minimal class count, maximal clarity
- **Configurable**: All weapon settings defined in one place (ScriptableObjects)
- **Extensible**: Easy to add new weapons or remove existing ones

## Weapon Characteristics (from spec)
Each weapon must support the following properties:

### Ammunition & Reloading
- Ammo capacity (per magazine)
- Number of reloads / total magazines
- Reload time
- Reload constraints (can reload while moving? while shooting?)

### Firing
- Fire mode: semi-auto, automatic, burst
- Hit chance at different ranges (per 100m intervals)
- Hit chance vs target posture (prone vs standing)
- Hit chance vs different target types
- Hit chance while moving at different speeds
- Penetration (buildings / vegetation)

### Ammo Types & Damage
- Ammo types available
- Time to switch between ammo types
- Time to impact (projectile travel time)
- Time from hit to destruction

### System Feedback
- Hit/kill indicators (feedback to shooter)
- Damage/destruction indicator to the operator when system is hit

## Architecture (UML V5 — Approved)

### Enums
- FireMode (SemiAuto, Automatic, Burst)
- AmmoType (Standard, AP, HE, HEAT, Smoke)
- TargetPosture (Standing, Crouching, Prone)
- TargetType (Infantry, LightVehicle, HeavyArmor, Aircraft, Structure)
- ObstacleType (Building, Vegetation, SoftCover, HardCover)

### ScriptableObjects (Data)
- WeaponMainConfig — master config referencing sub-configs
- AmmoConfig — per-ammo-type stats, penetration, hit chance by range
- AccuracyConfig — base hit chance, range dropoff, movement penalty, posture/target modifiers
- FeedbackConfig — hit/kill indicator settings

### Interfaces
- IWeapon — Shoot(), StopShoot(), CanShoot(), GetCurrentAmmo()
- IReloadable — Reload(), CanReload(), IsReloading

### Runtime (MonoBehaviours / Handlers)
- WeaponController — orchestrator, owns all handlers
- ShootingHandler — implements IWeapon
- ReloadHandler — implements IReloadable
- FeedbackHandler — hit/kill/damage feedback

### Utility
- HitCalculator — static, CalcHitChance(), EvalPenetration()

## Folder Structure
```
Assets/
  Scripts/
    Enums/        FireMode, AmmoType, TargetPosture, TargetType, ObstacleType
    Data/         WeaponMainConfig, AmmoConfig, AccuracyConfig, FeedbackConfig
    Interfaces/   IWeapon, IReloadable
    Handlers/     ShootingHandler, ReloadHandler, FeedbackHandler
    Core/         WeaponController, HitCalculator
    Example/      ExampleSetup
  Configs/        [ScriptableObject .asset files — created via menu]
```

## Code Style
- C# with Unity conventions
- Interfaces for contracts
- ScriptableObjects for weapon data/config
- Enums for fire modes, ammo types, etc.
- No magic numbers — all values in config
- User must approve architecture diagram before coding begins

## Git Rules
- Do NOT commit or push unless explicitly asked in that exact request
