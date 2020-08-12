using System;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class NightVisionX : DarkVision
    {
        public NightVisionX()
        {
            DisplayName = "Night Vision";
            MaxLevel = 10;
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
            return GetLightRadiusForLevel(level) * 2;
        }

        public override string GetDescription()
        {
            return "You see in the dark.";
        }

        public override string GetLevelText(int level)
        {
            return $"Sight Radius: {GetLightRadiusForLevel(level)}";
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