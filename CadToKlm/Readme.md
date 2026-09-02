# CadToKlm Plugin - Hướng dẫn sử dụng / User Guide

[English](#english) | [Tiếng Việt](#tiếng-việt)

---

<a name="tiếng-việt"></a>
## 🇻🇳 Hướng dẫn sử dụng Plugin CadToKlm

Plugin **CadToKlm.dll** là một tiện ích mở rộng mạnh mẽ dành cho AutoCAD, hỗ trợ người dùng chuyển đổi dữ liệu bản vẽ CAD sang định dạng KML/KMZ (Google Earth) một cách nhanh chóng, chính xác và tiện lợi.

### 1. Yêu cầu hệ thống
* **Phần mềm:** AutoCAD (các phiên bản hỗ trợ .NET API).
* **Môi trường:** .NET Framework tương ứng với phiên bản AutoCAD đang sử dụng.

### 2. Cài đặt và Nạp Plugin (Load Plugin)
Để sử dụng plugin trong AutoCAD, bạn thực hiện theo các bước sau:
1. Mở phần mềm AutoCAD.
2. Gõ lệnh **`NETLOAD`** trên dòng lệnh (Command Line) và nhấn `Enter`.
3. Duyệt đến thư mục chứa file **`CadToKlm.dll`**, chọn file và nhấn **Open**.

### 3. Danh sách lệnh (Commands)
Sau khi nạp thành công, plugin cung cấp các lệnh chính sau:

* **`CADTOKLM`** (hoặc lệnh tắt do plugin định nghĩa): Mở giao diện chính hoặc thực thi chức năng xuất dữ liệu CAD sang KML.
  * *Cách dùng:* Gõ tên lệnh trên dòng lệnh, chọn các đối tượng cần xuất (hoặc chọn toàn bộ bản vẽ tùy theo thiết kế của lệnh) và làm theo hướng dẫn trên màn hình để lưu file KML/KMZ kết quả.

### 4. Xử lý sự cố thường gặp
* **Lỗi không nhận lệnh:** Đảm bảo bạn đã dùng lệnh `NETLOAD` và chọn đúng file `.dll`. Kiểm tra xem file có bị chặn (Block) trong thuộc tính Windows Properties hay không (chọn file -> Properties -> bấm nút *Unblock* nếu có).
* **Sai hệ tọa độ:** Đảm bảo bản vẽ CAD của bạn đang sử dụng hệ tọa độ chuẩn (ví dụ: VN-2000, UTM, WGS 84) để Google Earth hiển thị đúng vị trí thực tế.

---

<a name="english"></a>
## 🇬🇧 CadToKlm Plugin User Guide

The **CadToKlm.dll** plugin is a powerful extension for AutoCAD, designed to help users quickly, accurately, and conveniently export CAD drawing data to KML/KMZ format (Google Earth).

### 1. System Requirements
* **Software:** AutoCAD (versions supporting .NET API).
* **Environment:** .NET Framework compatible with your AutoCAD version.

### 2. Installation & Loading
To use the plugin in AutoCAD, follow these steps:
1. Open AutoCAD.
2. Type **`NETLOAD`** in the Command Line and press `Enter`.
3. Browse to the folder containing **`CadToKlm.dll`**, select the file, and click **Open**.

### 3. Available Commands
Once loaded successfully, the plugin provides the following main command:

* **`CADTOKLM`**: Executes the export process from CAD entities to KML/KMZ.
  * *Usage:* Type the command in the command line, select the objects to export as prompted, and follow the on-screen instructions to save your KML/KMZ file.

### 4. Troubleshooting
* **Command not found:** Make sure you ran `NETLOAD` successfully and selected the correct `.dll` file. Check if the file is blocked by Windows (Right-click file -> Properties -> click *Unblock* if available).
* **Incorrect Coordinates:** Ensure your CAD drawing uses the correct coordinate system (e.g., VN-2000, UTM, WGS 84) so that Google Earth can display objects in their accurate real-world locations.
