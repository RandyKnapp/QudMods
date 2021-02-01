using XRL.Core;
using XRL.World.Parts;

namespace Quest1
{
    public class TestWishHandler
    {
        [XRL.Wish.WishCommand(Command = "q")]
        public static bool Q()
        {
            XRLCore.Core.IDKFA = true;
            XRLCore.Core.Calm = true;
            XRLCore.Core.cool = true;
            return true;
        }

        [XRL.Wish.WishCommand(Command = "c")]
        public static bool C()
        {
            XRLCore.Core.Game.Player.Body.RequirePart<Inventory>().AddObject("Quest_StrangeCurio", true);
            return true;
        }
    }
}