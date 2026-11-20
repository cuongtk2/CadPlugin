using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace HcPdfPrinter
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            string cmd = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "";
            bool quiet = Array.Exists(args, a => a.Equals("/quiet", StringComparison.OrdinalIgnoreCase));

            if (cmd == "install" || cmd == "uninstall")
                return RunSetup(cmd, quiet);

            using var mutex = new Mutex(true, @"Local\HcPdfPrinter.Tray", out bool isNew);
            if (!isNew) return 0;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayApp());
            return 0;
        }

        /// <param name="quiet">Chế độ không hộp thoại (dùng khi MSI gọi), lỗi ghi vào setup.log.</param>
        private static int RunSetup(string cmd, bool quiet)
        {
            // Cài/gỡ máy in cần quyền Admin: tự chạy lại với runas rồi chờ kết quả.
            if (!PrinterSetup.IsAdmin())
            {
                try
                {
                    var psi = new ProcessStartInfo(AppPaths.ExePath, quiet ? cmd + " /quiet" : cmd)
                    {
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    using var p = Process.Start(psi);
                    p?.WaitForExit();
                    return p?.ExitCode ?? 1;
                }
                catch (Win32Exception)
                {
                    return 1; // user bấm No ở UAC
                }
            }

            try
            {
                if (cmd == "install") PrinterSetup.Install();
                else PrinterSetup.Uninstall();

                if (!quiet)
                {
                    string done = cmd == "install" ? "Đã cài" : "Đã gỡ";
                    MessageBox.Show($"{done} máy in \"{AppPaths.PrinterName}\".", "HcPdfPrinter",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return 0;
            }
            catch (Exception ex)
            {
                if (quiet) WriteSetupLog(cmd, ex);
                else MessageBox.Show(ex.Message, "HcPdfPrinter - Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 2;
            }
        }

        private static void WriteSetupLog(string cmd, Exception ex)
        {
            try
            {
                Directory.CreateDirectory(AppPaths.Root);
                File.AppendAllText(Path.Combine(AppPaths.Root, "setup.log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {cmd} lỗi: {ex.Message}{Environment.NewLine}");
            }
            catch { }
        }
    }
}