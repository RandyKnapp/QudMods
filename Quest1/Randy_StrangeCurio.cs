using System;
using XRL.UI;

namespace XRL.World.Parts
{
    [Serializable]
    public class Randy_StrangeCurio : IPart
    {
        public int ActivationChargeAmount = 10000;

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "PowerSwitchActivated");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "PowerSwitchActivated")
            {
                var powerSwitch = ParentObject.RequirePart<PowerSwitch>();
                var energyCellSocket = ParentObject.RequirePart<EnergyCellSocket>();
                var renderPart = ParentObject.RequirePart<Render>();
                if (energyCellSocket == null || energyCellSocket.Cell == null)
                {
                    Popup.Show("Nothing happens. You wonder if the device requires power of some kind.");
                    powerSwitch.Active = false;
                    return false;
                }

                if (!ParentObject.UseCharge(ActivationChargeAmount))
                {
                    Popup.Show("The energy source was drained, but the device didn't do anything. Does it need more power?");
                    powerSwitch.Active = false;
                    return false;
                }

                Popup.Show("The device seems to power on. Orange light fills the crystal at its end. The device gracefully and quickly unfolds into an intricate collection of wires, frames, and screens. It becomes cumbersome to continue holding, you place the device on the ground.");

                Cell adjacentCell = ThePlayer.GetCurrentCell().GetFirstEmptyAdjacentCell();
                var umpTerminal = adjacentCell.AddObject("UmpTerminal");

                ParentObject.Destroy("Destroyed and turned into ump terminal", false, false);

                //Cybernetics2Terminal.ShowTerminal(umpTerminal, ThePlayer, new UmpMainScreen());
            }

            return base.FireEvent(E);
        }
    }
}