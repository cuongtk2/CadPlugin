using System;
using System.Diagnostics;
using System.IO;

namespace HcPdfPrinter
{
    internal static class AppPaths
    {
        public const string PrinterName = "HcPdf Writer";
        public const string DriverName = "Microsoft Print To PDF";

        /// <summary>Đường dẫn exe đang chạy (net48 không có Environment.ProcessPath).</summary>
        public static readonly string ExePath = Process.GetCurrentProcess().MainModule!.FileName!;

        public static readonly string Root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "HcPdfPrinter");

        /// <summary>Thư mục spooler ghi file PDF ra (port của máy in trỏ vào đây).</summary>
        public static readonly string SpoolDir = Path.Combine(Root, "spool");

        /// <summary>Mỗi job sau khi bắt được sẽ được chuyển sang đây.</summary>
        public static readonly string QueueDir = Path.Combine(Root, "queue");

        /// <summary>Tên port = đường dẫn file; mỗi job in sẽ ghi đè file này.</summary>
        public static readonly string SpoolFile = Path.Combine(SpoolDir, "job.pdf");

        public static void EnsureDirs()
        {
            Directory.CreateDirectory(SpoolDir);
            Directory.CreateDirectory(QueueDir);
        }
    }
}