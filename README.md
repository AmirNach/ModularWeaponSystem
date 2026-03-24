<div align="center">

# Modular Weapon System

### A plug-and-play weapon system for Unity — built to be dropped into any project and just work.

[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-black?logo=unity&logoColor=white)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![C#](https://img.shields.io/badge/C%23-100%25-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)

---

</div>

## Demo

<video src="https://github.com/AmirNach/ModularWeaponSystem/raw/main/Media/example.mp4" controls width="100%"></video>

> *Test scene — firing, reloading, ammo switching, and hit feedback. All driven by ScriptableObjects.*

---

## What It Does

Handles everything a weapon needs — **firing**, **reloading**, **ammo switching**, **hit calculation**, **penetration**, and **feedback** — all driven by ScriptableObjects so you configure weapons from the Inspector, not code.

---

## Architecture

```
WeaponController (orchestrator)
 ├── FiringHandler      -> IWeapon       (fire modes, burst, ammo management)
 ├── ReloadHandler      -> IReloadable   (magazine system, reload constraints)
 ├── FeedbackHandler    -> events        (hit/kill/damage indicators)
 └── HitCalculator      -> static        (accuracy, range, posture, penetration)
```

**All weapon data lives in ScriptableObjects:**

| Config | Purpose |
|--------|---------|
| `WeaponConfig` | Master config — fire mode, capacity, reload time |
| `AmmoConfig` | Per-ammo stats — penetration, damage, hit chance by range |
| `AccuracyConfig` | Hit chance modifiers — movement, posture, target type |
| `FeedbackConfig` | Indicator settings — hit markers, kill confirms |

---

## Features

| Feature | Details |
|---------|---------|
| **Fire Modes** | Semi-Auto / Automatic / Burst |
| **Ammo Types** | Standard, AP, HE, HEAT, Smoke — hot-switchable |
| **Hit Chance** | Range-based dropoff with configurable curves |
| **Accuracy Modifiers** | Movement speed & target posture penalties |
| **Penetration** | Buildings, vegetation, soft/hard cover |
| **Feedback System** | Hit markers, kill confirms, damage indicators |
| **Zero Magic Numbers** | Every value lives in config |

---

## Quick Start

```
1.  Drop the Scripts/ folder into your Unity project
2.  Create configs:  Assets -> Create -> WeaponSystem -> ...
3.  Add WeaponController to a GameObject, assign your config
4.  Call controller.Fire(), controller.Reload() — done
```

---

## Controls (Example Scene)

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Fire | `LMB` / `Space` | `RT` |
| Stop Fire | Release | Release |
| Reload | `R` | `X` |
| Switch Ammo | `1` - `5` | `D-Pad` |
| Simulate Hit | `H` | — |
| Simulate Kill | `K` | — |
| Simulate Damage | `D` | — |
| Hit Calc Test | `T` | — |

---

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

---

## Design Principles

| Principle | What It Means |
|-----------|---------------|
| **Modular** | Standalone module, no external dependencies |
| **DRY** | Interfaces, base patterns, shared configs |
| **Extensible** | Add a weapon = create a ScriptableObject |
| **Configurable** | All values in Inspector, zero hardcoded numbers |

---

## Requirements

- **Unity 2021.3+**
- **New Input System** package (for example scene)
- No third-party dependencies

---

<div align="center">

### License

MIT — use it however you want.

</div>
