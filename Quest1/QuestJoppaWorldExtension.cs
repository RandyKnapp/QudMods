using XRL.Core;
using XRL.World.WorldBuilders;

namespace Quest1
{
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