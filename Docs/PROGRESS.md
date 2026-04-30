# THE PROMISED RUN — Progress Documentation

> Cập nhật: Tháng 4, 2026  
> Dựa trên: `GDD_Final.md` + hiện trạng codebase

---

## ✅ ĐÃ HOÀN THÀNH (DONE)

### 1. Player Movement (80% → 90%)
| Tính năng | Trạng thái | Ghi chú |
|-----------|-------------|---------|
| Walk/Run | ✅ Done | 5 u/s, Input System mới |
| Jump | ✅ Done | 4 units, variable hold |
| Fall Gravity | ✅ Done | fallGravityMultiplier = 2.5 |
| Rotation | ✅ Done | Tự xoay theo hướng di chuyển |
| **Dash** | ✅ **Done** | `DashState.cs` đã có (3 units, I-frame 6f, 1.2s CD) |
| **Coyote Time** | ❌ Missing | 8 frames chưa implement |
| **Jump Buffer** | ❌ Missing | 6 frames chưa implement |

### 2. Combat System (85% → 90%)
| Tính năng | Trạng thái | Ghi chú |
|-----------|-------------|---------|
| 3-hit Combo | ✅ Done | Hades-style buffer, combo window 0.6s |
| Light Attack | ✅ Done | 4f startup, 3f active, 12f recovery |
| Heavy Attack | ✅ Done | 18f startup, 6f active, 24f recovery |
| Hitstop | ✅ Done | 3f light, 6f heavy |
| **Parry** | ✅ **Done** | `ParryState.cs` đã có (5f window, counter ×2.0) |
| Combo Chains | ⚠️ Partial | L→L→L, L→L→H hoạt động |

### 3. FSM (Finite State Machine) - 100% ✅
**States đã implement:**
- `LocomotionState` - Idle/Walking, set VelocityX/Z to animator
- `JumpState` - Jump với air timer (MinAirTime = 0.15s)
- `LandState` - Landing animation
- `AttackState` - Combo attack system
- `ParryState` - Parry với 5f window
- `DashState` - Dash với I-frames
- `OverloadState` - Safety window khi chaos meter đầy

**Transitions:**
```
Locomotion → Jump: IsJumpPressed && IsGrounded && !IsOverloaded
Jump → Land: CanLand (airTimer >= MinAirTime && IsGrounded && falling)
Land → Locomotion: IsLandingComplete
Any → Overload: ChaosMeter >= maxChaosThreshold && CooldownTimer <= 0
Overload → Locomotion: OverloadTimer <= 0 && IsGrounded
Any → Dash: IsDashPressed && !IsOverloaded
Any → Parry: IsParryPressed && canParry
```

**Core FSM Files:**
- `Core/FSM/StateMachine.cs` - Quản lý transitions
- `Core/FSM/IState.cs` - Interface cho states
- `Core/FSM/Transition.cs` - Transition logic
- `Core/FSM/FuncPredicate.cs` - Predicate từ lambda

### 4. Chaos/Overload System - 100% ✅
| Tính năng | Trạng thái | Ghi chú |
|-----------|-------------|---------|
| Chaos Meter | ✅ Done | 0-100, tích lũy từ actions |
| Chaos Sources | ✅ Done | Jump, Attack (Light +15, Heavy +10), Manual |
| Overload Trigger | ✅ Done | Tại 100%, 4-8s safety window |
| Overload Cooldown | ✅ Done | 5s cooldown sau overload |
| Chaos Decay | ✅ Done | 10 units/sec khi không overloaded |

**OL Gauge UI:** ❌ Missing (cần 4 states: gray/amber/red/flash)

### 5. Helper System (Popup Spawner) - 70%
| Tính năng | Trạng thái | Ghi chú |
|-----------|-------------|---------|
| Timer-based Spawn | ✅ Done | Spawn popup định kỳ |
| Random Messages | ✅ Done | Random system messages |
| Mute on Overload | ✅ Done | Popup biến mất khi overload |
| Priority Queue | ❌ Missing | Context-aware selection |
| Popup Types | ❌ Missing | Cần ≥3 loại (Late Warning, Fake Quest, etc.) |
| SFX per Type | ❌ Missing | Windows XP error, Outlook notification, etc. |

**Files:** `Gameplay/HelperSystem/HelperSystem.cs`, `HelperSystemConfig.cs`

### 6. Enemy AI - 65% → 80%
**States đã implement:**
- `EnemyIdleState` - Chờ và quét target
- `EnemyPatrolState` - Đi tuần tra
- `EnemyChaseState` - Đuổi theo target
- `EnemyAttackFSMState` - Tấn công khi trong tầm
- `EnemyDeadFSMState` - Chết và inactive
- `EnemyStunnedState` - Tạm thời bị disable
- `DisguisedState` - Cho Mimic (giả dạng relic)

**AI Components:**
- `EnemyAIControllerNavMesh` - Main AI controller
- `EnemyAINavMeshController` - NavMesh movement
- `EnemyBrain` - State machine cho enemy
- `EnemyAnimationEvents` - Animation event callbacks

**ScriptableObjects:**
- `EnemyAISettings` - Cấu hình AI (movement, detection, combat)
- `EnemyAIBehavior` - Behavior patterns và decision trees
- `AICondition` - Hệ thống điều kiện tái sử dụng

**Enemy Types:**
| Loại | Trạng thái | HP | Ghi chú |
|-------|-------------|-----|---------|
| Grunt | ✅ Done | 30 | Patrol, aggro 5u |
| Shield Knight | ❌ Missing | 60 | Cần Block, Break Start combo |
| Bomber | ❌ Missing | 25 | Đặt bom, evasive |
| Mimic | ⚠️ Partial | 50 | DisguisedState có, chưa hoàn thiện |
| System Echo | ❌ Missing | 80 | Invincible outside Overload |

### 7. Juice Effects - 70%
| Tính năng | Trạng thái | Ghi chú |
|-----------|-------------|---------|
| Squash-Stretch | ✅ Done | `SquashStretchJuice.cs` |
| Screenshake | ✅ Done | `PlayerJuice.cs` |
| Overload Pulse | ✅ Done | `OverloadJuice.cs` |
| Land Impact | ✅ Done | `LandImpactJuice.cs` |
| Attack Hitstop | ✅ Done | `AttackJuice.cs` |
| Hit Flash | ⚠️ Partial | Cần hoàn thiện |

### 8. SOLID Architecture - 100% ✅
**Core Interfaces:**
- `IState`, `ITransition`, `IPredicate` - FSM core
- `IEnemyDetector` - Detection interface
- `IPlayerController` - Player interface
- `IDamageable`, `IAttacker`, `IMovable` - Combat interfaces

**ScriptableObjects:**
- `PlayerProperties` - Player stats (movement, combat, health, audio)
- `EnemyProperties` - Enemy stats với Odin Inspector
- `ScriptableVariables` - Shared variables (Float, Int, Bool, Vector3)

**Dependency Injection:** Clean architecture với interfaces

### 9. UI System - 30%
| Tính năng | Trạng thái | Ghi chú |
|-----------|-------------|---------|
| Main Menu | ✅ Done | `MainMenuController.cs` |
| Settings Panel | ✅ Done | `SettingsPanel.cs`, `UIThemeSettings.cs` |
| **OL Gauge UI** | ❌ Missing | Cần 4 states visual |
| **Health Bar** | ❌ Missing | `EnemyHPBarUI.cs` có, health player chưa |
| **Combo Counter** | ❌ Missing | Cần flicker on popup interrupt |
| **Death Screen** | ⚠️ Partial | `DeathScreen.cs` có nhưng chưa hoàn thiện |
| Popup UI | ✅ Done | `PopupUI.cs` cho helper system |
| HUD Manager | ✅ Done | `GameHUDController.cs`, `HUDManager.cs` |

### 10. Other Systems
| Hệ thống | Trạng thái | Ghi chú |
|-----------|-------------|---------|
| Checkpoint System | ✅ **Done** | `CheckpointSystem.cs` đã implement |
| Level Exit Trigger | ✅ Done | `LevelExitTrigger.cs` |
| Death Pit Trigger | ✅ Done | `DeathPitTrigger.cs` |
| Damage System | ✅ Done | `DamageSystem.cs`, `DamageInfo.cs` |
| Audio Manager | ✅ Done | `AudioManager.cs` |
| Input System | ✅ Done | New Input System, `InputReader.cs` |
| Game Manager | ✅ Done | `GameManager.cs` |
| Game Bootstrapper | ✅ Done | `GameBootstrapper.cs` |

---

## ❌ ĐANG THIẾU (MISSING)

### 🔴 P0 - Critical for MVP (Gameplay Loop Broken Without These)

#### Player Mechanics
- [ ] **Coyote Time** (8 frames) - Thêm vào `JumpState.cs`
- [ ] **Jump Buffer** (6 frames) - Thêm vào `InputReader.cs`

#### HUD (Player can't see game state)
- [ ] **OL Gauge UI** - 4 visual states (gray/amber/red/flash), bind to `_chaosMeterVar`
- [ ] **Health Bar UI** - Bind to `_healthVar` ScriptableFloat
- [ ] **Combo Counter UI** - Flickers when popup interrupts combo

#### Core Loop Closure
- [ ] **Death Screen** - "HERO #47 PERFORMANCE REVIEW" với System Blame messages
- [ ] **Level 1 — The Welcome Hall** - Tilemap, pits, exit trigger, Grunt enemy

#### Audio Foundation
- [ ] **Base BGM** (lo-fi chiptune loop, 90 BPM)
- [ ] **Popup SFX** (≥3 types: Windows XP error, Outlook notification, 8-bit fanfare)
- [ ] **Hit SFX** (Clink light, Thud heavy, Sparkle parry)
- [ ] **Overload Audio** (Static burst + silence)

---

### 🟡 P1 - Core Experience

#### Popup System Enhancement
- [ ] **Popup Priority Queue** - Context-aware (jumping → CoverLandingSpot, combat → FakeQuest)
- [ ] **Popup Types** - ≥3 distinct với different SFX
- [ ] **Popup Adds Chaos** - +5 OL/sec while on screen

#### Enemy Variants
- [ ] **Shield Knight** - 60 HP, blocks until Break Start combo
- [ ] **Bomber** - 25 HP, places bombs, evasive
- [ ] **System Echo** - 80 HP, invincible outside Overload

#### Levels 2-3
- [ ] **Level 2: Armory of Echoes** - Combat focus, Grunt enemies
- [ ] **Level 3: Trap Garden** - Dash timing, traps (RockTrap.cs có sẵn)

---

### 🟢 P2 - Polish & Narrative

#### Relic System
- [ ] **Relic Pickup** - Trigger + 0.3s pause
- [ ] **Lore Text Display** - Handwritten font, 4s
- [ ] **Passive Buff** - Apply to player (Rusted Armor Shard, Diary Pages, etc.)

#### Level 4 Secret Room
- [ ] **Trigger** - Exit locked until room visited
- [ ] **Scroll Text** - Hero #01–#46 log
- [ ] **Absolute Silence** - No OL decay

#### Ending Sequence
- [ ] **White flash → black → footsteps → "HERO #47 WAS THE LAST."**
- [ ] **12 seconds total, no music**

#### Remaining Enemies
- [ ] **Bomber** - Place bombs, evasive movement
- [ ] **Mimic** - Fake relic, aggro when close, "Collect!" popup spam

#### Levels 5-7
- [ ] **Level 5: Clocktower** - Platform + combo, ≤3 deaths + exit
- [ ] **Level 6: Relic Vault** - System Echo boss, 2 Relics
- [ ] **Level 7: System Core** - 3-phase final boss, Touch The Architect

---

## 📊 TIẾN ĐỘ TỔNG THỂ

### Theo GDD_Final.md (April 29, 2026)
| Hệ thống | GDD Progress | Thực tế | Ghi chú |
|----------|--------------|---------|---------|
| Player Movement | 80% | **90%** | Dash đã xong (không như GDD note) |
| Combat (3-hit combo) | 85% | **90%** | Parry đã xong |
| FSM | 100% | **100%** | ✅ Complete |
| Chaos/Overload | 100% | **100%** | ✅ Complete, thiếu UI |
| Helper System | 70% | **70%** | Cần Priority Queue + Popup types |
| Enemy AI | 65% | **80%** | Mimic + DisguisedState có |
| Juice | 70% | **70%** | ✅ OK |
| SOLID Refactor | 100% | **100%** | ✅ Complete |
| **Checkpoint** | ❌ Missing | **✅ Done** | `CheckpointSystem.cs` có! |
| **Parry** | ❌ Missing | **✅ Done** | `ParryState.cs` có! |
| **Dash** | ❌ Missing | **✅ Done** | `DashState.cs` có! |

### MVP Checklist (Must Ship)
- [x] ~~Player: dash~~ ✅ **DONE** (DashState.cs)
- [ ] Player: coyote time, jump buffer
- [x] ~~Combat: parry~~ ✅ **DONE** (ParryState.cs)
- [ ] Combat: 2–3 combo chains working
- [ ] Popup: ≥3 types with different SFX
- [ ] OL Gauge: visual with 4 states
- [ ] Overload: popup disappear, 4s window, cooldown
- [ ] Level 1–3 playable to completion
- [x] ~~Checkpoint system~~ ✅ **DONE** (CheckpointSystem.cs)
- [ ] Death screen with System blame
- [ ] Ending: silence + text + fade
- [ ] Base BGM + 3 popup SFX + overload audio

**Progress: ~40% → 55%** (Phát hiện thêm Dash, Parry, Checkpoint đã xong!)

---

## 🎮 CÁCH CHƠI HIỆN TẠI

### Điều khiển (Input System mới)
- **WASD / Arrow Keys / Gamepad Left Stick** - Di chuyển
- **Space / Gamepad A** - Jump
- **Left Click / Gamepad X** - Attack (combo 3 hit)
- **Right Click / Gamepad Y** - Parry (5f window)
- **Shift / Gamepad B** - Dash (I-frame 6f, 1.2s CD)
- **Esc / Gamepad Start** - Pause/Settings

### Core Loop (Hiện tại)
1. Player di chuyển → System hiện popup (late advice)
2. Chaos tăng dần → Overload tại 100%
3. Overload: Tất cả popup biến mất, 4-8s an toàn
4. System hồi phục → Tiếp tục vòng lặp

---

## 📁 CẤU TRÚC FOLDER

```
Assets/
├── _Project/
│   ├── _Scripts/
│   │   ├── Core/
│   │   │   ├── FSM/              # State Machine (100%)
│   │   │   ├── Interfaces/       # IState, ITransition, etc.
│   │   │   └── Systems/          # DamageSystem
│   │   ├── Gameplay/
│   │   │   ├── Combat/           # PlayerCombat, AttackHitbox
│   │   │   ├── Enemy/            # Enemy, AI, ScriptableObjects
│   │   │   ├── Input/            # InputReader, PlayerInputActions
│   │   │   ├── Juice/            # SquashStretch, Screenshake
│   │   │   ├── Level/            # ExitTrigger, DeathPit
│   │   │   ├── Player/           # Properties, Animation
│   │   │   ├── States/           # Locomotion, Jump, Attack, etc.
│   │   │   └── HelperSystem/     # Popup spawner
│   │   └── UI/
│   │       ├── MainMenu           # MainMenuController
│   │       ├── HUD/              # GameHUD, ComboCounter
│   │       └── Popup/            # PopupUI
│   ├── Art/
│   ├── Audio/
│   └── Scenes/
```

---

## 🚀 KẾ HOẠCH TIẾP THEO (Implementation Order)

### Tuần 1 (P0 - Playable Loop)
1. **Coyote Time + Jump Buffer** (1 ngày)
   - Sửa `JumpState.cs` thêm coyote timer
   - Sửa `InputReader.cs` thêm jump buffer queue

2. **HUD Implementation** (1 ngày)
   - Tạo `OLGaugeUI.cs` với 4 states visual
   - Tạo `HealthBarUI.cs` bind `_healthVar`
   - Tạo `ComboCounterUI.cs` với flicker effect

3. **Level 1 + Death Screen** (1 ngày)
   - Tilemap cho "The Welcome Hall"
   - Exit trigger + Grunt enemy
   - Death screen với System Blame messages

### Tuần 2 (P1 - Core Experience)
4. **Popup Priority Queue + Audio** (2 ngày)
   - Context-aware popup selection
   - ≥3 popup types với SFX
   - Base BGM + hit SFX

5. **Enemy Variants + Levels 2-3** (2 ngày)
   - Shield Knight (block mechanics)
   - Level 2: Armory of Echoes
   - Level 3: Trap Garden

### Tuần 3 (P2 - Polish)
6. **Relic System + Secret Room** (2 ngày)
7. **Ending Sequence + Bug Fixes** (1 ngày)
8. **Build Test + Polish** (1 ngày)

---

## 📝 GHI CHÚ QUAN TRỌNG

### Đã phát hiện (không ghi trong GDD)
1. ✅ **DashState.cs** đã implement đầy đủ
2. ✅ **ParryState.cs** đã implement đầy đủ
3. ✅ **CheckpointSystem.cs** đã implement đầy đủ
4. ✅ **DisguisedState.cs** cho Mimic enemy
5. ✅ **RockTrap.cs** cho Trap Garden
6. ⚠️ **EnemyHPBarUI.cs** có nhưng health bar player chưa có

### Cần ưu tiên
1. **Coyote Time + Jump Buffer** - Critical cho platforming feel
2. **OL Gauge UI** - Player cần thấy chaos meter
3. **Health Bar UI** - Player cần thấy HP
4. **Popup Priority Queue** - Core experience của game
5. **Audio Foundation** - Game feels empty without sound

---

*Tài liệu này được tự động tạo từ `GDD_Final.md` và phân tích codebase thực tế.*
