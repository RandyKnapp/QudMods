using System;
using RedShadow.CommonDialogs;
using XRL.Core;

namespace Overlay.MapEditor
{
    public class MapEditorViewX : MapEditorView
    {
        public override void OnEnter()
        {
            base.OnEnter();
            var menu = DialogManager.createMenu();
            menu.addItem(null, "Test").setMenuItemClicked(new Action(this.OnTestClick));
            MenuBar.addMenu("Test", menu);
        }

        private void OnTestClick()
        {
            XRLCore.Log("Test");
        }
    }
}