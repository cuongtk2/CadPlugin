using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using HCTool.Kientruc.ViewList;
using System;

[assembly: CommandClass(typeof(ViewManager.CmdPalette))]

namespace ViewManager
{
    public class PaletteViewManager : PaletteSet
    {
        public PaletteViewManager()
            : base("ViewManager by Cường", "VIEWMANAGER", new Guid("{2AAD6CDB-AE49-4EDF-9A57-1BE9937A8354}"))  // tên, lệnh, GUID
        {
            Style = PaletteSetStyles.ShowAutoHideButton |
                    PaletteSetStyles.ShowCloseButton |
                    PaletteSetStyles.ShowPropertiesMenu;
            MinimumSize = new System.Drawing.Size(180, 300);

            AddVisual("VIEW LIST", new UcViewList());
        }
    }

    public class CmdPalette
    {
        public static PaletteViewManager paletteViewManager;

        [CommandMethod("VIEWMANAGER")]
        public static void PaletteViewCMD()
        {
            if (paletteViewManager == null)
                paletteViewManager = new PaletteViewManager { Visible = true };
            else
                paletteViewManager.Visible = !paletteViewManager.Visible;  // bật/tắt
        }
    }
}