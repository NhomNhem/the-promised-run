# Strafe Left/Right Bug - Tasks

## Task List

- [x] 1. Write exploratory tests (run on UNFIXED code to confirm bug)
  - [x] 1.1 Create test file `Assets/Tests/PlayerControllerMovementTests.cs` with NUnit setup
  - [x] 1.2 Write test: `ApplyMovement_MoveLeft_SetsNegativeScaleOnRoot` — assert `transform.localScale.x == -1` after `MoveInput.x = -1` (confirms bug exists)
  - [x] 1.3 Write test: `ApplyMovement_MoveLeft_DoesNotRotateVisual` — assert `visual.localRotation == Quaternion.identity` after `MoveInput.x = -1` (confirms fix not yet applied)
  - [x] 1.4 Write test: `ApplyMovement_StopAfterLeft_RootScaleRemainsNegative` — assert scale stays `-1` when `MoveInput.x` returns to `0`
  - [x] 1.5 Run tests in Unity Test Runner and confirm they pass (proving bug is present on unfixed code)

- [x] 2. Apply the fix to `PlayerController.cs`
  - [x] 2.1 Remove `transform.localScale = new Vector3(Mathf.Sign(Input.MoveInput.x), 1, 1)` from `ApplyMovement()`
  - [x] 2.2 Add rotation logic: when `MoveInput.x > 0` set `visual.localRotation = Quaternion.identity`; when `MoveInput.x < 0` set `visual.localRotation = Quaternion.Euler(0, 180, 0)`
  - [x] 2.3 Wrap rotation logic in `if (visual != null)` null-safety guard

- [x] 3. Write fix-checking tests (Property 1 — verify bug is resolved)
  - [x] 3.1 Write test: `ApplyMovement_MoveLeft_RotatesVisualToFaceLeft` — assert `visual.localRotation == Quaternion.Euler(0, 180, 0)` after `MoveInput.x = -1`
  - [x] 3.2 Write test: `ApplyMovement_MoveRight_RotatesVisualToFaceRight` — assert `visual.localRotation == Quaternion.identity` after `MoveInput.x = 1`
  - [x] 3.3 Write test: `ApplyMovement_MoveLeft_RootScaleUnchanged` — assert `root.localScale == Vector3(1, 1, 1)` after `MoveInput.x = -1`
  - [x] 3.4 Write test: `ApplyMovement_MoveRight_RootScaleUnchanged` — assert `root.localScale == Vector3(1, 1, 1)` after `MoveInput.x = 1`
  - [x] 3.5 Write test: `ApplyMovement_DirectionSwitch_VisualRotationUpdatesImmediately` — move right then left, assert rotation updates on same frame
  - [x] 3.6 Run fix-checking tests and confirm all pass

- [x] 4. Write preservation-checking tests (Property 2 — verify no regressions)
  - [x] 4.1 Write test: `ApplyMovement_ZeroX_DoesNotChangeVisualRotation` — assert `visual.localRotation` unchanged when `MoveInput.x = 0`
  - [x] 4.2 Write test: `ApplyMovement_ZAxisMovement_AppliesCorrectVelocityZ` — assert `Rb.linearVelocity.z == MoveInput.y * moveSpeed` when `MoveInput.x = 0`
  - [x] 4.3 Write test: `ApplyMovement_Idle_VelocityXIsZero` — assert `Rb.linearVelocity.x == 0` when `MoveInput = Vector2.zero`
  - [x] 4.4 Write test: `ApplyMovement_NullVisual_DoesNotThrow` — assert no exception when `visual` is null and `MoveInput.x != 0`
  - [x] 4.5 Write property-based test: `ApplyMovement_AnyZeroXInput_RootScaleAlwaysOne` — generate random `MoveInput` with `x = 0`, assert `root.localScale == (1,1,1)` always
  - [x] 4.6 Run preservation tests and confirm all pass

- [x] 5. Verify in Unity Editor (manual smoke test)
  - [ ] 5.1 Enter Play mode and move player left — confirm visual faces left and strafe-left animation plays correctly
  - [ ] 5.2 Move player right — confirm visual faces right and strafe-right animation plays correctly
  - [ ] 5.3 Switch direction rapidly — confirm no visual glitch or scale artifact
  - [ ] 5.4 Confirm root Player object `localScale` stays `(1, 1, 1)` throughout all movement in Inspector
