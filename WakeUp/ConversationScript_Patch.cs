using HarmonyLib;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

namespace WakeUp
{
    [HarmonyPatch(typeof(ConversationScript))]
    [HarmonyPatch("IsPhysicalConversationPossible", new[] {typeof(GameObject), typeof(bool), typeof(int)})]
    public class ConversationScript_Patch
    {
        public static bool Prefix(GameObject With, bool ShowPopup, int ChargeUse)
        {
            if (With.GetEffect<Asleep>() is Asleep sleepEffect && ShowPopup)
            {
                if (!sleepEffect.CanWake(XRLCore.Core.Game.Player.Body))
                {
                    return true;
                }

                var usesSleepMode = Asleep.UsesSleepMode(With);
                var name = With.the + With.ShortDisplayName;
                var message = usesSleepMode ? $"{name}{With.Is}  utterly unresponsive." : $"{name}{With.GetVerb("snore", true, false)} loudly.\n";
                message += $"Do you want to {(usesSleepMode ? "activate" : "wake")} {With.them}?";
                var select = Popup.ShowOptionList(string.Empty,
                    new [] { $"{{{{W|N}}}}o, leave {With.them} be.", (usesSleepMode ? $"{{{{W|Y}}}}es, activate {With.them}" : $"{{{{W|Y}}}}es, wake {With.them} up.") },
                    new [] { 'n', 'y' },
                    0, message, 60, true, true);
                if (select == 1)
                {
                    sleepEffect.HandleEvent(new InventoryActionEvent()
                    {
                        Command = "WakeSleeper",
                        Actor = XRLCore.Core.Game.Player.Body
                    });
                }
                return false;
            }

            return true;
        }
    }
}