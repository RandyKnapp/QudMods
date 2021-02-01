using System;
using XRL.Core;

namespace XRL.World.Parts.Skill
{
    public class Randy_ShortBlades_Rejoinder : ShortBlades_Rejoinder
    {
        private Guid ToggleAbilityId;

        [XRL.Wish.WishCommand(Command = "setup_rejoinder")]
        public static void SetupWishHandler()
        {
            var skills = ThePlayer.RequirePart<Skills>();
            bool hasSkill = ThePlayer.HasSkill("ShortBlades_Rejoinder");
            if (hasSkill)
            {
                ThePlayer.RemoveSkill("ShortBlades_Rejoinder");
                ThePlayer.AddSkill("Randy_ShortBlades_Rejoinder");
            }
        }

        [XRL.Wish.WishCommand(Command = "add_rejoinder")]
        public static void AddWishHandler()
        {
            var skills = ThePlayer.RequirePart<Skills>();
            bool hasSkill = ThePlayer.HasSkill("ShortBlades_Rejoinder");
            if (hasSkill)
            {
                ThePlayer.RemoveSkill("ShortBlades_Rejoinder");
            }

            ThePlayer.AddSkill("Randy_ShortBlades_Rejoinder");
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "CommandRejoinderToggle");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "DefenderAfterAttackMissed")
            {
                if (!IsMyActivatedAbilityToggledOn(ToggleAbilityId))
                {
                    Checked = true;
                }
            }
            else if (E.ID == "CommandRejoinderToggle")
            {
                ToggleMyActivatedAbility(ToggleAbilityId);
            }
            return base.FireEvent(E);
        }

        public override bool AddSkill(GameObject GO)
        {
            ToggleAbilityId = AddMyActivatedAbility("Rejoinder", "CommandRejoinderToggle", "Skill", "Toggle to enable or disable the harvesting of plants", "h", Toggleable: true, DefaultToggleState: true);
            return base.AddSkill(GO);
        }

        public override bool RemoveSkill(GameObject GO)
        {
            RemoveMyActivatedAbility(ref ToggleAbilityId);
            return base.RemoveSkill(GO);
        }
    }
}