# THE PROMISED RUN — Completion Plan
> Based on GDD v3.0 vs current project state (April 29, 2026)

---

## PRIORITY TIERS

### 🔴 P0 — Gameplay Loop Broken Without These

**1. Player Movement Completion**
- [ ] Dash state (`DashState.cs`) — 3 units, I-frame 6f, 1.2s cooldown
- [ ] Coyote time — 8 frame grace period after leaving ground in `JumpState`
- [ ] Jump buffer — 6 frame input buffer in `InputReader`

**2. HUD (Player can't see game state)**
- [ ] OL Gauge UI — 4 visual states (gray/amber/red/flash), bind to `_chaosMeterVar`
- [ ] Health bar UI — bind to `_healthVar` ScriptableFloat
- [ ] Combo counter UI — flickers when popup interrupts combo

**3. Core Loop Closure**
- [ ] Death screen — "HERO #47 PERFORMANCE REVIEW" with System Blame messages
- [ ] Checkpoint system — auto every 45s, save position/HP/Relic
- [ ] Respawn flow — fade in at checkpoint, OL reset

**4. Level 1 — The Welcome Hall**
- [ ] Tilemap/floor layout with pits
- [ ] Exit trigger
- [ ] Grunt enemy placed and functional

---

### 🟡 P1 — Core Experience

**5. Popup Priority Queue**
- [ ] Context-aware popup selection (jumping → CoverLandingSpot, combat → FakeQuest)
- [ ] ≥3 distinct popup types with different SFX
- [ ] Popup adds +5 OL/sec while on screen

**6. Parry System**
- [ ] `ParryState.cs` — 5f window, 20f recovery on miss
- [ ] Counter attack: damage ×2.0, enemy stun 1.5s
- [ ] +30 OL on success

**7. Enemy Variants**
- [ ] Grunt — 30 HP, basic melee, patrol
- [ ] Shield Knight — 60 HP, blocks until Break Start combo

**8. Audio Foundation**
- [ ] Base BGM (lo-fi chiptune loop)
- [ ] 3 popup SFX (Windows XP error, Outlook notification, 8-bit fanfare cut)
- [ ] Overload audio (static burst + silence)
- [ ] Hit SFX (clink for light, thud for heavy)

**9. Level 2–3**
- [ ] Level 2: Armory of Echoes (combat focus, Grunt enemies)
- [ ] Level 3: Trap Garden (dash timing, traps)

---

### 🟢 P2 — Polish & Narrative

**10. Relic System**
- [ ] Pickup trigger + 0.3s pause
- [ ] Lore text display (handwritten font, 4s)
- [ ] Passive buff application

**11. Level 4 Secret Room**
- [ ] Trigger: exit locked until room visited
- [ ] Scroll text: Hero #01–#46 log
- [ ] Absolute silence, no OL decay

**12. Ending Sequence**
- [ ] White flash → black → footsteps → "HERO #47 WAS THE LAST."
- [ ] 12 seconds total, no music

**13. Remaining Enemies**
- [ ] Bomber, Mimic, System Echo

**14. Levels 5–7**

---

## IMPLEMENTATION ORDER

```
Week 1 (Days 1-3): P0 — Playable Loop
  Day 1: Dash + Coyote + Jump Buffer
  Day 2: HUD (OL Gauge + Health + Combo Counter)
  Day 3: Death Screen + Checkpoint + Level 1

Week 2 (Days 4-6): P1 — Core Experience  
  Day 4: Popup Priority Queue + 3 types + SFX
  Day 5: Parry + Grunt + Shield Knight
  Day 6: Audio + Level 2-3

Week 3 (Days 7-9): P2 — Polish
  Day 7: Relic System + Level 4 Secret Room
  Day 8: Ending Sequence + Level 5
  Day 9: Bug fixes + build test
```

---

## DEPENDENCY GRAPH

```
[Dash] ──────────────────────────────────────────────────────────┐
[Coyote + Jump Buffer] ──────────────────────────────────────────┤
                                                                  ▼
[OL Gauge UI] ──────────────────────────────────────────→ [Playtest Loop]
[Health UI] ─────────────────────────────────────────────────────┤
[Combo Counter UI] ──────────────────────────────────────────────┤
                                                                  │
[Death Screen] ──────────────────────────────────────────────────┤
[Checkpoint] ────────────────────────────────────────────────────┤
[Level 1] ───────────────────────────────────────────────────────┘
                                                                  │
                                                                  ▼
[Popup Priority Queue] ──────────────────────────────────→ [Core Loop]
[Parry] ─────────────────────────────────────────────────────────┤
[Grunt + Shield Knight] ─────────────────────────────────────────┤
[Audio] ─────────────────────────────────────────────────────────┘
                                                                  │
                                                                  ▼
[Relic] ─────────────────────────────────────────────────→ [Full Game]
[Secret Room] ───────────────────────────────────────────────────┤
[Ending] ────────────────────────────────────────────────────────┘
```

---

## WHAT'S ALREADY SOLID (Don't Touch)

- ✅ FSM (Locomotion/Jump/Land/Attack/Overload) — working
- ✅ Chaos/Overload system — working
- ✅ 3-hit combo with Hades-style buffer — working
- ✅ Enemy AI (Idle/Patrol/Chase/Attack) with NavMesh + RaycastPro — working
- ✅ SOLID architecture (PlayerProperties SO, IEnemyDetector, ScriptableVariables) — clean
- ✅ Juice (squash-stretch, screenshake, overload pulse) — working

---

## PLAYTEST GATE (After P0 Complete)

Find 2–3 people who've never seen the game. 30 min. Don't explain anything.

- [ ] Do they understand controls in first 30s?
- [ ] Do they try to use Overload? (gauge visual working?)
- [ ] When they die to popup: angry or laughing?
  - Angry = popup rate too high → reduce
  - Laughing = perfect → keep
- [ ] Do they notice System has personality?
- [ ] Can they reach exit of Level 1 without guidance?

**If mostly no → stop, cut features, fix fundamentals first.**
