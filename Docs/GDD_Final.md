# THE PROMISED RUN — GDD Final v3.0
> PC Indie · Keyboard & Gamepad · Game Jam Sprint

---

## EXECUTIVE SUMMARY

**One sentence:** A platformer where a "helpful" system constantly gives late advice and ruins your playthrough — you must learn to stop trusting it and use it against itself.

**Core loop:** Player acts → System reacts (popup) → Chaos accumulates → Overload at 100% → System silenced 4–8s → Relief → System recovers angry → Repeat.

---

## PROGRESS SNAPSHOT (April 29, 2026)

### ✅ DONE
| System | Status | Notes |
|--------|--------|-------|
| Player Movement | 80% | Walk, jump, fall gravity, rotation |
| Combat (3-hit combo) | 85% | Hades-style buffer, step-forward, hitstop |
| FSM (Locomotion/Jump/Land/Attack/Overload) | 100% | All transitions working |
| Chaos/Overload System | 100% | Gauge, decay, trigger, cooldown |
| HelperSystem (popup spawner) | 70% | Timer-based, random messages, mute on overload |
| Enemy AI (Idle/Patrol/Chase/Attack/Dead) | 65% | NavMesh + RaycastPro detection |
| Juice (squash-stretch, screenshake, overload pulse) | 70% | Most effects wired |
| SOLID Refactor | 100% | PlayerProperties SO, IEnemyDetector, ScriptableVariables |

### ❌ MISSING (Critical for MVP)
| Feature | Priority | GDD Ref |
|---------|----------|---------|
| **Dash** (I-frame 6f, 1.2s cooldown) | 🔴 P0 | §3.1 |
| **Coyote time** (8 frames) | 🔴 P0 | §3.1 |
| **Jump buffer** (6 frames) | 🔴 P0 | §3.1 |
| **OL Gauge UI** (4 states: gray/amber/red/flash) | 🔴 P0 | §6.2 |
| **Health bar UI** | 🔴 P0 | §3.2 |
| **Combo counter UI** (flicker on popup interrupt) | 🔴 P0 | §4.2 |
| **Playable Level 1** (The Welcome Hall) | 🔴 P0 | §9.1 |
| **Death screen** (System Blame messages) | 🔴 P0 | §9.4 |
| **Checkpoint system** | 🔴 P0 | §9.3 |
| **Parry** (5f window, counter ×2.0) | 🟡 P1 | §3.2 |
| **Popup Priority Queue** (context-aware) | 🟡 P1 | §5.1 |
| **Popup types** (≥3 distinct with different SFX) | 🟡 P1 | §5.2 |
| **Enemy variants** (Grunt, Shield Knight) | 🟡 P1 | §7.1 |
| **Audio** (base BGM + 3 popup SFX + overload static) | 🟡 P1 | §13 |
| **Level 2–3** | 🟡 P1 | §9.1 |
| **Relic system** (pickup + lore text) | 🟢 P2 | §8 |
| **Level 4 Secret Room** | 🟢 P2 | §9.2 |
| **Ending sequence** (silence + text + fade) | 🟢 P2 | §12 |

---

## CORE MECHANICS SPEC

### Player Stats
| Stat | Value |
|------|-------|
| Walk speed | 5 u/s |
| Jump height | 4 units (variable hold) |
| Coyote time | 8 frames |
| Jump buffer | 6 frames |
| Dash distance | 3 units, I-frame 6f |
| Dash cooldown | 1.2s |

### Combat Frame Data
| Move | Startup | Active | Recovery |
|------|---------|--------|----------|
| Light | 4f | 3f | 12f |
| Heavy | 18f | 6f | 24f |
| Parry | — | 5f | 20f miss |

### Combo Chains
| Sequence | Name | Damage | Effect |
|----------|------|--------|--------|
| L→L→L | Triple Slash | 1.0/1.0/1.2 | Pushback |
| L→L→H | Finish Blow | 1.0/1.0/1.8 | Knockback + Stagger |
| H→L→L | Break Start | 1.5/0.8/0.8 | Breaks shield |
| H (hold 1s) | Overcharge | 2.5 AOE | Clears ALL popups |

### OL Gauge Sources
| Source | Amount |
|--------|--------|
| Hit enemy (Light) | +10 |
| Hit enemy (Heavy) | +15 |
| Popup on screen | +5/sec |
| Parry success | +30 |
| Die (System Blame) | +15 on respawn |
| Hit during combo | +25 |
| Idle >3s | -2%/sec |

### Overload States
| State | Range | Visual | Gameplay |
|-------|-------|--------|----------|
| Safe | 0–30% | Gray | Normal |
| Building | 30–70% | Amber flicker | Popup -10% |
| Critical | 70–99% | Red + vignette | Popup +20% |
| OVERLOAD | 100% | White flash | All popup gone, safe 4–8s |

---

## SYSTEM — Priority Queue Logic

```
if (jumping && velocity.y < -2)  → CoverLandingSpot (80%)
if (inCombat && enemy.attacking) → FakeQuest (60%) or FakeLevelUp (40%)
if (idle > 2s)                   → AFKWarning (100%)
if (hp < 30%)                    → FakeHealRecommend (70%) or MimicSpawn (30%)
if (justOverloaded)              → spawnRate ×1.5 for 10s
if (nearExit)                    → ExitBlockQuest
```

### Popup Types
| Type | SFX | Tactic |
|------|-----|--------|
| Late Warning | Windows XP error | Wrong advice, wrong time |
| Fake Quest | Outlook notification | Distraction mid-combat |
| Fake Level Up | 8-bit fanfare (cut) | Misdirection |
| AFK Timer | Low battery beep | Pressure |
| Low HP Fake Heal | Music box chime | False hope |
| System Recalibrating | Dial-up modem | System "panicking" |

---

## ENEMY TYPES

| Enemy | HP | Behavior | System Synergy |
|-------|-----|----------|----------------|
| Grunt | 30 | Patrol, aggro 5u | Warning popup AFTER hit |
| Shield Knight | 60 | Blocks, needs Break Start | Popup during Heavy charge |
| Bomber | 25 | Places bombs, evasive | UI hides bomb positions |
| Mimic | 50 | Fake relic, aggro when close | "Collect!" popup spam |
| System Echo | 80 | Invincible outside Overload | Requires Overload to damage |

---

## LEVEL OVERVIEW

| # | Name | Duration | Key Mechanic | Win Condition |
|---|------|----------|--------------|---------------|
| 1 | The Welcome Hall | 5min | Move + avoid pits | Reach exit |
| 2 | Armory of Echoes | 5min | Combat + Grunt | Reach exit |
| 3 | Trap Garden | 6min | Dash timing | Reach exit |
| 4 | Mirror Hall | 6min | Mimic ID + Secret Room | Secret Room + exit |
| 5 | Clocktower | 7min | Platform + combo | ≤3 deaths + exit |
| 6 | Relic Vault | 8min | System Echo boss | Boss + 2 Relic |
| 7 | System Core | 10min | 3-phase final boss | Touch The Architect |

---

## AUDIO DESIGN

### Music Layers
| Layer | When | Description |
|-------|------|-------------|
| Base | Always | Lo-fi chiptune, 90 BPM |
| Tension | >3 popup on screen | Percussion overlay |
| Overload | During Overload | Stereo glitch + bass pulse |
| Silence | Post-OL 0.5s | Total silence |

### Hit Feedback
| Event | Hitstop | Screenshake | Sound |
|-------|---------|-------------|-------|
| Light hit | 3f | None | Clink |
| Heavy hit | 6f | 0.1s | Thud |
| Parry | 8f | 0.2s | Sparkle |
| Overload | — | 0.4s | Static burst |

---

## RELIC TABLE

| Level | Relic | Buff | Lore |
|-------|-------|------|------|
| 1 | Rusted Armor Shard | -10% damage | "Anh hùng thứ 1 không hiểu vì sao mình chết." |
| 2 | Diary — Page 1 | +20% OL speed | "Ngày 1: Hệ thống rất hữu ích. Tôi tin nó hoàn toàn." |
| 3 | Diary — Page 8 | Coyote +8f | "Ngày 8: Hôm nay tôi bỏ qua lần đầu tiên. Và tôi sống." |
| 4 | Broken Coin | +10 OL after Overload | "Phần thưởng duy nhất đến trong sự hỗn loạn." |
| 5 | Crystal Eye | See through 1 popup | "Cô ấy mù mắt trái vì tin vào cảnh báo." |
| 6 | Red Binding Cord | Parry +8f | "Ngày 34: Đừng nghe nó." |

---

## MVP CHECKLIST (Must Ship)

- [ ] Player: dash, coyote time, jump buffer
- [ ] Combat: parry, 2–3 combo chains working
- [ ] Popup: ≥3 types with different SFX
- [ ] OL Gauge: visual with 4 states
- [ ] Overload: popup disappear, 4s window, cooldown
- [ ] Level 1–3 playable to completion
- [ ] Checkpoint system
- [ ] Death screen with System blame
- [ ] Ending: silence + text + fade
- [ ] Base BGM + 3 popup SFX + overload audio
