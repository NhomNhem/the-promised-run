<div align="center">
  <img src="asset/icon.png" width="120" height="120" alt="Logo" />
  <h1>THE PROMISED RUN</h1>
  <p align="center">
    <strong>"The system always gives the right advice... just always too late."</strong>
  </p>

  <p align="center">
    <img src="https://img.shields.io/badge/Engine-Unity%202022.3-FFFFFF?style=for-the-badge&logo=unity" alt="Unity" />
    <img src="https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=c-sharp" alt="C#" />
    <img src="https://img.shields.io/badge/Rendering-URP-FFFFFF?style=for-the-badge&logo=unity" alt="URP" />
    <img src="https://img.shields.io/badge/Platform-PC-FF6B6B?style=for-the-badge" alt="Platform" />
    <img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="License" />
  </p>
</div>

---

## Giới thiệu

**The Promised Run** là một game platformer hài hước và hỗn loạn, nơi bạn chiến đấu chống lại chính hệ thống được cho là "hỗ trợ" bạn. Điều hướng qua các dungeon đầy chướng ngại, kẻ thù, và một trợ lý AI không mấy hữu ích — luôn đưa ra hướng dẫn đúng về mặt logic, nhưng sai hoàn toàn về thời điểm.

---

## Tính năng chính

- 🎮 **Gameplay hài hước** — Bị một hệ thống "hỗ trợ" đeo bám, luôn đúng nhưng trễ
- 💀 **Lời khuyên trễ** — "Jump!" khi đã rơi xuống hố, "Run!" khi đã trúng đòn
- 📋 **Nhiệm vụ vô nghĩa** — Yêu cầu đứng yên giữa nguy hiểm hoặc thu thập thứ không tồn tại
- 🔔 **Notification Spam** — Popup "LEVEL UP", "WARNING" xuất hiện đúng lúc thao tác
- 👁️ **Gây nhiễu tầm nhìn** — UI che mất các yếu tố quan trọng trong gameplay
- ⚡ **System Overload** — Kích hoạt overload để làm câm hệ thống trong vài giây

---

## Core Mechanic — System Overload

Trigger **System Overload** bằng cách thực hiện nhiều hành động cùng lúc (nhảy, trap, enemy...).

Khi hệ thống quá tải:

- Tất cả popup và notification bị tắt trong vài giây
- UI gây nhiễu dừng lại
- Bạn có cửa sổ an toàn để hành động không bị gián đoạn

---

## Progression Arc

Game dạy người chơi thành thạo qua ba giai đoạn:

| Giai đoạn | Mô tả |
|---|---|
| **Phase 1 — Tin hệ thống** | Tin theo lời khuyên → Thất bại |
| **Phase 2 — Nghi ngờ** | Bắt đầu bỏ qua → Bắt đầu học |
| **Phase 3 — Làm chủ** | Lợi dụng hệ thống → Chiến thắng |

---

## Levels

Mỗi level mang đến thử thách độc đáo xoay quanh:

- Né chướng ngại
- Tìm cách khai thác hệ thống
- Timing overload đúng thời điểm

**System Core** — Level cuối, người chơi bị kéo vào bên trong hệ thống. UI glitch liên tục, popup chồng chéo, hỗn loạn cực độ. Sử dụng tất cả kỹ năng đã học cho một overload cuối cùng để phá vỡ.

---

## The Ending

Trong cuộc đối đầu cuối cùng, thực thể đứng sau hệ thống liên tục spam hướng dẫn. Lần này, bạn tiếp cận và **đánh trực tiếp**.

Ngay lập tức — mọi âm thanh dừng lại, UI biến mất hoàn toàn. Không còn chỉ dẫn. Không còn spam. Không còn hệ thống.

Chỉ còn bạn.

---

## Tech Stack

- **Engine:** Unity 2022.3 LTS
- **Language:** C#
- **Rendering:** Universal Render Pipeline (URP)
- **Input:** New Input System
- **Platform:** PC (Windows/macOS/Linux)

---

## Cấu trúc dự án

```text
Assets/
├── Scenes/           # Level scenes
├── _Project/
│   ├── Scripts/      # Core gameplay systems
│   ├── _Art/         # Sprites, textures, shaders
│   └── Prefabs/      # Reusable game objects
├── InputSystem/      # Input actions configuration
├── Settings/         # URP pipeline settings
└── TutorialInfo/     # Unity onboarding content
```

---

## Hướng dẫn cài đặt

1. **Yêu cầu hệ thống:** Unity 2022.3 LTS hoặc mới hơn
2. **Clone project:**
   ```bash
   git clone https://github.com/NhomNhem/the-promised-run
   ```
3. **Mở dự án:** Dùng Unity Hub mở thư mục project
4. **Chạy game:** Mở scene `Assets/Scenes/` và nhấn Play

## Controls

| Action | Input |
|---|---|
| Move | WASD / Arrow Keys |
| Jump | Space |
| Overload | Multiple simultaneous inputs |
| Interact | E |

---

## Assets sử dụng

- **Characters:** [KayKit - Adventurers](https://kaylousberg.com/character/knight)
- **Dungeons:** LowPoly Fantasy Bundle
- **UI Effects:** Layer Lab GUI Pro-Fantasy RPG

---

<p align="center">
  Made with ❤️ | A chaotic platformer about fighting the system
</p>
