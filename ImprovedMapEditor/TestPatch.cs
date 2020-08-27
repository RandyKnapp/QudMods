using HarmonyLib;
using Qud.UI;
using QupKit;
using XRL.UI;

namespace ImprovedMapEditor
{
    [HarmonyPatch(typeof(ModToolkit))]
    [HarmonyPatch("OnActivate")]
    public class TestPatch
    {
        public static bool Prefix(ModToolkit __instance, QudMenuItem data)
        {
            if (data.command == "MapEditor")
            {
                UIManager.getWindow("MapEditorX").Show();
                __instance.Hide();
                return false;
            }

            return true;
        }
    }
}