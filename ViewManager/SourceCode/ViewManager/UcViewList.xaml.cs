using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace HCTool.Kientruc.ViewList
{
    public class ViewItem
    {
        public string Name { get; set; }
    }

    public partial class UcViewList : UserControl
    {
        public UcViewList()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AcApp.DocumentManager.DocumentActivated += OnDocumentActivated;
            LoadViews();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            AcApp.DocumentManager.DocumentActivated -= OnDocumentActivated;
        }

        // Chuyển bản vẽ -> nạp lại danh sách view
        private void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            LoadViews();
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            LoadViews();
        }

        private void LoadViews()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                dgViews.ItemsSource = null;
                return;
            }

            var list = new List<ViewItem>();
            using (var tr = doc.Database.TransactionManager.StartTransaction())
            {
                var vt = (ViewTable)tr.GetObject(doc.Database.ViewTableId, OpenMode.ForRead);
                foreach (ObjectId id in vt)
                {
                    var v = (ViewTableRecord)tr.GetObject(id, OpenMode.ForRead);
                    if (!v.IsPaperspaceView)
                        list.Add(new ViewItem { Name = v.Name });
                }
                tr.Commit();
            }
            dgViews.ItemsSource = list.OrderBy(x => x.Name).ToList();
        }

        // Click vào tên view -> màn hình nhảy đến view đó
        private void OnNameClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ViewItem item)
                GoToView(item.Name);
        }

        private void GoToView(string name)
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            if (doc == null || !doc.Database.TileMode) return;

            using (doc.LockDocument())
            using (var tr = doc.Database.TransactionManager.StartTransaction())
            {
                var vt = (ViewTable)tr.GetObject(doc.Database.ViewTableId, OpenMode.ForRead);
                if (!vt.Has(name))
                {
                    LoadViews(); // view đã bị xóa/đổi tên
                    return;
                }

                var v = (ViewTableRecord)tr.GetObject(vt[name], OpenMode.ForRead);
                doc.Editor.SetCurrentView(v);
                tr.Commit();
            }
        }

        // Pick 2 góc đối diện trên bản vẽ -> gán làm khung của view đã có
        private async void OnSetCorners(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement)?.DataContext as ViewItem;
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            if (item == null || doc == null) return;

            await AcApp.DocumentManager.ExecuteInCommandContextAsync(async _ =>
            {
                var ed = doc.Editor;
                if (!PickCorners(ed, $"\nView \"{item.Name}\" - chọn góc thứ nhất: ", out var p1, out var p2))
                    return;

                using (doc.LockDocument())
                using (var tr = doc.Database.TransactionManager.StartTransaction())
                {
                    var vt = (ViewTable)tr.GetObject(doc.Database.ViewTableId, OpenMode.ForRead);
                    if (!vt.Has(item.Name))
                    {
                        ed.WriteMessage("\nView không còn tồn tại.");
                        return;
                    }

                    var v = (ViewTableRecord)tr.GetObject(vt[item.Name], OpenMode.ForWrite);
                    if (!ToViewRect(p1, p2, Dcs2Wcs(v).Inverse(), out var center, out var w, out var h))
                    {
                        ed.WriteMessage("\n2 góc trùng nhau theo phương ngang hoặc dọc, bỏ qua.");
                        return;
                    }

                    v.CenterPoint = center;
                    v.Width = w;
                    v.Height = h;
                    tr.Commit();
                }
                ed.WriteMessage($"\nĐã cập nhật view \"{item.Name}\".");
            }, null);
        }

        // Thêm view mới: nhập tên, pick 2 góc. Hướng nhìn lấy theo màn hình hiện hành
        private async void OnAdd(object sender, RoutedEventArgs e)
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            await AcApp.DocumentManager.ExecuteInCommandContextAsync(async _ =>
            {
                var ed = doc.Editor;
                if (!doc.Database.TileMode)
                {
                    ed.WriteMessage("\nHãy chuyển về tab Model để thêm view.");
                    return;
                }

                var rn = ed.GetString(new PromptStringOptions("\nTên view mới: ") { AllowSpaces = true });
                if (rn.Status != PromptStatus.OK) return;
                string name = rn.StringResult.Trim();
                try { SymbolUtilityServices.ValidateSymbolName(name, false); }
                catch
                {
                    ed.WriteMessage("\nTên view không hợp lệ.");
                    return;
                }

                if (!PickCorners(ed, "\nChọn góc thứ nhất: ", out var p1, out var p2)) return;

                using (doc.LockDocument())
                using (var tr = doc.Database.TransactionManager.StartTransaction())
                using (var cur = ed.GetCurrentView())
                {
                    var vt = (ViewTable)tr.GetObject(doc.Database.ViewTableId, OpenMode.ForRead);
                    if (vt.Has(name))
                    {
                        ed.WriteMessage($"\nĐã có view \"{name}\".");
                        return;
                    }
                    if (!ToViewRect(p1, p2, Dcs2Wcs(cur).Inverse(), out var center, out var w, out var h))
                    {
                        ed.WriteMessage("\n2 góc trùng nhau theo phương ngang hoặc dọc, bỏ qua.");
                        return;
                    }

                    var v = new ViewTableRecord
                    {
                        Name = name,
                        ViewDirection = cur.ViewDirection,
                        Target = cur.Target,
                        ViewTwist = cur.ViewTwist,
                        CenterPoint = center,
                        Width = w,
                        Height = h
                    };
                    vt.UpgradeOpen();
                    vt.Add(v);
                    tr.AddNewlyCreatedDBObject(v, true);
                    tr.Commit();
                }
                ed.WriteMessage($"\nĐã thêm view \"{name}\".");
            }, null);

            LoadViews();
        }

        // Xóa view đang chọn trong grid
        private void OnDelete(object sender, RoutedEventArgs e)
        {
            var item = dgViews.SelectedItem as ViewItem;
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            if (item == null || doc == null) return;

            if (MessageBox.Show($"Xóa view \"{item.Name}\"?", "Xóa view",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            using (doc.LockDocument())
            using (var tr = doc.Database.TransactionManager.StartTransaction())
            {
                var vt = (ViewTable)tr.GetObject(doc.Database.ViewTableId, OpenMode.ForRead);
                if (vt.Has(item.Name))
                {
                    var v = (ViewTableRecord)tr.GetObject(vt[item.Name], OpenMode.ForWrite);
                    v.Erase();
                }
                tr.Commit();
            }
            LoadViews();
        }

        // Pick 2 góc, trả về điểm WCS
        private static bool PickCorners(Editor ed, string firstPrompt, out Point3d p1, out Point3d p2)
        {
            p1 = p2 = Point3d.Origin;

            var r1 = ed.GetPoint(firstPrompt);
            if (r1.Status != PromptStatus.OK) return false;

            var r2 = ed.GetCorner(new PromptCornerOptions("\nChọn góc đối diện: ", r1.Value));
            if (r2.Status != PromptStatus.OK) return false;

            var ucs2wcs = ed.CurrentUserCoordinateSystem;
            p1 = r1.Value.TransformBy(ucs2wcs);
            p2 = r2.Value.TransformBy(ucs2wcs);
            return true;
        }

        // 2 điểm WCS -> tâm/rộng/cao trong DCS của view
        private static bool ToViewRect(Point3d p1, Point3d p2, Matrix3d wcs2dcs,
            out Point2d center, out double w, out double h)
        {
            var d1 = p1.TransformBy(wcs2dcs);
            var d2 = p2.TransformBy(wcs2dcs);
            center = new Point2d((d1.X + d2.X) / 2, (d1.Y + d2.Y) / 2);
            w = Math.Abs(d1.X - d2.X);
            h = Math.Abs(d1.Y - d2.Y);
            return w > 1e-9 && h > 1e-9;
        }

        private static Matrix3d Dcs2Wcs(ViewTableRecord v)
        {
            return Matrix3d.Rotation(-v.ViewTwist, v.ViewDirection, v.Target)
                 * Matrix3d.Displacement(v.Target - Point3d.Origin)
                 * Matrix3d.PlaneToWorld(v.ViewDirection);
        }
    }
}