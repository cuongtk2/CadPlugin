using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Printing;
using System.Threading;
using System.Threading.Tasks;

namespace HcPdfPrinter
{
    /// <summary>
    /// Theo dõi file job.pdf mà spooler ghi ra, chuyển sang queue\{guid}.pdf
    /// và báo ra ngoài (đường dẫn, tên tài liệu). Sự kiện được raise ở thread nền.
    /// </summary>
    internal sealed class SpoolWatcher : IDisposable
    {
        public event Action<string, string>? JobReceived;

        private readonly FileSystemWatcher _fsw;
        private readonly Timer _debounce;
        private readonly Timer _nameTimer;
        private int _busy;

        // theo dõi tên tài liệu từ hàng đợi in
        private PrintQueue? _queue;
        private int _lastJobId;
        private string? _lastJobName;

        public SpoolWatcher()
        {
            AppPaths.EnsureDirs();

            _debounce = new Timer(_ => Kick(), null, Timeout.Infinite, Timeout.Infinite);
            _nameTimer = new Timer(_ => PollJobName(), null, 0, 250);

            _fsw = new FileSystemWatcher(AppPaths.SpoolDir, Path.GetFileName(AppPaths.SpoolFile))
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            _fsw.Created += (_, _) => Schedule();
            _fsw.Changed += (_, _) => Schedule();
            _fsw.Renamed += (_, _) => Schedule();

            // xử lý file tồn đọng từ lần chạy trước
            Schedule();
        }

        private void Schedule() => _debounce.Change(50, Timeout.Infinite);

        private void Kick()
        {
            if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0) return;
            Task.Run(Drain);
        }

        private void Drain()
        {
            try
            {
                while (File.Exists(AppPaths.SpoolFile))
                {
                    string? dest = WaitAndMove();
                    if (dest == null) break;

                    if (!LooksLikePdf(dest))
                    {
                        TryDelete(dest);
                        continue;
                    }
                    JobReceived?.Invoke(dest, TakeJobName());
                }
            }
            catch
            {
                // bỏ qua, lần ghi sau sẽ kích hoạt lại
            }
            finally
            {
                Interlocked.Exchange(ref _busy, 0);
                if (File.Exists(AppPaths.SpoolFile)) Schedule();
            }
        }

        /// <summary>
        /// Đợi spooler ghi xong rồi chuyển file đi. File coi là xong khi phần cuối có %%EOF
        /// (hoặc kích thước đứng yên ~1 giây), và spooler đã nhả file (Move không bị lỗi).
        /// </summary>
        private static string? WaitAndMove()
        {
            string src = AppPaths.SpoolFile;
            string dest = Path.Combine(AppPaths.QueueDir,
                $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf");

            DateTime deadline = DateTime.UtcNow.AddMinutes(3);
            long prev = -1;
            int stable = 0;

            while (DateTime.UtcNow < deadline)
            {
                if (!File.Exists(src)) return null;

                long len;
                try { len = new FileInfo(src).Length; }
                catch (IOException) { len = 0; }

                if (len > 0 && len == prev) stable++;
                else { stable = 0; prev = len; }

                if (len > 0 && (HasEof(src) || stable >= 10))
                {
                    try
                    {
                        File.Move(src, dest);
                        return dest;
                    }
                    catch (IOException) { /* spooler còn giữ file, thử lại */ }
                    catch (UnauthorizedAccessException) { }
                }

                Thread.Sleep(100);
            }
            return null;
        }

        /// <summary>PDF ghi xong thì phần cuối file có %%EOF.</summary>
        private static bool HasEof(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
                if (fs.Length < 32) return false;

                int n = (int)Math.Min(1024, fs.Length);
                fs.Seek(-n, SeekOrigin.End);
                var buf = new byte[n];
                int read = fs.Read(buf, 0, n);
                return Encoding.ASCII.GetString(buf, 0, read).Contains("%%EOF");
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        private static bool LooksLikePdf(string path)
        {
            try
            {
                using var fs = File.OpenRead(path);
                var b = new byte[4];
                return fs.Read(b, 0, 4) == 4 && b[0] == '%' && b[1] == 'P' && b[2] == 'D' && b[3] == 'F';
            }
            catch { return false; }
        }

        private static void TryDelete(string path)
        {
            try { File.Delete(path); } catch { }
        }

        // ---- tên tài liệu từ hàng đợi in ----

        private void PollJobName()
        {
            try
            {
                _queue ??= new LocalPrintServer().GetPrintQueue(AppPaths.PrinterName);
                _queue.Refresh();
                foreach (PrintSystemJobInfo job in _queue.GetPrintJobInfoCollection())
                {
                    if (job.JobIdentifier > _lastJobId)
                    {
                        _lastJobId = job.JobIdentifier;
                        _lastJobName = job.Name;
                    }
                }
            }
            catch
            {
                _queue?.Dispose();
                _queue = null; // máy in chưa cài hoặc vừa bị xoá, thử lại ở lần poll sau
            }
        }

        private string TakeJobName()
        {
            string? raw = _lastJobName;
            _lastJobName = null;
            string name = raw == null ? "" : CleanTitle(raw);
            return name.Length > 0 ? name : $"Tài liệu {DateTime.Now:HH:mm:ss}";
        }

        /// <summary>
        /// Chỉ giữ tên bản vẽ: bỏ đường dẫn thư mục và đuôi file thường gặp.
        /// "D:\BV\Mat bang.dwg - Layout1" -> "Mat bang - Layout1"
        /// </summary>
        private static string CleanTitle(string raw)
        {
            string name = raw.Trim();

            int cut = name.LastIndexOfAny(new[] { '\\', '/' });
            if (cut >= 0) name = name.Substring(cut + 1);

            name = Regex.Replace(name, @"\.(dwg|dxf|dwt|docx?|xlsx?|pptx?|pdf|txt)(?=\s|$)", "",
                RegexOptions.IgnoreCase);

            return name.Trim();
        }

        public void Dispose()
        {
            _fsw.Dispose();
            _debounce.Dispose();
            _nameTimer.Dispose();
            _queue?.Dispose();
        }
    }
}
