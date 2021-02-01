using HarmonyLib;
using XRL.World;

namespace ShorthandModNames
{
    [HarmonyPatch(typeof(IDisplayNameEvent), MethodType.Constructor)]
    public class IDisplayNameEvent_Patch
    {
        public static void Postfix(IDisplayNameEvent __instance)
        {
            __instance.DB = new ShorthandDescriptionBuilder();
        }
    }
}