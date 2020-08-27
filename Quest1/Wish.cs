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

            var player = XRLCore.Core.Game.Player.Body;
            player.GetPart<Mutations>().AddMutation("Clairvoyance", 10);
            player.GetStat("Ego").BaseValue = 18;
            return true;
        }
    }
}