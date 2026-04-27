# Strafe Left/Right Bug - Bugfix Design

## Overview

Khi player di chuyển trái/phải trong `LocomotionState`, animation strafe left/right hiển thị sai chiều. Nguyên nhân là `ApplyMovement()` trong `PlayerController.cs` dùng `transform.localScale` trên root Player object để flip nhân vật. Điều này đảo ngược toàn bộ hierarchy (bao gồm cả Animator), khiến BlendTree nhận `VelocityX` âm trên một visual đã bị mirror — kết quả là animation strafe chạy ngược chiều so với hướng nhân vật đang nhìn.

Fix approach: Thay thế `transform.localScale` flip bằng `visual.localRotation` rotation chỉ trên Visual child object. Root Player object luôn giữ scale `(1, 1, 1)`.

## Glossary

- **Bug_Condition (C)**: Điều kiện kích hoạt bug — khi `MoveInput.x != 0` (player di chuyển theo trục X)
- **Property (P)**: Hành vi đúng khi bug condition xảy ra — Visual child rotate đúng chiều, root scale không đổi, BlendTree nhận VelocityX nhất quán với hướng visual
- **Preservation**: Các hành vi hiện tại không được thay đổi bởi fix — di chuyển trục Z, Idle, Jump, physics velocity
- **ApplyMovement()**: Method trong `PlayerController.cs` chịu trách nhiệm apply velocity và flip hướng nhân vật
- **visual**: `[SerializeField] private Transform visual` — Transform của Visual child object chứa Animator và Knight model
- **VelocityX / VelocityZ**: Animator parameters dùng trong BlendTree để chọn animation strafe/forward/idle
- **isBugCondition**: Hàm pseudocode xác định input nào kích hoạt bug

## Bug Details

### Bug Condition

Bug xảy ra khi player di chuyển theo trục X (`MoveInput.x != 0`). `ApplyMovement()` set `transform.localScale.x = Sign(MoveInput.x)` trên root Player object, đảo ngược trục X của toàn bộ hierarchy. Khi visual đã bị mirror nhưng `VelocityX` vẫn phản ánh hướng input thực, BlendTree chạy animation strafe ngược chiều so với hướng nhân vật đang nhìn.

**Formal Specification:**
```
FUNCTION isBugCondition(X)
  INPUT: X of type MoveInput (Vector2)
  OUTPUT: boolean

  RETURN X.x != 0
END FUNCTION
```

### Examples

- **Di chuyển trái** (`MoveInput.x = -1`): `localScale = (-1, 1, 1)` → visual bị mirror sang phải, nhưng `VelocityX = -1` → BlendTree chạy strafe-left trên visual đang nhìn phải → trông như strafe-right
- **Di chuyển phải** (`MoveInput.x = 1`): `localScale = (1, 1, 1)` → visual nhìn phải, `VelocityX = 1` → BlendTree chạy strafe-right đúng, nhưng nếu trước đó scale đã bị `-1` thì có thể không nhất quán
- **Đổi chiều nhanh** (trái → phải): Scale flip từ `(-1,1,1)` về `(1,1,1)` — physics và collider bị ảnh hưởng trong frame chuyển tiếp
- **Đứng yên** (`MoveInput.x = 0`): Scale không thay đổi, bug không xảy ra

## Expected Behavior

### Preservation Requirements

**Unchanged Behaviors:**
- Di chuyển trục Z (`MoveInput.y != 0`) phải tiếp tục apply velocity Z và set `VelocityZ` đúng như hiện tại
- Khi đứng yên (`MoveInput == Vector2.zero`), `VelocityX = 0` và `VelocityZ = 0`, BlendTree blend về Idle
- Jump transition (`IsJumpPressed && IsGrounded`) phải tiếp tục hoạt động đúng
- `moveSpeed` phải tiếp tục được apply lên cả trục X và Z của `Rb.linearVelocity`
- Hướng nhìn phải cập nhật ngay lập tức khi `MoveInput.x` đổi dấu (không có delay)

**Scope:**
Tất cả input không liên quan đến `MoveInput.x != 0` phải hoàn toàn không bị ảnh hưởng bởi fix này. Bao gồm:
- Di chuyển chỉ theo trục Z (`MoveInput.x == 0, MoveInput.y != 0`)
- Đứng yên (`MoveInput == Vector2.zero`)
- Jump input và JumpState transitions
- Physics velocity trên trục Y (gravity, jump arc)

## Hypothesized Root Cause

Dựa trên phân tích code, nguyên nhân gốc rễ là:

1. **Sai target của flip operation**: `ApplyMovement()` flip `transform.localScale` (root Player object) thay vì `visual.localRotation` (Visual child). Root object chứa Rigidbody, Collider, và toàn bộ hierarchy — flip scale ở đây đảo ngược trục X cho tất cả children.

2. **Scale flip làm đảo ngược Animator space**: Khi `localScale.x = -1`, Animator của Visual child hoạt động trong không gian bị mirror. BlendTree nhận `VelocityX = -1` (di chuyển trái) nhưng visual đang nhìn phải (do mirror), dẫn đến animation strafe chạy ngược chiều.

3. **`visual` field đã tồn tại nhưng không được dùng cho flip**: `PlayerController` đã có `[SerializeField] private Transform visual` được dùng để lấy Animator trong `Awake()`, nhưng `ApplyMovement()` không dùng nó để rotate.

4. **Không có fallback khi `MoveInput.x == 0`**: Code hiện tại chỉ set scale khi `MoveInput.x != 0`, nghĩa là scale giữ nguyên giá trị cuối cùng khi player dừng lại — có thể để lại scale `-1` khi idle.

## Correctness Properties

Property 1: Bug Condition - Strafe Direction Correctness

_For any_ input `X` where `isBugCondition(X)` is true (i.e., `X.x != 0`), the fixed `ApplyMovement()` SHALL:
- Giữ `root.localScale == Vector3(1, 1, 1)` (không thay đổi scale của root)
- Set `visual.localRotation = Quaternion.identity` khi `X.x > 0` (nhìn phải)
- Set `visual.localRotation = Quaternion.Euler(0, 180, 0)` khi `X.x < 0` (nhìn trái)
- Đảm bảo hướng visual nhất quán với dấu của `VelocityX` mà LocomotionState set cho BlendTree

**Validates: Requirements 2.1, 2.2, 2.3**

Property 2: Preservation - Non-X-Axis Input Behavior

_For any_ input `X` where `isBugCondition(X)` is false (i.e., `X.x == 0`), the fixed `ApplyMovement()` SHALL produce the same result as the original function, preserving velocity application trên trục Z, Idle animation behavior, và tất cả physics behavior không liên quan đến flip hướng.

**Validates: Requirements 3.1, 3.2, 3.4**

## Fix Implementation

### Changes Required

**File**: `Assets/_Project/_Scripts/Gameplay/PlayerController.cs`

**Method**: `ApplyMovement()`

**Specific Changes**:

1. **Xóa `transform.localScale` manipulation**: Bỏ dòng `transform.localScale = new Vector3(Mathf.Sign(Input.MoveInput.x), 1, 1)` hoàn toàn.

2. **Thêm rotation logic cho Visual child**:
   - Khi `MoveInput.x > 0`: `visual.localRotation = Quaternion.identity` (facing right — default orientation)
   - Khi `MoveInput.x < 0`: `visual.localRotation = Quaternion.Euler(0, 180, 0)` (facing left — rotated 180° trên trục Y)

3. **Đảm bảo null-safety cho `visual`**: Wrap rotation logic trong `if (visual != null)` để tránh NullReferenceException nếu field chưa được assign trong Inspector.

**Code sau khi fix:**
```csharp
public void ApplyMovement() {
    Rb.linearVelocity = new Vector3(Input.MoveInput.x * moveSpeed, Rb.linearVelocity.y, Input.MoveInput.y * moveSpeed);

    if (Input.MoveInput.x != 0 && visual != null) {
        visual.localRotation = Input.MoveInput.x > 0
            ? Quaternion.identity
            : Quaternion.Euler(0, 180, 0);
    }
}
```

**Không cần thay đổi**:
- `LocomotionState.cs` — `VelocityX` và `VelocityZ` đã được set đúng
- `visual` field declaration — đã tồn tại với `[SerializeField]`
- Bất kỳ file nào khác

## Testing Strategy

### Validation Approach

Testing theo hai phase: (1) Chạy exploratory tests trên code **chưa fix** để xác nhận root cause, (2) Sau khi fix, chạy fix-checking và preservation-checking tests để verify correctness.

### Exploratory Bug Condition Checking

**Goal**: Surface counterexamples chứng minh bug trên code chưa fix. Xác nhận hoặc bác bỏ root cause analysis.

**Test Plan**: Tạo `PlayerController` với mock components, gọi `ApplyMovement()` với `MoveInput.x != 0`, và assert rằng `transform.localScale` bị thay đổi (chứng minh bug) và `visual.localRotation` không thay đổi (chứng minh fix chưa được apply).

**Test Cases**:
1. **Scale Flip Left Test**: Gọi `ApplyMovement()` với `MoveInput.x = -1` → assert `transform.localScale.x == -1` (sẽ pass trên unfixed code, chứng minh bug tồn tại)
2. **Scale Flip Right Test**: Gọi `ApplyMovement()` với `MoveInput.x = 1` → assert `transform.localScale.x == 1` (baseline)
3. **Visual Rotation Not Changed Test**: Gọi `ApplyMovement()` với `MoveInput.x = -1` → assert `visual.localRotation == Quaternion.identity` (sẽ pass trên unfixed code — visual không được rotate)
4. **Root Scale After Stop Test**: Gọi `ApplyMovement()` với `MoveInput.x = -1` rồi `MoveInput.x = 0` → assert `transform.localScale.x == -1` (scale bị giữ nguyên khi dừng)

**Expected Counterexamples**:
- Root `localScale.x` bị set thành `-1` khi di chuyển trái — đây là bug
- `visual.localRotation` không thay đổi khi di chuyển — fix chưa được implement

### Fix Checking

**Goal**: Verify rằng với mọi input `X` thỏa `isBugCondition(X)`, fixed function cho kết quả đúng.

**Pseudocode:**
```
FOR ALL X WHERE isBugCondition(X) DO
  result := ApplyMovement_fixed(X)
  ASSERT root.localScale == Vector3(1, 1, 1)
  ASSERT visual.localRotation == expectedRotation(Sign(X.x))
END FOR
```

### Preservation Checking

**Goal**: Verify rằng với mọi input `X` thỏa `NOT isBugCondition(X)`, fixed function cho kết quả giống original.

**Pseudocode:**
```
FOR ALL X WHERE NOT isBugCondition(X) DO
  // X.x == 0
  ASSERT ApplyMovement_original(X).velocity == ApplyMovement_fixed(X).velocity
  ASSERT visual.localRotation không thay đổi
END FOR
```

**Testing Approach**: Property-based testing phù hợp cho preservation checking vì:
- Tự động generate nhiều test case với `MoveInput.x = 0` và `MoveInput.y` ngẫu nhiên
- Bắt edge case mà manual test có thể bỏ sót
- Đảm bảo velocity Z và Idle behavior không bị ảnh hưởng trên toàn bộ input domain

**Test Cases**:
1. **Z-Only Movement Preservation**: `MoveInput = (0, 1)` và `(0, -1)` → velocity Z đúng, visual rotation không thay đổi
2. **Idle Preservation**: `MoveInput = (0, 0)` → velocity `(0, y, 0)`, không có rotation change
3. **Physics Velocity Preservation**: Với bất kỳ `MoveInput.x = 0`, `Rb.linearVelocity.x == 0` và `Rb.linearVelocity.z == MoveInput.y * moveSpeed`

### Unit Tests

- Test `ApplyMovement()` với `MoveInput.x = 1` → `visual.localRotation == Quaternion.identity`, `root.localScale == (1,1,1)`
- Test `ApplyMovement()` với `MoveInput.x = -1` → `visual.localRotation == Quaternion.Euler(0,180,0)`, `root.localScale == (1,1,1)`
- Test `ApplyMovement()` với `MoveInput.x = 0` → `visual.localRotation` không thay đổi
- Test null-safety: `visual == null` không throw exception
- Test velocity application: `Rb.linearVelocity` đúng với mọi combination của `MoveInput`

### Property-Based Tests

- Generate random `MoveInput.x` values `!= 0` → verify `root.localScale` luôn là `(1,1,1)` sau fix
- Generate random `MoveInput` với `x = 0` → verify `Rb.linearVelocity` và `visual.localRotation` không thay đổi so với expected
- Generate random sequence of left/right inputs → verify visual rotation luôn nhất quán với input cuối cùng

### Integration Tests

- Test full LocomotionState flow: enter state → move left → verify `VelocityX = -1` và `visual` nhìn trái
- Test direction switch: move right → move left → verify visual rotation cập nhật ngay lập tức
- Test stop after move: move left → stop → verify `root.localScale` vẫn là `(1,1,1)` và visual giữ nguyên rotation cuối
