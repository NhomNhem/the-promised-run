# Hướng dẫn Đặt tên Branch và Viết Commit Message

Tài liệu này quy định các tiêu chuẩn về Git workflow để đảm bảo tính nhất quán và dễ quản lý trong quá trình phát triển dự án.

---

## 1. Quy tắc đặt tên Branch

Cấu trúc: `<loại>/<module>/<mô-tả-ngắn-gọn-hoặc-ID>`

### Các loại branch (`<loại>`):
- **feat**: Khi thêm tính năng mới.
- **fix**: Khi sửa lỗi.
- **refactor**: Khi tái cấu trúc mã nguồn (không thay đổi tính năng).
- **docs**: Khi cập nhật tài liệu.
- **chore**: Các thay đổi nhỏ về build system, cấu hình, dependencies.

### Module (`<module>`):
- Tên folder hoặc thành phần chính mà bạn đang làm việc (ví dụ: `UI`, `Gameplay`, `Core`, `Sound`, `VFX`).

### ID/Mô tả (`<mô-tả-ngắn-gọn-hoặc-ID>`):
- Sử dụng ID của task (ví dụ: `0001`) hoặc mô tả ngắn bằng tiếng Anh/Tiếng Việt không dấu, cách nhau bởi dấu gạch ngang.

### Ví dụ:
- `feat/UI/0001` (Thêm UI mới cho task 0001)
- `fix/Gameplay/character-jump` (Sửa lỗi nhảy của nhân vật)
- `feat/Sound/background-music` (Thêm nhạc nền)

---

## 2. Quy tắc viết Commit Message

Chúng ta tuân theo chuẩn **Conventional Commits**.

Cấu trúc: `<loại>(<phạm vi>): <mô tả>`

### Các loại commit:
- **feat**: Một tính năng mới.
- **fix**: Sửa một lỗi.
- **docs**: Thay đổi về tài liệu.
- **style**: Thay đổi không ảnh hưởng đến logic code (formatting, missing semi-colons, etc).
- **refactor**: Thay đổi code nhưng không sửa lỗi cũng không thêm tính năng.
- **perf**: Thay đổi code để cải thiện hiệu suất.
- **test**: Thêm hoặc sửa các bài test.
- **chore**: Thay đổi bộ máy build hoặc công cụ hỗ trợ.

### Ví dụ:
- `feat(UI): thêm màn hình chính cho game`
- `fix(Gameplay): sửa lỗi nhân vật không rơi xuống khi hết máu`
- `docs(README): cập nhật hướng dẫn cài đặt`
- `refactor(Core): tối ưu hóa hệ thống quản lý tài nguyên`

---

## 3. Quy trình làm việc (Workflow)

1. **Cập nhật nhánh chính**: Trước khi tạo branch mới, hãy đảm bảo nhánh chính (`main` hoặc `develop`) đã được cập nhật bản mới nhất.
2. **Tạo branch**: Sử dụng quy tắc đặt tên ở trên.
3. **Commit thường xuyên**: Nên chia nhỏ các commit để dễ kiểm soát và rollback khi cần.
4. **Pull Request (PR)**: Sau khi hoàn thành task, đẩy branch lên và tạo PR để team review.
