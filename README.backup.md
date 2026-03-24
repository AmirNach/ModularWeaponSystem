# Modular Weapon System

A plug-and-play weapon system for Unity — built to be dropped into any project and just work.

## What It Does

Handles everything a weapon needs: firing, reloading, ammo switching, hit calculation, penetration, and feedback — all driven by ScriptableObjects so you configure weapons from the Inspector, not code.

## Architecture

```
WeaponController (orchestrator)
├── FiringHandler      → IWeapon       (fire modes, burst, ammo management)
├── ReloadHandler      → IReloadable   (magazine system, reload constraints)
├── FeedbackHandler    → events        (hit/kill/damage indicators)
└── HitCalculator      → static        (accuracy, range, posture, penetration)
```

**All weapon data lives in ScriptableObjects:**
- `WeaponConfig` — master config (fire mode, capacity, reload time)
- `AmmoConfig` — per-ammo stats (penetration, damage, hit chance by range)
- `AccuracyConfig` — hit chance modifiers (movement, posture, target type)
- `FeedbackConfig` — indicator settings (hit markers, kill confirms)

## Quick Start

1. Drop the `Scripts/` folder into your Unity project
2. Create configs: **Assets → Create → WeaponSystem → ...**
3. Add `WeaponController` to a GameObject, assign your config
4. Call `controller.Fire()`, `controller.Reload()`, done

## Features

- **Semi-Auto / Automatic / Burst** fire modes
- **Multiple ammo types** with hot-switching (AP, HE, HEAT, Smoke, Standard)
- **Range-based hit chance** with dropoff curves
- **Movement & posture penalties** on accuracy
- **Penetration system** (buildings, vegetation, soft/hard cover)
- **Hit/Kill feedback** indicators
- **Zero magic numbers** — everything in config

## Folder Structure

```
Assets/
  Scripts/
    Core/         WeaponController, HitCalculator
    Data/         WeaponConfig, AmmoConfig, AccuracyConfig, FeedbackConfig
    Enums/        FireMode, AmmoType, TargetPosture, TargetType, ObstacleType
    Handlers/     FiringHandler, ReloadHandler, FeedbackHandler
    Interfaces/   IWeapon, IReloadable
    Example/      Test scene + setup script
  Configs/        ScriptableObject assets (created via Unity menu)
```

## Design Principles

- **Modular** — standalone module, no external dependencies
- **DRY** — interfaces, base patterns, shared configs
- **Extensible** — add a weapon = create a ScriptableObject
- **Configurable** — all values in Inspector, zero hardcoded numbers

## Requirements

- Unity 2021.3+
- No third-party dependencies

## License

MIT
