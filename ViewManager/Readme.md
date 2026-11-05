# ViewManager - AutoCAD View & Palette Management Plugin

<details>
<summary><b>🌐 Language / Ngôn ngữ: Vietnamese | English (Click to expand / Bấm để mở)</b></summary>

---

### 🇻🇳 Tiếng Việt

**ViewManager** là module mã nguồn mở hỗ trợ quản lý Viewport và giao diện Palette cho AutoCAD. Dự án được cấu trúc để bạn có thể dễ dàng **copy mã nguồn trong thư mục `SourceCode`** và tích hợp thẳng vào giải pháp CAD của mình.

#### 💡 Tích Hợp Mã Nguồn (Integration)
* **Thư mục mã nguồn:** `ViewManager/SourceCode/ViewManager/`
* **Các thành phần chính có thể tái sử dụng:**
  * `PaletteViewManager.cs`: Class mẫu quản lý việc khởi tạo, hiển thị và neo Palette (`Dock`) trong AutoCAD.
  * `UcViewList.xaml` & `UcViewList.xaml.cs`: UserControl hiển thị danh sách dạng WPF.
  * `Themes/`: Bộ ResourceDictionary style giao diện tối (Dark Theme) dùng sẵn cho WPF.

#### 🛠️ Sử Dụng Nhanh (Dành cho Người dùng)
1. **Cài đặt:** Chạy file `ViewManager/Plugin/ViewManager_Setup.exe` hoặc dùng lệnh `NETLOAD` chọn `ViewManager.dll`.
2. **Lệnh CAD:** Gõ **`VIEWMGR`** để mở Palette, **`VIEWMGROFF`** để đóng Palette.

---

### 🇬🇧 English

**ViewManager** is an AutoCAD view and UI palette management module designed for easy integration. You can directly **copy the source code from the `SourceCode` directory** into your own AutoCAD C# project.

#### 💡 Source Code Integration
* **Source path:** `ViewManager/SourceCode/ViewManager/`
* **Reusable Components:**
  * `PaletteViewManager.cs`: Boilerplate code for creating, managing, and docking palettes in AutoCAD.
  * `UcViewList.xaml` & `UcViewList.xaml.cs`: Ready-to-use WPF UserControl list view.
  * `Themes/`: Reusable WPF Dark Theme ResourceDictionaries.

#### 🛠️ Quick Start (For Users)
1. **Installation:** Run `ViewManager/Plugin/ViewManager_Setup.exe` or use `NETLOAD` to load `ViewManager.dll`.
2. **Commands:** Type **`VIEWMGR`** to open the palette, or **`VIEWMGROFF`** to hide it.

</details>

---

## 📁 Repository Structure / Cấu Trúc Thư Mục

```text
ViewManager/
├── Plugin/
│   └── ViewManager_Setup.exe   # Installer package / Bộ cài đặt
└── SourceCode/
    └── ViewManager/            # 👈 Core C# source code & WPF components for integration