using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace HcPdfPrinter
{
    internal sealed class TrayApp : ApplicationContext
    {
        private readonly NotifyIcon _icon;
        private readonly SpoolWatcher _watcher;
        private readonly Control _ui; // để marshal từ thread nền về UI thread
        private ViewerWindow? _viewer;

        public TrayApp()
        {
            _ui = new Control();
            _ = _ui.Handle;

            var menu = new ContextMenuStrip();
            menu.Items.Add("Cài máy in", null, (_, _) => RunSetupCommand("install"));
            menu.Items.Add("Gỡ máy in", null, (_, _) => RunSetupCommand("uninstall"));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Mở thư mục queue", null, (_, _) => Process.Start("explorer.exe", AppPaths.QueueDir));
            menu.Items.Add("Mở file log", null, (_, _) => OpenLog());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Thoát", null, (_, _) => ExitApp());

            _icon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = AppPaths.PrinterName,
                ContextMenuStrip = menu,
                Visible = true
            };

            CleanQueue(); // chưa có session nào, file còn sót trong queue là của lần chạy trước
            _watcher = new SpoolWatcher();
            _watcher.JobReceived += OnJobReceived;
            Log("Tray khởi động");
        }

        private void OnJobReceived(string pdfPath, string title)
        {
            Log($"Nhận job: {title} | {pdfPath}");
            _ui.BeginInvoke(new Action(async () =>
            {
                try
                {
                    if (_viewer == null)
                    {
                        Log("Tạo ViewerWindow");
                        _viewer = new ViewerWindow();
                        _viewer.Closed += (_, _) => _viewer = null; // đóng viewer = kết thúc session
                    }

                    _viewer.Show();
                    if (_viewer.WindowState == WindowState.Minimized) _viewer.WindowState = WindowState.Normal;
                    _viewer.Topmost = true;   // kéo lên trước rồi bỏ topmost
                    _viewer.Topmost = false;
                    _viewer.Activate();
                    Log("Đã Show viewer");

                    await _viewer.AddJobAsync(pdfPath, title);
                    Log("Đã thêm job vào viewer");
                }
                catch (Exception ex)
                {
                    Log("LỖI: " + ex);
                    _viewer = null;
                    System.Windows.Forms.MessageBox.Show(ex.ToString(), "HcPdfPrinter - Lỗi mở Viewer",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        private static void CleanQueue()
        {
            try
            {
                foreach (string f in Directory.GetFiles(AppPaths.QueueDir, "*.pdf"))
                {
                    try { File.Delete(f); } catch { }
                }
            }
            catch { }
        }

        private static string LogPath => Path.Combine(AppPaths.Root, "log.txt");

        private static void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(AppPaths.Root);
                File.AppendAllText(LogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}");
            }
            catch { }
        }

        private static void OpenLog()
        {
            if (File.Exists(LogPath))
                Process.Start(new ProcessStartInfo(LogPath) { UseShellExecute = true });
        }

        private static void RunSetupCommand(string cmd)
        {
            // Tiến trình con tự xin quyền Admin (xem Program.RunSetup).
            Process.Start(new ProcessStartInfo(AppPaths.ExePath, cmd) { UseShellExecute = true });
        }

        private void ExitApp()
        {
            _icon.Visible = false;
            _icon.Dispose();
            _watcher.Dispose();
            _viewer?.Close();
            _ui.Dispose();
            ExitThread();
        }
    }
}