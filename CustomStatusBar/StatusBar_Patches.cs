using System;
using System.Reflection;
using HarmonyLib;
using Qud.UI;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using GameObject = XRL.World.GameObject;

/*namespace CustomStatusBar
{
    [HarmonyPatch(typeof(PlayerStatusBar))]
    [HarmonyPatch("BeginEndTurn")]
    public class StatusBar_Patches
    {
        private static readonly MethodInfo PlayerStatusBar_UpdateString = typeof(PlayerStatusBar).GetMethod("UpdateString", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(PlayerStatusBar __instance, XRLCore core)
        {
            var body = core.Game.Player.Body;
            UpdateLocationText(__instance, body);
            UpdateTimeText(__instance);
            UpdateHpBar(__instance, body);
            UpdateHpText(__instance, body);
        }

        private static void UpdateLocationText(PlayerStatusBar __instance, GameObject body)
        {
            Cell currentCell = body.GetCurrentCell();
            if (currentCell != null && currentCell.ParentZone != null)
            {
                string zoneString = currentCell.ParentZone.DisplayName;
                var parts = zoneString.Split(new string[] {", "}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length <= 2)
                {
                    UpdateString(__instance, 4, zoneString);
                }
                else
                {
                    var output = parts[0] + ", " + parts[1] + "\n";
                    string[] rest = new string[parts.Length - 2];
                    Array.Copy(parts, 2, rest, 0, parts.Length - 2);
                    output += string.Join(", ", rest);
                    UpdateString(__instance, 4, output);
                }
            }
        }

        private static void UpdateTimeText(PlayerStatusBar __instance)
        {
            // Inserting a newline into the time string, and re-coloring the date
            UpdateString(__instance, 1, Calendar.getTime() + "\n" + Calendar.getDay() + " of " + Calendar.getMonth());
        }

        private static void UpdateHpBar(PlayerStatusBar __instance, GameObject body)
        {
            // Make the HP bar the health status color
            string statusColor = Strings.HealthStatusColor(body);
            switch (statusColor)
            {
                case "Y":
                case "G":
                    ColorUtility.TryParseHtmlString("#009403", out __instance.HPBar.BarColor);
                    break;

                case "W":
                    ColorUtility.TryParseHtmlString("#b0a337", out __instance.HPBar.BarColor);
                    break;

                case "R":
                case "r":
                    ColorUtility.TryParseHtmlString("#a64a2e", out __instance.HPBar.BarColor);
                    break;
            }

            __instance.HPBar.UpdateBar();
        }

        private static void UpdateHpText(PlayerStatusBar __instance, GameObject body)
        {
            // Make the text on the HP Bar all white
            if (body.GetIntProperty("Analgesia") > 0)
            {
                UpdateString(__instance, 5, "&YHP: " + ConsoleLib.Console.ColorUtility.StripFormatting(Strings.WoundLevel(body)));
            }
            else
            {
                UpdateString(__instance, 5, "{{Y|HP: " + body.hitpoints + " / " + body.baseHitpoints + "}}");
            }
        }

        private static void UpdateString(PlayerStatusBar __instance, int type, string text, bool toRTF = true)
        {
            PlayerStatusBar_UpdateString.Invoke(__instance, new object[] {type, text, toRTF});
        }
    }
}*/