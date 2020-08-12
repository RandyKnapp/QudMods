using System.Globalization;
using HarmonyLib;
using XRL.UI;
using XRL.World.Parts;

namespace SuppressScavengingPopups
{
    [HarmonyPatch(typeof(Garbage))]
    [HarmonyPatch("AttemptRifle")]
    public class SuppressScavengingPopups
    {
        private const string SuppressPopupOption = "OptionSuppressScavengingPopup";

        private static bool PreviousSuppressionState;

        public static bool Prefix()
        {
            PreviousSuppressionState = Popup.bSurpressPopups;
            if (Options.GetOption(SuppressPopupOption) == "Yes")
            {
                Popup.bSurpressPopups = true;
            }
            return true;
        }

        public static void Postfix()
        {
            Popup.bSurpressPopups = PreviousSuppressionState;
        }
    }
}