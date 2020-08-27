using Overlay.MapEditor;
using XRL.UI;

namespace Qud.UI
{
    // Token: 0x02000EF8 RID: 3832
    [UIView("MapEditorX", true, false, "Menu", "MapEditor", false, 0, UICanvasHost = 1)]
    public class MapEditorWindowX : LegacyViewWindow<MapEditorViewX>
    {
        // Token: 0x06008510 RID: 34064 RVA: 0x00340F96 File Offset: 0x0033F196
        public override void Show()
        {
            base.Show();
            base.gameObject.SetActive(true);
        }

        // Token: 0x06008511 RID: 34065 RVA: 0x00340FAA File Offset: 0x0033F1AA
        public override void Hide()
        {
            base.Hide();
            base.gameObject.SetActive(false);
        }
    }
}