using System;
using XRL.UI;

namespace XRL.World.Parts
{
    [Serializable]
    public class UmpDevice : IActivePart
    {
		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
			{
				return ID == InventoryActionEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetInventoryActionsEvent E)
		{
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				E.AddAction("Interface", "interface", "InterfaceWithBecomingNook", 'i', FireOnActor: false, 100);
			}
			return true;
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "InterfaceWithBecomingNook" && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				if (E.Actor == null || !E.Actor.IsTrueKin())
				{
					Cybernetics2Terminal.ShowTerminal(ParentObject, E.Actor, new CyberneticsScreenGoodbye());
				}
				else
				{
					Cybernetics2Terminal.ShowTerminal(ParentObject, E.Actor, new CyberneticsScreenMainMenu());
				}
				E.RequestInterfaceExit();
			}
			return true;
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "CanSmartUse");
			Object.RegisterPartEvent(this, "CommandSmartUse");
			base.Register(Object);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "CanSmartUse")
			{
				if (E.GetGameObjectParameter("User").IsPlayer() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					return false;
				}
			}
			else if (E.ID == "CommandSmartUse")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("User");
				if (gameObjectParameter.IsPlayer() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					if (!gameObjectParameter.IsTrueKin())
					{
						Cybernetics2Terminal.ShowTerminal(ParentObject, gameObjectParameter, new CyberneticsScreenGoodbye());
					}
					else
					{
						Cybernetics2Terminal.ShowTerminal(ParentObject, gameObjectParameter, new CyberneticsScreenMainMenu());
					}
				}
			}
			return base.FireEvent(E);
		}
    }
}