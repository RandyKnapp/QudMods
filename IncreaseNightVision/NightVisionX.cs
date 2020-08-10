using System;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class NightVisionX : DarkVision
    {
        public NightVisionX()
        {
            Name = "NightVisionX";
            DisplayName = "Night Vision (Improved)";
        }

        public override bool CanLevel()
        {
            return true;
        }

        private int GetLightRadiusForLevel(int level)
        {
            return Radius + level - 1;
        }

        private int GetBonusRadiusForLevel(int level)
        {
            return GetLightRadiusForLevel(level) + level;
        }

        public override string GetLevelText(int level)
        {
            string Ret = "You can see in the dark.\n";
            Ret += $"Light Radius: {GetLightRadiusForLevel(level)}\n";
            Ret += $"Bonus Radius: {GetBonusRadiusForLevel(level)}" ;
            return Ret;
        }

        public override bool HandleEvent(BeforeRenderEvent e)
        {
            if (ParentObject.IsPlayer())
            {
                ParentObject.CurrentCell?.ParentZone.AddLight(currentCell.X, currentCell.Y, GetLightRadiusForLevel(Level));
                ParentObject.CurrentCell?.ParentZone.AddExplored(currentCell.X, currentCell.Y, GetBonusRadiusForLevel(Level));
            }
            return true;
        }
    }
}