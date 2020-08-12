using System;
using XRL.Rules;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class HeightenedSpeedX : BaseMutation
    {
        public int Bonus;

        public HeightenedSpeedX()
        {
            this.DisplayName = "Heightened Quickness";
            this.Type = "Physical";
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == GetItemElementsEvent.ID;
        }

        public override bool HandleEvent(GetItemElementsEvent E)
        {
            E.Add("travel", 1);
            return true;
        }

        public override void Register(GameObject Object)
        {
            base.Register(Object);
        }

        public int GetSpeedBonus(int Level)
        {
            switch (Level)
            {
                case 1:
                    return 7;
                case 2:
                    return 9;
                case 3:
                    return 11;
                case 4:
                    return 13;
                case 5:
                    return 15;
                case 6:
                    return 17;
                case 7:
                    return 19;
                case 8:
                    return 21;
                case 9:
                    return 23;
                default:
                    return 25;
            }
        }

        public override string GetDescription()
        {
            return "You are gifted with tremendous speed.";
        }

        public override string GetLevelText(int Level)
        {
            return "+" + (object) this.GetSpeedBonus(Level) + " Quickness";
        }

        public override bool FireEvent(Event E)
        {
            return base.FireEvent(E);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            Bonus = GetSpeedBonus(NewLevel);
            StatShifter.DefaultDisplayName = "quickness bonus";
            StatShifter.SetStatShift(ParentObject, "Speed", Bonus);

            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            this.Bonus = 0;
            StatShifter.RemoveStatShift(ParentObject, "Speed");
            return base.Unmutate(GO);
        }
    }
}