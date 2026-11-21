# HcPdfPrinter (HcPdf Writer)

Công cụ máy in ảo và trình xem/ghép trang PDF cho luồng in từ **AutoCAD** (hỗ trợ gom nhiều job in, chỉnh hướng/thứ tự/tên trang, tạo bookmark tự động và xuất ra file PDF hoàn chỉnh).

---

## Kiến trúc & Thành phần chính

- **Máy in ảo (`HcPdf Writer`)**: Dùng driver `Microsoft Print To PDF` và trỏ port tới file spool `job.pdf` trong thư mục `spool`.
- **Tray App (`TrayApp.cs`, `Program.cs`)**: Chạy nền ở khay hệ thống (chỉ cho phép 1 instance qua Mutex), theo dõi job in qua `SpoolWatcher`, mở `ViewerWindow` theo session và ghi log tại `log.txt`.
- **SpoolWatcher (`SpoolWatcher.cs`)**: Chờ spooler ghi xong file PDF (`%%EOF` hoặc kích thước ổn định), chuyển file sang thư mục `queue` và lấy tên tài liệu từ hàng đợi in.
- **ViewerWindow (`ViewerWindow.xaml`, `ViewerWindow.xaml.cs`)**: Xem trước trang (hỗ trợ fit chiều rộng, zoom, xoay 90° trái/phải, đổi thứ tự lên/xuống, xóa trang, sửa tiêu đề từng trang).
- **PdfExporter (`PdfExporter.cs`)**: Ghép các trang trong session thành một file PDF duy nhất, áp dụng góc xoay và tạo bookmark/outline theo tiêu đề từng trang.

---

## Đường dẫn làm việc mặc định

Dữ liệu ứng dụng nằm dưới thư mục `CommonApplicationData\HcPdfPrinter` (ví dụ `C:\ProgramData\HcPdfPrinter`):

- **Thư mục gốc (`Root`)**: `...\HcPdfPrinter` (chứa `log.txt`)
- **Thư mục spool (`SpoolDir`)**: `...\HcPdfPrinter\spool\job.pdf`
- **Thư mục queue (`QueueDir`)**: `...\HcPdfPrinter\queue` (lưu các file PDF tạm của từng job in)

---

## Cài đặt & Gỡ cài đặt máy in ảo

1. Khởi chạy ứng dụng `HcPdfPrinter` (ứng dụng sẽ nằm ở khay hệ thống - System Tray).
2. Chuột phải vào biểu tượng **HcPdf Writer** trên khay hệ thống và chọn **Cài máy in**.
3. Xác nhận **UAC (Run as Administrator)** nếu được hỏi[cite: 11]. Chương trình sẽ kiểm tra driver `Microsoft Print To PDF`, tạo port/máy in và cấp quyền `Modify` trên thư mục dữ liệu cho nhóm `Users`.

Để gỡ máy in: chuột phải vào icon trên khay hệ thống và chọn **Gỡ máy in**[cite: 6].

---

## Hướng dẫn in từ AutoCAD

1. Mở hộp thoại **Plot** trong AutoCAD và chọn máy in **`HcPdf Writer`**[cite: 13].
2. Chọn **Paper size** và **Drawing orientation** (**Landscape** hoặc **Portrait**) phù hợp với bản vẽ.
3. Nếu bản vẽ Landscape bị xuất đứng, kiểm tra lại **Printing preferences** của máy in **`HcPdf Writer`** trong Windows để đảm bảo hướng giấy và khổ giấy khớp với thiết lập in[cite: 13].
4. Bấm **OK / Plot** để gửi bản vẽ tới máy in ảo. `SpoolWatcher` sẽ tự động chuyển file sang `queue` và mở `ViewerWindow`[cite: 6, 7, 13].

---

## Thao tác trong Viewer & Lưu PDF

- **Thêm nhiều job vào cùng một session**: Khi `ViewerWindow` đang mở, mỗi lần in tiếp từ AutoCAD sẽ tự động nối thêm trang vào danh sách bên trái[cite: 5, 6]. Đóng `ViewerWindow` sẽ kết thúc session hiện tại[cite: 6].
- **Sửa tên / bookmark từng trang**: Click vào ô tiêu đề của từng trang ở danh sách bên trái để đổi tên (tên này sẽ dùng làm bookmark khi xuất PDF)[cite: 3, 4, 5].
- **Xoay trang**: Dùng **Xoay trái** (`270°`) hoặc **Xoay phải** (`90°`)[cite: 4, 5].
- **Sắp xếp / Xóa trang**: Dùng **Lên**, **Xuống**, hoặc **Xoá trang** (hoặc phím `Delete` khi chọn item trong danh sách)[cite: 4, 5].
- **Thu phóng**: Dùng **Vừa chiều rộng**, **100%**, **−**, **+**, hoặc giữ `Ctrl` + cuộn chuột[cite: 4, 5].
- **Lưu file PDF**: Bấm **Lưu PDF...**, chọn đường dẫn lưu và tên file[cite: 4, 5].

---

## Dọn dẹp file tạm & Khắc phục sự cố nhanh

- **Dọn file rác trong `queue`**: Nhả tài nguyên `PdfDocument` / ảnh khi đóng `ViewerWindow` và dọn các file `.pdf` trong `AppPaths.QueueDir` khi đóng `ViewerWindow`, khi khởi động `TrayApp`, hoặc trước `ExitThread()` khi thoát ứng dụng[cite: 5, 6, 7, 13].
- **Kiểm tra log**: Chuột phải vào icon khay hệ thống -> **Mở file log** hoặc **Mở thư mục queue** để kiểm tra chi tiết khi có sự cố[cite: 6, 13].
- **Lưu ý phiên bản `PDFsharp`**: Khai báo phiên bản ổn định rõ ràng trong `HcPdfPrinter.csproj` (tránh để wildcard không kiểm soát nếu gặp lỗi license/version)[cite: 2, 12].
