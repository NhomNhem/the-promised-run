# Bugfix Requirements Document

## Introduction

Khi player di chuyển trái/phải trong LocomotionState, animation strafe left/right hiển thị sai chiều. Bug xảy ra do `ApplyMovement()` dùng `transform.localScale` trên root Player object để flip nhân vật thay vì rotate Visual child — điều này làm đảo ngược trục X của toàn bộ hierarchy, khiến BlendTree nhận `VelocityX` âm trong khi visual đã bị flip, dẫn đến animation strafe chạy ngược chiều so với hướng nhân vật đang nhìn.

## Bug Analysis

### Current Behavior (Defect)

1.1 WHEN player nhấn phím di chuyển sang trái (`MoveInput.x < 0`) THEN the system flip root Player object bằng `transform.localScale = new Vector3(-1, 1, 1)` và set `VelocityX = -1` cho BlendTree, khiến animation strafe-left chạy trên một visual đã bị mirror — kết quả trông như strafe-right

1.2 WHEN player nhấn phím di chuyển sang phải (`MoveInput.x > 0`) THEN the system giữ `transform.localScale.x = 1` và set `VelocityX = 1`, nhưng do logic flip không nhất quán với Visual child, animation strafe-right có thể hiển thị không đúng tùy trạng thái trước đó

1.3 WHEN `transform.localScale.x` của root Player bị set thành `-1` THEN the system đảo ngược trục X cho toàn bộ hierarchy bao gồm cả Rigidbody physics và Animator, gây ra sự không nhất quán giữa hướng di chuyển vật lý và hướng animation

### Expected Behavior (Correct)

2.1 WHEN player nhấn phím di chuyển sang trái (`MoveInput.x < 0`) THEN the system SHALL rotate Visual child object (chứa Animator + Knight model) để nhìn sang trái, trong khi root Player object giữ nguyên scale `(1, 1, 1)`, và BlendTree nhận `VelocityX = -1` tương ứng với strafe-left animation đúng chiều

2.2 WHEN player nhấn phím di chuyển sang phải (`MoveInput.x > 0`) THEN the system SHALL rotate Visual child object để nhìn sang phải, root Player object giữ nguyên scale `(1, 1, 1)`, và BlendTree nhận `VelocityX = 1` tương ứng với strafe-right animation đúng chiều

2.3 WHEN hướng nhân vật thay đổi THEN the system SHALL chỉ thay đổi rotation của Visual child (ví dụ: `visual.localRotation = Quaternion.Euler(0, 180, 0)` cho trái, `Quaternion.identity` cho phải) mà không ảnh hưởng đến scale của root Player object

### Unchanged Behavior (Regression Prevention)

3.1 WHEN player nhấn phím di chuyển lên/xuống (`MoveInput.y != 0`) THEN the system SHALL CONTINUE TO apply velocity trên trục Z (`Rb.linearVelocity.z`) và set `VelocityZ` cho BlendTree đúng như hiện tại

3.2 WHEN player đứng yên (`MoveInput == Vector2.zero`) THEN the system SHALL CONTINUE TO set `VelocityX = 0` và `VelocityZ = 0`, BlendTree blend về Idle animation

3.3 WHEN player nhảy (`IsJumpPressed && IsGrounded`) THEN the system SHALL CONTINUE TO transition sang JumpState và apply jump force đúng như hiện tại

3.4 WHEN player đang trong LocomotionState và di chuyển THEN the system SHALL CONTINUE TO apply `moveSpeed` lên cả trục X và Z của `Rb.linearVelocity`

3.5 WHEN `MoveInput.x` thay đổi dấu (đổi chiều trái/phải) THEN the system SHALL CONTINUE TO cập nhật hướng nhìn của nhân vật ngay lập tức (không có delay)

---

## Bug Condition (Pseudocode)

```pascal
FUNCTION isBugCondition(X)
  INPUT: X of type MoveInput (Vector2)
  OUTPUT: boolean
  
  // Bug xảy ra khi player di chuyển theo trục X (trái hoặc phải)
  RETURN X.x != 0
END FUNCTION
```

```pascal
// Property: Fix Checking - Strafe Direction Correctness
FOR ALL X WHERE isBugCondition(X) DO
  result ← ApplyMovement'(X)
  ASSERT root.localScale = Vector3(1, 1, 1)
  ASSERT visual.localRotation phản ánh đúng hướng của Sign(X.x)
  ASSERT animator.VelocityX = Clamp(X.x, -1, 1) nhất quán với visual facing direction
END FOR
```

```pascal
// Property: Preservation Checking
FOR ALL X WHERE NOT isBugCondition(X) DO
  // X.x == 0, chỉ di chuyển trục Z hoặc đứng yên
  ASSERT F(X) = F'(X)
  // Physics velocity, VelocityZ, Idle/forward animation không thay đổi
END FOR
```
