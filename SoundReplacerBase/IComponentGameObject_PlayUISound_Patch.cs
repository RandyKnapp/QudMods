using HarmonyLib;
using XRL.World;

namespace NoEatingSound
{
    public static class SoundOverrides
    {
        private static readonly string[][] Overrides =
        {
            new string[] {"Human_Eating", null},
            new string[] {"breakage", "Clink1"},
        };

        [HarmonyPatch(typeof(IComponent<GameObject>))]
        [HarmonyPatch("PlayUISound")]
        public class IComponentGameObject_PlayUISound_Patch
        {
            public static bool Prefix(ref string clip)
            {
                return CheckSound(ref clip);
            }
        }

        [HarmonyPatch(typeof(IComponent<GameObject>))]
        [HarmonyPatch("PlayWorldSound")]
        public class IComponentGameObject_PlayWorldSound_Patch
        {
            public static bool Prefix(ref string clip)
            {
                return CheckSound(ref clip);
            }
        }

        private static bool CheckSound(ref string clip)
        {
            foreach (string[] soundOverride in Overrides)
            {
                var sound = soundOverride[0];
                var replacement = soundOverride[1];
                if (clip == sound)
                {
                    if (string.IsNullOrEmpty(replacement))
                    {
                        return false;
                    }
                    else
                    {
                        clip = replacement;
                        return true;
                    }
                }
            }

            return true;
        }
    }
}