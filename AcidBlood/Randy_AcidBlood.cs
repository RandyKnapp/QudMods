using System;
using HarmonyLib;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation
{
    //Bleeding.SyncVersion()
    [HarmonyPatch(typeof(Bleeding))]
    [HarmonyPatch("SyncVersion")]
    public class Randy_AcidBlood_Patch
    {
        public static bool Prefix(Bleeding __instance)
        {
            var acidBloodPart = __instance.Object.GetPart<Randy_AcidBlood>();
            if (acidBloodPart != null)
            {
                __instance.DisplayName = "bleeding acid";
                return false;
            }

            return true;
        }
    }

    [Serializable]
    public class Randy_AcidBlood : BaseMutation
    {
        public int AcidResistBonus = 10;

        public Randy_AcidBlood()
        {
            DisplayName = "Acid blood";
            Type = "Physical";
        }

        public override bool CanLevel()
        {
            return false;
        }

        public override string GetDescription()
        {
            return "You have acid instead of blood pumping through your body. Under your skin, your veins are a highly-visible bright green.";
        }

        public override string GetLevelText(int Level)
        {
            return "{{green|You are immune to acid damage.}}";
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "BeforeApplyDamage");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "BeforeApplyDamage")
            {
                if (E.GetParameter("Damage") is Damage damage && damage.HasAttribute("Acid"))
                {
                    damage.Amount = 0;
                    return false;
                }
            }
            return base.FireEvent(E);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            GO.SetStringProperty("BleedLiquid", "acid-1000");
            GO.SetStringProperty("BleedColor", "&G");
            GO.SetStringProperty("BleedPrefix", "&Gacidic");
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            GO.RemoveStringProperty("BleedLiquid");
            GO.RemoveStringProperty("BleedColor");
            GO.RemoveStringProperty("BleedPrefix");
            return base.Unmutate(GO);
        }
    }
}