using System;
using XRL.UI;
using XRL.World.Tinkering;

namespace XRL.World.Parts.Skill
{
    [Serializable]
    public class Randy_Tinkering_RepairRobot : BaseSkill
    {
        public Randy_Tinkering_RepairRobot()
        {
            DisplayName = nameof(Randy_Tinkering_RepairRobot);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == InventoryActionEvent.ID || ID == OwnerGetInventoryActionsEvent.ID;
        }

        public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
        {
            if (ParentObject.HasSkill(nameof(Randy_Tinkering_RepairRobot)) && IsRepairable(E.Object))
            {
                E.AddAction("RepairRobot", "repair", "RepairRobot", ' ', true, 300);
            }
            return true;
        }

        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (E.Command == "RepairRobot")
            {
                GameObject actor = E.Actor;
                GameObject robot = E.Item;
                BitLocker bitLocker = actor.RequirePart<BitLocker>();
                if (actor.IsPlayer() && !robot.Understood())
                {
                    Popup.ShowBlock("You cannot repair " + robot.the + robot.DisplayNameOnly + " until you understand " + robot.them + ".");
                }
                else
                {
                    bool didRepair = false;
                    string repairCost = CalculateRepairCost(E.Item);
                    if (actor.IsPlayer())
                    {
                        if (!bitLocker.HasBits(repairCost))
                        {
                            Popup.ShowBlock("You don't have " + BitType.GetString(repairCost) + " &yto repair " + robot.the + robot.BaseDisplayName + "! You have:\n\n" + bitLocker.GetBitsString());
                            goto label_13;
                        }

                        if (Popup.ShowYesNoCancel("Do you want to spend " + BitType.GetString(repairCost) + " &yto repair " + robot.the + robot.BaseDisplayName + "? You have:\n\n" + bitLocker.GetBitsString()) == DialogResult.Yes)
                        {
                            didRepair = true;
                            bitLocker.UseBits(repairCost);
                        }
                    }
                    else if (IsRepairableBy(robot, actor, repairCost))
                    {
                        didRepair = true;
                    }
                    else
                    {
                        actor.FireEvent(Event.New("UnableToRepair", "Object", robot));
                    }

                    if (didRepair)
                    {
                        XDidYToZ(actor, "repair", robot);
                        int energyCost = 1000 + (1000 * (repairCost.Length / 6));
                        actor.UseEnergy(energyCost, "Skill Tinkering Repair");
                        RepairRobot(robot);
                    }
                }
            }

            label_13:
            return true;
        }

        public static bool IsRepairable(GameObject obj)
        {
            bool isNonHostileRobot = obj.HasTag("Robot") && (obj.GetPart<Brain>() is Brain brain) && !brain.Hostile;
            bool needsRepair = obj.isDamaged() || (obj.HasEffect("Rusted") || obj.HasEffect("Broken") || (obj.HasPart("Repair") || obj.HasEffect("ShatteredArmor"))) || obj.HasEffect("ShatterArmor");
            return isNonHostileRobot && needsRepair;
        }

        public bool IsRepairable()
        {
            return IsRepairable(ParentObject);
        }

        public static bool IsRepairableBy(GameObject obj, GameObject actor, string RepairCost = null)
        {
            if (!actor.HasSkill(nameof(Randy_Tinkering_RepairRobot)) || !IsRepairable(obj))
            {
                return false;
            }

            int tier = GetRepairTier(RepairCost ?? CalculateRepairCost(obj));
            if (obj.GetPart<Repair>() is Repair part)
            {
                int num = Math.Min(Math.Max(part.Difficulty / 2, 0), 8);
                if (num > tier)
                {
                    tier = num;
                }
            }

            return tier <= 0 || actor.HasSkill(DataDisk.GetRequiredSkill(tier));
        }

        public bool IsRepairableBy(GameObject actor, string RepairCost = null)
        {
            return IsRepairableBy(ParentObject, actor, RepairCost);
        }

        public static void RepairRobot(GameObject obj)
        {
            obj.RemoveEffect("Rusted");
            obj.RemoveEffect("Broken");
            obj.RemoveEffect("ShatterArmor");
            obj.RemoveEffect("ShatteredArmor");
            obj.hitpoints = obj.baseHitpoints;
            obj.FireEvent("TinkerRepaired");
        }

        public static int GetRepairTier(string RepairCost)
        {
            int num = 0;
            int index = 0;
            for (int length = RepairCost.Length; index < length; ++index)
            {
                int bitTier = BitType.GetBitTier(RepairCost[index]);
                if (bitTier > num)
                    num = bitTier;
            }

            return num;
        }

        public static string CalculateRepairCost(GameObject obj)
        {
            Randy_RobotTinkerItem part = obj.RequirePart<Randy_RobotTinkerItem>();
            return part.RepairCost;
        }

        public override bool AddSkill(GameObject GO)
        {
            return true;
        }

        public override bool RemoveSkill(GameObject GO)
        {
            return true;
        }
    }
}