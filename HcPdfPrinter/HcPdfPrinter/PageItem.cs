using System.ComponentModel;

namespace HcPdfPrinter
{
    /// <summary>Một trang trong session (thuộc một file PDF của một job in).</summary>
    public sealed class PageItem : INotifyPropertyChanged
    {
        public string SourcePdf { get; }
        public int PageIndex { get; }

        private string _title;
        public string Title
        {
            get => _title;
            set { if (_title != value) { _title = value; Raise(nameof(Title)); } }
        }

        private int _rotation; // 0, 90, 180, 270
        public int Rotation
        {
            get => _rotation;
            set { if (_rotation != value) { _rotation = value; Raise(nameof(Rotation)); } }
        }

        public PageItem(string sourcePdf, int pageIndex, string title)
        {
            SourcePdf = sourcePdf;
            PageIndex = pageIndex;
            _title = title;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
