using System.Text;
using HarmonyLib;
using Qud.UI;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace BetterTime
{
    [HarmonyPatch(typeof(Calendar))]
    [HarmonyPatch("getTime", new [] { typeof(int) })]
    public static class Calendar_getTime_Patch
    {
        private const string SIMPLEMODE_OPTION_NAME = "OptionBetterTimeSimple";
        private const string CLOCK24HOUR_OPTION_NAME = "OptionBetterTime24Hour";
        private const int MINUTES_IN_DAY = 1200;
        private const int MINUTES_PER_HOUR = 50;
        private const int START_DAY = 325;
        private const int START_NIGHT = 1000;
        private const int DAY_LENGTH = START_NIGHT - START_DAY;
        private const int NIGHT_LENGTH = MINUTES_IN_DAY - DAY_LENGTH;
        private const int NOTCHES = 18;
        private const int SEGMENTS_PER_NOTCH = 2;

        public static bool OverrideSimple;

        public static void Postfix(ref string __result, int minute)
        {
            bool UseSimplifiedView = Options.GetOption(SIMPLEMODE_OPTION_NAME) == "Yes";
            bool Use24HourClock = Options.GetOption(CLOCK24HOUR_OPTION_NAME) == "Yes";

            bool day = minute >= START_DAY && minute < START_NIGHT;
            float percentOfDay = GetPercentOfDay(minute, day);

            int hoursValue = minute / MINUTES_PER_HOUR;
            int minutesValue = minute % MINUTES_PER_HOUR;
            string extraDisplay = string.Empty;
            if (!Use24HourClock)
            {
                extraDisplay = (hoursValue >= 12) ? "pm" : "am";
                hoursValue %= 12;
                hoursValue = hoursValue == 0 ? 12 : hoursValue;
            }

            string todDisplay = $"{hoursValue}:{minutesValue:00}{extraDisplay}";

            if (UseSimplifiedView || OverrideSimple)
            {
                __result += (__result.Length < 18 ? " " : "") + "&c" + todDisplay;
                return;
            }

            const string BASE_STRING = "\u00B0 \u00B0\u00B0 \u00B0\u00B0 \u00B0\u00B0 \u00B0\u00B0 \u00B0\u00B0 \u00B0";
            string bgColors = day ? "c  C  W  Y  O  m  " : "m  K  k     K  M  ";
            string fgColors = day ? "KKCccWCCYWWOYYmOOK" : "MMKmmkkkkkkKkkMkkc";
            string notchColor = day ? "k" : "Y";

            int notchPosition = (int)(NOTCHES * SEGMENTS_PER_NOTCH * percentOfDay);
            var charNotchPosition = notchPosition / SEGMENTS_PER_NOTCH;
            const string NOTCH_CHARS = "\u00DD\u00DE";
            var notchChar = NOTCH_CHARS[notchPosition % SEGMENTS_PER_NOTCH];

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < NOTCHES; index++)
            {
                char character = BASE_STRING[index];
                string bgColor = ("" + bgColors[index]).Trim();
                string fgColor = ("" + fgColors[index]).Trim();
                if (index == charNotchPosition)
                {
                    fgColor = notchColor;
                    character = notchChar;
                }

                if (bgColor.Length > 0)
                {
                    builder.Append("^" + bgColor);
                }
                if (fgColor.Length > 0)
                {
                    builder.Append("&" + fgColor);
                }

                builder.Append(character);
            }

            __result = builder + "^k&y" + (hoursValue >= 10 ? "" : " ") + todDisplay;
        }

        private static float GetPercentOfDay(int minute, bool day)
        {
            if (day)
            {
                return (minute - START_DAY) / (float)DAY_LENGTH;
            }
            else
            {
                var minutesIntoNight = 0;
                if (minute >= START_NIGHT)
                {
                    minutesIntoNight = minute - START_NIGHT;
                }
                else if (minute < START_DAY)
                {
                    minutesIntoNight = (MINUTES_IN_DAY - START_NIGHT) + minute;
                }

                return (minutesIntoNight) / (float)NIGHT_LENGTH;
            }
        }
    }

    //Bed.AttemptSleep(GameObject who, out bool SleepSuccessful, out bool MoveFailed)
    [HarmonyPatch(typeof(Bed))]
    [HarmonyPatch("AttemptSleep", new [] { typeof(GameObject) })]
    public static class Bed_AttemptSleep_Patch
    {
        public static bool Prefix()
        {
            Calendar_getTime_Patch.OverrideSimple = true;
            return true;
        }

        public static void Postfix()
        {
            Calendar_getTime_Patch.OverrideSimple = false;
        }
    }

    //PlayerStatusBar.BeginEndTurn(XRLCore)
    [HarmonyPatch(typeof(PlayerStatusBar))]
    [HarmonyPatch("BeginEndTurn", new [] { typeof(XRLCore) })]
    public static class PlayerStatusBar_BeginEndTurn_Patch
    {
        public static bool Prefix()
        {
            Calendar_getTime_Patch.OverrideSimple = true;
            return true;
        }

        public static void Postfix()
        {
            Calendar_getTime_Patch.OverrideSimple = false;
        }
    }

    //ZoneManager.SetActiveZone(Zone Z)
    [HarmonyPatch(typeof(ZoneManager))]
    [HarmonyPatch("SetActiveZone", new [] { typeof(Zone) })]
    public static class ZoneManager_SetActiveZone_Patch
    {
        public static bool Prefix()
        {
            Calendar_getTime_Patch.OverrideSimple = true;
            return true;
        }

        public static void Postfix()
        {
            Calendar_getTime_Patch.OverrideSimple = false;
        }
    }
}