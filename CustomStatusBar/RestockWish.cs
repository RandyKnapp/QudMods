using System.Collections.Generic;
using System.Reflection;
using XRL.Core;
using XRL.World;
using XRL.World.Encounters.EncounterObjectBuilders;
using XRL.World.Parts;

namespace SimplifiedLog
{
    public class RestockWish
    {
        private static readonly MethodInfo GenericInventoryRestocker_PerformRestock = typeof(GenericInventoryRestocker).GetMethod("PerformRestock", BindingFlags.NonPublic | BindingFlags.Instance);

        [XRL.Wish.WishCommand(Command = "restock")]
        public static void RestockWishHandler()
        {
            var thePlayer = XRLCore.Core.Game.Player.Body;
            List<GameObject> objects = thePlayer.CurrentZone.GetObjects();
            foreach (var gameObject in objects)
            {
                if (gameObject.GetPart<GenericInventoryRestocker>() is GenericInventoryRestocker restocker)
                {
                    GenericInventoryRestocker_PerformRestock.Invoke(restocker, new object[] { false });
                    continue;
                }
            }
        }
    }
}