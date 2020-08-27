using HarmonyLib;
using XRL.World;

namespace NoEatingSound
{
    [HarmonyPatch(typeof(IComponent<GameObject>))]
    [HarmonyPatch("PlayUISound")]
    public class IComponentGameObject_PlayUISound_Patch
    {
        public static bool Prefix(string clip)
        {
            return clip != "Human_Eating";
        }
    }
}