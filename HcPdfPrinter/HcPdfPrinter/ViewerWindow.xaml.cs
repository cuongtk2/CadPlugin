using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace HcPdfPrinter
{
    public partial class ViewerWindow : Window
    {
        private readonly ObservableCollection<PageItem> _pages = new();
        private readonly Dictionary<string, PdfDocument> _docs = new();
        private readonly DispatcherTimer _resizeTimer;

        private bool _fitWidth = true;
        private double _zoom = 1.0;   // hệ số zoom hiện tại (so với 96 dpi)
        private int _renderVersion;   // bỏ kết quả render cũ khi đã có yêu cầu mới

        private static double Clamp(double value, double min, double max)
            => value < min ? min : (value > max ? max : value);

        public ViewerWindow()
        {
            InitializeComponent();
            PageList.ItemsSource = _pages;

            // Window WPF chạy trong message loop WinForms: cần để nhận phím (gõ title, mũi tên...)
            ElementHost.EnableModelessKeyboardInterop(this);

            _resizeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _resizeTimer.Tick += (_, _) =>
            {
                _resizeTimer.Stop();
                if (_fitWidth) _ = ShowCurrentAsync();
            };
            Scroller.SizeChanged += (_, _) => { _resizeTimer.Stop(); _resizeTimer.Start(); };
        }

        /// <summary>Thêm các trang của một job in vào cuối session và chọn trang đầu của job đó.</summary>
        public async Task AddJobAsync(string pdfPath, string title)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(pdfPath);
            PdfDocument doc = await PdfDocument.LoadFromFileAsync(file);
            _docs[pdfPath] = doc;

            int start = _pages.Count;
            int count = (int)doc.PageCount;
            for (int i = 0; i < count; i++)
            {
                string t = count == 1 ? title : $"{title} - trang {i + 1}";
                _pages.Add(new PageItem(pdfPath, i, t));
            }

            PageList.SelectedIndex = start;
            PageList.ScrollIntoView(_pages[start]);
        }

        // ---- render ----

        private static async Task<BitmapImage> RenderAsync(PdfDocument doc, uint index, uint pixelWidth)
        {
            using PdfPage page = doc.GetPage(index);
            using var ms = new InMemoryRandomAccessStream();
            var opt = new PdfPageRenderOptions
            {
                DestinationWidth = pixelWidth, // chiều cao tự tính theo tỉ lệ
                BackgroundColor = new global::Windows.UI.Color { A = 255, R = 255, G = 255, B = 255 } // trắng
            };
            await page.RenderToStreamAsync(ms, opt);
            ms.Seek(0);

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = ms.AsStreamForRead();
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        private async Task ShowCurrentAsync()
        {
            int version = ++_renderVersion;
            try
            {
                if (PageList.SelectedItem is not PageItem item)
                {
                    PageImage.Source = null;
                    StatusText.Text = _pages.Count == 0 ? "Chưa có trang nào" : "";
                    return;
                }

                PdfDocument doc = _docs[item.SourcePdf];
                double wDip, hDip;
                using (PdfPage page = doc.GetPage((uint)item.PageIndex))
                {
                    wDip = page.Size.Width;   // đơn vị DIP (1/96 inch)
                    hDip = page.Size.Height;
                }

                bool sideways = item.Rotation % 180 != 0;
                double zoom = _zoom;
                if (_fitWidth)
                {
                    double avail = Scroller.ViewportWidth - 48;
                    double effW = sideways ? hDip : wDip; // chiều rộng thực khi đã xoay
                    zoom = avail > 0 ? Math.Max(0.05, avail / effW) : 1.0;
                    _zoom = zoom;
                }

                double dpiScale = VisualTreeHelper.GetDpi(this).DpiScaleX;
                uint px = (uint)Clamp(Math.Round(wDip * zoom * dpiScale), 16, 10000);

                BitmapImage bmp = await RenderAsync(doc, (uint)item.PageIndex, px);
                if (version != _renderVersion) return; // đã có yêu cầu render mới hơn

                PageImage.Source = bmp;
                PageImage.Width = wDip * zoom;
                PageImage.Height = hDip * zoom;
                PageImage.LayoutTransform = item.Rotation == 0 ? Transform.Identity : new RotateTransform(item.Rotation);

                int idx = PageList.SelectedIndex + 1;
                StatusText.Text =
                    $"Trang {idx}/{_pages.Count}   |   {wDip / 96 * 25.4:0} x {hDip / 96 * 25.4:0} mm   |   {zoom * 100:0}%";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Lỗi render: " + ex.Message;
            }
        }

        // ---- thao tác trang ----

        private void RotateLeft_Click(object sender, RoutedEventArgs e) => Rotate(270);
        private void RotateRight_Click(object sender, RoutedEventArgs e) => Rotate(90);

        private void Rotate(int delta)
        {
            if (PageList.SelectedItem is not PageItem item) return;
            item.Rotation = (item.Rotation + delta) % 360;
            _ = ShowCurrentAsync();
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e) => MoveSelected(-1);
        private void MoveDown_Click(object sender, RoutedEventArgs e) => MoveSelected(1);

        private void MoveSelected(int delta)
        {
            int i = PageList.SelectedIndex;
            int j = i + delta;
            if (i < 0 || j < 0 || j >= _pages.Count) return;
            _pages.Move(i, j);
            PageList.SelectedIndex = j;
            PageList.ScrollIntoView(_pages[j]);
        }

        private void Delete_Click(object sender, RoutedEventArgs e) => DeleteSelected();

        private void DeleteSelected()
        {
            int i = PageList.SelectedIndex;
            if (i < 0) return;
            _pages.RemoveAt(i);
            if (_pages.Count > 0)
                PageList.SelectedIndex = Math.Min(i, _pages.Count - 1);
        }

        private void PageList_KeyDown(object sender, KeyEventArgs e)
        {
            // Chỉ xoá trang khi focus đang ở item (gõ Delete trong ô title thì không tới đây).
            if (e.Key == Key.Delete && e.OriginalSource is ListBoxItem)
            {
                DeleteSelected();
                e.Handled = true;
            }
        }

        // ---- mở thêm file PDF có sẵn để nối vào session ----

        private async void OpenPdf_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                Multiselect = true
            };
            if (dlg.ShowDialog(this) != true) return;

            var errors = new List<string>();
            foreach (string path in dlg.FileNames)
            {
                try
                {
                    // Chép vào queue để file gốc của bạn không bị khoá, và bị xoá khi đóng viewer chỉ là bản chép.
                    Directory.CreateDirectory(AppPaths.QueueDir);
                    string copy = Path.Combine(AppPaths.QueueDir,
                        $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf");
                    File.Copy(path, copy);

                    try { await AddJobAsync(copy, Path.GetFileNameWithoutExtension(path)); }
                    catch { try { File.Delete(copy); } catch { } throw; }
                }
                catch (Exception ex)
                {
                    errors.Add($"{Path.GetFileName(path)}: {ex.Message}");
                }
            }

            if (errors.Count > 0)
                MessageBox.Show(this, string.Join(Environment.NewLine, errors), "Không mở được file",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // ---- lưu ----

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_pages.Count == 0) return;

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName = SafeFileName(_pages[0].Title)
            };
            if (dlg.ShowDialog(this) != true) return;

            try
            {
                PdfExporter.Export(_pages, dlg.FileName);
                StatusText.Text = $"Đã lưu: {dlg.FileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Lỗi lưu PDF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string SafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name.Trim();
        }

        // ---- đóng viewer = kết thúc session: xoá các file PDF của session khỏi queue ----

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            var files = new List<string>(_docs.Keys);
            _docs.Clear();
            _ = Task.Run(() => DeleteFiles(files));
        }

        private static void DeleteFiles(List<string> files)
        {
            // Windows.Data.Pdf có thể còn giữ handle tới file cho tới khi object được thu dọn,
            // nên GC rồi thử xoá lại vài lần.
            for (int attempt = 0; attempt < 5 && files.Count > 0; attempt++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                files.RemoveAll(f =>
                {
                    try { File.Delete(f); return true; }
                    catch (IOException) { return false; }
                    catch (UnauthorizedAccessException) { return false; }
                });
                if (files.Count > 0) Thread.Sleep(300);
            }
        }

        // ---- UI events ----

        private void PageList_SelectionChanged(object sender, SelectionChangedEventArgs e) => _ = ShowCurrentAsync();

        // Click/focus vào TextBox title trong item thì chọn luôn item đó.
        private void Item_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is ListBoxItem li) li.IsSelected = true;
        }

        private void FitWidth_Click(object sender, RoutedEventArgs e) { _fitWidth = true; _ = ShowCurrentAsync(); }
        private void Zoom100_Click(object sender, RoutedEventArgs e) { SetZoom(1.0); }
        private void ZoomIn_Click(object sender, RoutedEventArgs e) { SetZoom(_zoom * 1.25); }
        private void ZoomOut_Click(object sender, RoutedEventArgs e) { SetZoom(_zoom / 1.25); }

        private void SetZoom(double zoom)
        {
            _fitWidth = false;
            _zoom = Clamp(zoom, 0.05, 8.0);
            _ = ShowCurrentAsync();
        }

        private void Scroller_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            SetZoom(e.Delta > 0 ? _zoom * 1.15 : _zoom / 1.15);
            e.Handled = true;
        }
    }
}