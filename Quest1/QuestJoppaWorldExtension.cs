using System;
using XRL.Core;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.WorldBuilders;

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

    [JoppaWorldBuilderExtension]
    public class QuestJoppaWorldExtension : IJoppaWorldBuilderExtension
    {
        private static readonly bool enableLogging = true;

        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            FindAndReplaceRandomCellObject(builder, "TerrainRustWell", "TerrainRustWell_Quest");
        }

        protected static void FindAndReplaceRandomCellObject(JoppaWorldBuilder builder, string objectName, string replaceWithThisObject)
        {
            var cells = builder.WorldZone.GetCellsWithObject(objectName);
            if (cells.Count == 0)
            {
                Log($"ReplaceOneOfCellWithObject: Could not find any cells with object={objectName}");
                return;
            }

            Log($"ReplaceOneOfCellWithObject: Found {cells.Count} cells with object={objectName}");
            var randomCell = cells.GetRandomElement();
            Log($"ReplaceOneOfCellWithObject: Selected cell at ({randomCell.X}, {randomCell.Y})");

            var originalObject = randomCell.GetFirstObject(objectName);
            if (originalObject == null)
            {
                Log($"OnAfterBuild: Selected cell did not have {objectName} object!");
                return;
            }
            randomCell.RemoveObject(originalObject);
            randomCell.AddObject(replaceWithThisObject);
        }

        private static void Log(string message)
        {
            if (enableLogging)
            {
                XRLCore.Log($"[{nameof(QuestJoppaWorldExtension)}] {message}");
            }
        }
    }
}