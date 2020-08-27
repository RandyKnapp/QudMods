using System;
using XRL.Core;
using XRL.UI;
using Random = System.Random;

namespace XRL.World.Parts
{
    [Serializable]
    public class Randy_RobotTinkerItem : IPart
    {
        private const int HP_PER_BIT = 10;
        private const int HP_PER_ADVBIT = 50;

        public string RepairCost => RepairCostBasic + RepairCostRusted + RepairCostAdvanced;

        public int RepairedCount;
        public string RepairCostBasic;
        public string RepairCostAdvanced;
        public string RepairCostRusted;

        public override void Attach()
        {
            ResetRepairCost();
            CalculateRepairCost();
            base.Attach();
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "TinkerRepaired");
            base.Register(Object);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == StatChangeEvent.ID;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "TinkerRepaired")
            {
                ResetRepairCost();
                RepairedCount++;
            }
            return base.FireEvent(E);
        }

        public override bool HandleEvent(StatChangeEvent E)
        {
            if (E.Name == "Hitpoints")
            {
                CalculateRepairCost();
            }
            return base.HandleEvent(E);
        }

        public void ResetRepairCost()
        {
            RepairCostBasic = string.Empty;
            RepairCostAdvanced = string.Empty;
            RepairCostRusted = string.Empty;
        }

        private void CalculateRepairCost(int additionalDamage = 0)
        {
            var obj = ParentObject;
            if (additionalDamage == 0 && !obj.isDamaged())
            {
                ResetRepairCost();
                return;
            }

            RepairCostBasic = string.IsNullOrEmpty(RepairCostBasic) ? string.Empty : RepairCostBasic;
            RepairCostRusted = string.IsNullOrEmpty(RepairCostRusted) ? string.Empty : RepairCostRusted;
            RepairCostAdvanced = string.IsNullOrEmpty(RepairCostAdvanced) ? string.Empty : RepairCostAdvanced;

            bool isRusted = obj.HasEffect("Rusted");
            int amountToRepair = (obj.baseHitpoints - obj.hitpoints) + additionalDamage;
            int level = obj.GetStat("Level").Value;
            int maxAdvancedBitIndex = level / 10;

            Random random = new Random(XRLCore.Core.Game.GetWorldSeed(obj.Blueprint + "RepairCost" + RepairedCount + "" + amountToRepair));

            int basicBitCount = GetBasicBitCount(amountToRepair);
            int rustedBitCount = GetRustedBitCount(isRusted);
            int advancedBitCount = GetAdvancedBitCount(amountToRepair);

            // Basic bits, starting metal bit, add 1 plus an additional bit for every 10 HP repaired
            while (RepairCostBasic.Length != basicBitCount)
            {
                if (RepairCostBasic.Length > basicBitCount)
                {
                    RepairCostBasic = RepairCostBasic.Remove(RepairCostBasic.Length - 1);
                }
                else if (RepairCostBasic.Length < basicBitCount)
                {
                    RepairCostBasic += RepairCostBasic.Length == 0 ? 'B' : GetBasicBit(random);
                }
            }

            // Rusted bits, add an additional basic and advanced bit if the robot is rusted
            if (RepairCostRusted.Length != rustedBitCount)
            {
                RepairCostRusted = "" + GetBasicBit(random) + GetAdvancedBit(random, maxAdvancedBitIndex);
            }

            // Advanced bits, add 1 if at least 10 damage to repair plus an additional advanced bit for every 50 HP repaired,
            // with higher complexity bits possible if high level
            while (RepairCostAdvanced.Length != advancedBitCount)
            {
                if (RepairCostAdvanced.Length > advancedBitCount)
                {
                    RepairCostAdvanced = RepairCostAdvanced.Remove(RepairCostAdvanced.Length - 1);
                }
                else if (RepairCostAdvanced.Length < advancedBitCount)
                {
                    RepairCostAdvanced += GetAdvancedBit(random, maxAdvancedBitIndex);
                }
            }
        }

        private int GetBasicBitCount(int amountToRepair)
        {
            return 2 + (amountToRepair / HP_PER_BIT);
        }

        private int GetRustedBitCount(bool isRusted)
        {
            return isRusted ? 2 : 0;
        }

        private int GetAdvancedBitCount(int amountToRepair)
        {
            return amountToRepair <= 10 ? 0 : 1 + (amountToRepair / HP_PER_ADVBIT);
        }

        private static char GetAdvancedBit(Random random, int maxAdvancedBitIndex)
        {
            const string ADV_BITS = "rgbc";
            return ADV_BITS[random.Next(0, maxAdvancedBitIndex)];
        }

        private static char GetBasicBit(Random random)
        {
            const string BASIC_BITS = "RGBC";
            return BASIC_BITS[random.Next(0, BASIC_BITS.Length - 1)];
        }
    }
}