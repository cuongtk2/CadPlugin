# CadToKlm Plugin - Hướng dẫn sử dụng / User Guide

[English](#english) | [Tiếng Việt](#tiếng-việt)

---

<a name="tiếng-việt"></a>
## 🇻🇳 Hướng dẫn sử dụng Plugin CadToKlm

Plugin **CadToKlm.dll** là một tiện ích mở rộng mạnh mẽ dành cho AutoCAD, hỗ trợ người dùng chuyển đổi dữ liệu bản vẽ CAD sang định dạng KML/KMZ (Google Earth) một cách nhanh chóng, chính xác và tiện lợi.

### 1. Yêu cầu hệ thống
* **Phần mềm:** AutoCAD 2017-2027 (các phiên bản hỗ trợ .NET API).
* **Môi trường:** .NET Framework tương ứng với phiên bản AutoCAD đang sử dụng.

### 2. Cài đặt và Nạp Plugin (Load Plugin)
Để sử dụng plugin trong AutoCAD, bạn thực hiện theo các bước sau:
1. Mở phần mềm AutoCAD.
2. Gõ lệnh **`NETLOAD`** trên dòng lệnh (Command Line) và nhấn `Enter`.
3. Duyệt đến thư mục chứa file **`CadToKlm.dll`**, chọn file và nhấn **Open**.

### 3. Danh sách lệnh (Commands)
Sau khi nạp thành công, plugin cung cấp các lệnh chính sau:

* **`CADTOKLM`**: Mở giao diện chính chức năng xuất dữ liệu CAD sang KML.
  * *Cách dùng:* Chọn vị trí tỉnh/ thành phố của bản đồ. Button Xuất Klm sẽ xuất hiện bảng lựa chọn tên file klm. Sau đó thực hiện Select các entities để xuất. Nhấn `Enter` để kết thúc.

---

<a name="english"></a>
## 🇬🇧 CadToKlm Plugin User Guide

The **CadToKlm.dll** plugin is a powerful extension for AutoCAD, designed to help users quickly, accurately, and conveniently export CAD drawing data to KML/KMZ format (Google Earth).

### 1. System Requirements
* **Software:** AutoCAD 2017-2027 (versions supporting .NET API).
* **Environment:** .NET Framework compatible with your AutoCAD version.

### 2. Installation & Loading
To use the plugin in AutoCAD, follow these steps:
1. Open AutoCAD.
2. Type **`NETLOAD`** in the Command Line and press `Enter`.
3. Browse to the folder containing **`CadToKlm.dll`**, select the file, and click **Open**.

### 3. Available Commands
Once loaded successfully, the plugin provides the following main command:

* **`CADTOKLM`**: Opens the main interface for exporting CAD data to KML.
  * *Usage:* Select the province/city location of the map. The Export KML button will prompt a dialog to choose the KML file name. Then, select the entities to export and press `Enter` to finish.
