using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.WorldBuilders;

namespace TheMachine
{
    [JoppaWorldBuilderExtension]
    public class TheMachineJoppaWorldExtension : IJoppaWorldBuilderExtension
    {
        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            if (CreateCharacter.Template.Genotype == "Machine Soul")
            {
                // Do a custom terrain instead of Joppa (copied from JoppaWorldBuilder.cs)
                Factions.get("Joppa").Visible = false;
                Cell currentCell = builder.WorldZone.GetFirstObject((GameObject o) => o.Blueprint == "TerrainJoppa").GetCurrentCell();
                currentCell.Clear();
                currentCell.AddObject("TerrainTheMachine");

                // Custom embark string state for custom opening message
                XRLCore.Core.Game.SetStringGameState("embark", "TheMachine");
            }
        }
    }
}