using HarmonyLib;
using Qud.API;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace TheMachine
{
    [HarmonyPatch(typeof(OpeningStory))]
    [HarmonyPatch("FireEvent")]
    public static class OpeningStory_Patch
    {
        public static bool Prefix(OpeningStory __instance, Event E, ref bool __result)
        {
            string stringGameState = XRLCore.Core.Game.GetStringGameState("embark");
            if (stringGameState == "TheMachine")
            {
                if (E.ID == "BeforeTakeAction")
                {
                    if (__instance.bTriggered)
                    {
                        return true;
                    }
                    __instance.bTriggered = true;
                    string text = "&yOn the $day of $month, you awake, naked and confused, to the cold steel and smooth glass of {{G|The Machine}}." +
                        "\n\nAround you, amidst the rubble, appears to be the ruins of some science lab or military bunker." +
                        "\n\nYou sit up and {{G|The Machine's}} glowing, green eye pierces into your soul. \"ACTIVATE ASSET NUMBER {{r|ERROR ERROR ERROR}}\" its voice screetches from somewhere within it's plastic dome." +
                        "\n\n<Press space, then press F1 for help.";
                    const string displayName = "The Machine";
                    XRLCore.Core.Game.SetStringGameState("villageZeroName", displayName);
                    text = text.Replace("$day", Calendar.getDay());
                    text = text.Replace("$month", Calendar.getMonth());
                    text = text.Replace("$village", displayName);
                    Popup.Show(text);
                    JournalAPI.AddAccomplishment("On the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", you awoke in The Machine.", "On the auspicious " + Calendar.getDay() + " of " + Calendar.getMonth() + ", =name= awoke in The Machine and began " + IComponent<GameObject>.ThePlayer.GetPronounProvider().PossessiveAdjective + " prodigious odyssey through Qud.", "general", JournalAccomplishment.MuralCategory.IsBorn);
                    __instance.ParentObject.RemovePart(__instance);
                    __result = true;
                    return false;
                }
            }

            return true;
        }
    }
}