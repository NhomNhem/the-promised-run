# Hướng dẫn tạo Shader Graph cho UI VFX

## QUAN TRỌNG - Unity 6.3+

Từ Unity 6.3, UI Toolkit hỗ trợ trực tiếp Shader Graph mà **KHÔNG CẦN RenderTexture**!

Xem docs: https://docs.unity3d.com/6000.3/Documentation/Manual/ui-systems/ui-shader-graph.html

---

## Tổng quan

Hướng dẫn này giúp bạn tạo 5 Shader Graph effects cho UI Toolkit:
- UIVFX_Pulse - Nhịp nhàng
- UIVFX_Glow - Phát sáng
- UIVFX_Shimmer - Quét ngang
- UIVFX_Ripple - Sóng tròn
- UIVFX_Feedback - Chớp màu

---

## Chuẩn bị

### Bước 1: Kiểm tra URP đã cài

1. **Window → Package Manager**
2. Search: **Universal RP**
3. Nếu chưa cài → Install

### Bước 2: Tạo UI Shader Graph (QUAN TRỌNG!)

1. **Project window** → Right-click
2. **Create → Shader Graph → URP → UI Shader Graph** (KHÔNG phải VFX hay Unlit!)
3. Save vào: `Assets/_Art/Shaders/UI/`

### Bước 3: Cấu hình Graph Inspector (bên phải)

```
Graph Settings:
├── Target: Universal Render Pipeline
└── (UI Shader Graph đã được cấu hình sẵn cho UI)
```

---

## Bước 4: Node bắt buộc

**UI Shader Graph yêu cầu sử dụng node:**

```
[Render Type Branch] - Required for UI!
```

Node này xử lý các render types:
- Solid
- SDF (Signed Distance Field)
- Bitmap

---

## Cách sử dụng Shader

### Cách 1: USS (Khuyến nghị)

```css
/* Trong .uss file */
.main-menu__logo-container {
    -unity-material: resource("Materials/UIVFX_Pulse.mat");
}
```

### Cách 2: C#

```csharp
// Trong C#
var material = new Material(Shader.Find("UI/VFX/UIVFX_Pulse"));
material.SetFloat("_Intensity", 0.5f);
element.style.unityMaterial = material;
```

### Cách 3: UI Builder

1. Inspector của element
2. Material dropdown
3. Chọn Material

---

## Chi tiết từng Shader

### 1. UIVFX_Pulse.shadergraph

**Mục đích:** Nhịp nhàng thay đổi brightness

**Nodes:**

```
[Time] → [Multiply] (_Speed) → [Sine] → [Multiply 0.1] → [Add 1] → [Multiply] → [Render Type Branch] → [Base Color]
                                                                              ↓
[Color (_Color)] → [Multiply] (_Intensity) ────────────────────────────────┘
```

**Cách tạo:**

1. Kéo **Time** node vào graph
2. Kéo **Multiply** → nối Time vào A, tạo property _Speed vào B
3. Kéo **Sine** → nối Multiply vào
4. Kéo **Multiply** → nối Sine vào A, tạo Float = 0.1 vào B
5. Kéo **Add** → nối Multiply vào A, tạo Float = 1 vào B
6. Kéo **Multiply** → nối Add vào A, _Intensity vào B
7. Kéo **Color** → chọn property _Color từ blackboard
8. Kéo **Multiply** → nối Color vào A, kết quả bước 6 vào B
9. Kéo **Render Type Branch** → nối Multiply vào Solid input
10. Nối Color output vào **Base Color** trong Fragment

---

### 2. UIVFX_Glow.shadergraph

**Mục đích:** Phát sáng mềm mại ở viền

**Nodes:**

```
[Position (Object)] → [Fresnel Effect] → [One Minus] → [Multiply] (_Intensity)
                                                                  ↓
                                              [Color (_Color)] → [Multiply] → [Emission]
                                                                              ↓
[Render Type Branch] ───────────────────────────────────────────────────────→ [Output]
```

---

### 3. UIVFX_Shimmer.shadergraph

**Mục đích:** Dải ánh sáng quét ngang qua element

**Nodes:**

```
[Time] → [Multiply] (_Speed) → [Fraction] → [Add UV]
                                           ↓
[UV] ──────────────────────────────────────→ [Gradient Noise]
                                              ↓
                                         [Step 0.5] → [Lerp (Transparent → _Color)]
                                              ↓
                                         [Multiply] (_Intensity) → [Render Type Branch]
```

---

### 4. UIVFX_Ripple.shadergraph

**Mục đích:** Sóng tròn lan tỏa từ tâm

**Nodes:**

```
[UV] - [Subtract (Center 0.5,0.5)] → [Length] → [Sine (Time×Speed - Length)]
                                              ↓
                                        [Multiply] (_Intensity) → [Add UV]
                                              ↓
                               [UV] ──────────────────────────────────→ [Sample Texture2D]
                                              ↓
                                         [Render Type Branch] → [Output]
```

---

### 5. UIVFX_Feedback.shadergraph

**Mục đích:** Chớp màu (flash) - dùng cho damage/heal feedback

**Nodes:**

```
[Base Color] → [Lerp] ← [Flash Color]
                   ↑
            [Flash Progress] ← Float 0-1
                   ↓
            [Multiply] (_Intensity) → [Alpha]
```

**Cách dùng trong code:**
```csharp
// Trigger flash
DOTween.To(() => 0f, x => material.SetFloat("_FlashProgress", x), 1f, 0.1f)
    .OnComplete(() => DOTween.To(() => 1f, x => material.SetFloat("_FlashProgress", x), 0f, 0.2f));
```

---

## Tạo Material từ Shader

1. Right-click shader → **Create → Material**
2. Đặt tên: `Mat_UIVFX_Pulse`
3. Kéo vào UI element hoặc dùng USS

---

## Video học Shader Graph

| Kênh | Video | Link |
|------|-------|------|
| Unity Official | UI Shader Graph Tutorial | youtube.com/watch?v=xAOBBW9hsjA |
| Brackeys | Shader Graph Tutorial | youtube.com/watch?v=GLu-I9XqVrM |

---

## Mẹo

1. **Preview**: Bấm **Space** trong graph để xem preview
2. **Render Type Branch**: Bắt buộc phải có trong UI Shader Graph!
3. **Hot Reload**: Save shader → Unity tự reload
4. **Material**: Tạo Material asset để sử dụng

---

## Kết quả

Sau khi tạo xong 5 shaders:

```
Assets/_Art/Shaders/UI/
├── UIVFX_Pulse.shadergraph
├── UIVFX_Glow.shadergraph  
├── UIVFX_Shimmer.shadergraph
├── UIVFX_Ripple.shadergraph
├── UIVFX_Feedback.shadergraph

Assets/
└── Materials/
    ├── Mat_UIVFX_Pulse.mat
    ├── Mat_UIVFX_Glow.mat
    └── ...
```

---

## Tài liệu tham khảo

- Unity Docs: https://docs.unity3d.com/6000.3/Documentation/Manual/ui-systems/ui-shader-graph.html
- Get Started: https://docs.unity3d.com/6000.3/Documentation/Manual/ui-systems/get-started-with-ui-shader-graph.html
