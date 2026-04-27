# DevLog - The Promised Run

---

## 2026-04-27

### State Machine Implementation (FSM)

**Core Framework:**
- Added `ThePromisedRun.Core.FSM` namespace with:
  - `IState` interface - base for all states
  - `ITransition` interface - transition between states
  - `IPredicate` interface - condition for transitions
  - `StateMachine` class - manages state transitions
  - `FuncPredicate` - predicate from lambda/function

**State Classes:**
- `BaseState` - abstract base with player/animator references
- `LocomotionState` - idle/walking, sets VelocityX/VelocityZ to animator
- `JumpState` - handles jump with air timer (MinAirTime = 0.15s), transitions to LandState
- `LandState` - landing animation with configurable duration
- `OverloadState` - "safety window" state when chaos meter full

**Transitions:**
```
Locomotion → Jump: IsJumpPressed && IsGrounded && !IsOverloaded
Jump → Land: CanLand (airTimer >= MinAirTime && IsGrounded && falling)
Land → Locomotion: IsLandingComplete
Any → Overload: ChaosMeter >= maxChaosThreshold && CooldownTimer <= 0
Overload → Locomotion: OverloadTimer <= 0 && IsGrounded
```

### Player Controller Updates

**Movement:**
- Added `fallGravityMultiplier` (2.5) for snappier jump arc
- Visual rotates to face movement direction
- Movement applies in world XZ based on input

**Chaos System:**
- `ChaosMeter` (0-100) accumulates from actions
- `AddChaos()` method with `ChaosSource` enum (Jump, Attack, Manual)
- `IsOverloaded` property triggers when ChaosMeter >= threshold
- Overload: 3s duration, 5s cooldown
- Chaos decays at 10 units/second when not overloaded

**Attack System (Fixed):**
- Combo system with 3-hit max
- ComboIndex animator parameter: starts at 1 (not 0!) for transition matching
- Cooldown: 0.15s, Combo window: 0.6s
- Triggers juice effects and adds chaos per hit

### Input System

- Updated to New Input System (`PlayerInputActions`)
- Added Attack input action
- `InputReader` now exposes:
  - `MoveInput` (Vector2)
  - `IsJumpPressed`
  - `IsAttackPressed`
  - `ConsumeJumpInput()`
  - `ConsumeAttackInput()`

### Bug Fixes Applied

1. **Syntax error in OnDisable()** - fixed orphaned code outside method
2. **Attack input not consumed** - added `ConsumeAttackInput()` call
3. **Unreachable combo continuation** - check input BEFORE timer decrement
4. **Missing CheckGround() method** - replaced with `UpdateGroundedState()`
5. **ComboIndex mismatch** - animator requires 1, code used 0

### New Files Added

- `Assets/_Project/_Scripts/Gameplay/ChaosSource.cs` - enum for chaos sources
- `Assets/_Project/_Scripts/Gameplay/States/LandState.cs` - landing state
- `Assets/_Project/_Scripts/Gameplay/Juice/` - juice effects system
- `Assets/_Project/_UI/` - UI components
- `Assets/_Project/_Animations/UpperBody_Mask.mask` - animation mask

---

## 2026-04-XX (Earlier)

- Initial project setup
- Basic UI system with HP display
- Play button functionality
- Core ScriptableVariable system

---

_Remember to update this file regularly!_