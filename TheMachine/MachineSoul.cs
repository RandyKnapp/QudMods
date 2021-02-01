using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts
{
    public class MachineSoul : IPart
    {
        [Serializable]
        public class Battery
        {
            public int Energy;
        }

        public const int EnergyRequiredForResurrection = 100;
        public const string TheMachineZone = "JoppaWorld.11.22.1.1.11";
        public const int TheMachineLocationX = 32;
        public const int TheMachineLocationY = 11;

        public Battery[] BackupBatteries = new Battery[]
        {
            new Battery { Energy = EnergyRequiredForResurrection },
            new Battery { Energy = EnergyRequiredForResurrection },
            new Battery { Energy = EnergyRequiredForResurrection },
        };
        public Battery MainBattery = new Battery { Energy = 4 };

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == BeforeDieEvent.ID;
        }

        public override bool HandleEvent(BeforeDieEvent e)
        {
            if (HasEnergyForResurrection())
            {
                var successfulResurrect = ResurrectPlayer(e);
                if (!successfulResurrect)
                {
                    Popup.Show("Normality blocked The {G|The Machine}} from retrieving your machine soul.");
                    XRLCore.Core.Game.DeathReason += "\n{{G|The Machine}} could not retrieve your soul due to a {{normal|normality lattice}}.";
                    return base.HandleEvent(e);
                }
                return false;
            }
            else
            {
                XRLCore.Core.Game.DeathReason += "\n{{G|The Machine}} lacked the energy to resurrect you.";
            }

            return base.HandleEvent(e);
        }

        public bool HasEnergyForResurrection()
        {
            if (MainBattery.Energy >= EnergyRequiredForResurrection)
            {
                return true;
            }
            else
            {
                foreach (Battery backupBattery in BackupBatteries)
                {
                    if (backupBattery.Energy >= EnergyRequiredForResurrection)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ResurrectPlayer(BeforeDieEvent beforeDieEvent)
        {
            HealToFull();
            CreatePlayerCorpse();
            DropAllInventory(beforeDieEvent);
            SpendEnergy();
            return TeleportToTheMachine();
        }

        private void CreatePlayerCorpse()
        {
            Cell dropCell = ParentObject.GetDropCell();
            if (dropCell != null)
            {
                GameObject corpse = CreatePlayerCorpseObject();
                dropCell.AddObject(corpse);
            }
        }

        private GameObject CreatePlayerCorpseObject()
        {
            var corpse = GameObject.create("Human Corpse");
            corpse.SetIntProperty("MachineSoulCorpse", 1);
            corpse.pRender.DisplayName = Grammar.MakePossessive(ParentObject.DisplayName) + " corpse";
            corpse.pRender.ColorString = "&O";
            corpse.pRender.TileColor = "&O";

            AddButcherableCyberneticsToCorpse(corpse);

            return corpse;
        }

        private void AddButcherableCyberneticsToCorpse(GameObject corpse)
        {
            Body body = ParentObject.Body;
            if (body != null)
            {
                List<GameObject> cybernetics = Event.NewGameObjectList();
                foreach (BodyPart part in body.GetParts())
                {
                    if (part.Cybernetics != null)
                    {
                        cybernetics.Add(part.Cybernetics);
                        part.Cybernetics.FireEvent(Event.New("Unimplanted", "Object", ParentObject));
                    }
                }
                if (cybernetics.Count > 0)
                {
                    corpse.AddPart(new Cybernetics2ButcherableCybernetic(cybernetics));
                    corpse.RemovePart("Food");
                }
            }
        }

        private void DropAllInventory(BeforeDieEvent beforeDieEvent)
        {
            Cell dropCell = ParentObject.GetDropCell();
            if (dropCell != null)
            {
                UnequipAllEquipment();

                Inventory inventory = ParentObject.Inventory;
                if (inventory != null && inventory.Objects.Count > 0)
                {
                    List<GameObject> list = Event.NewGameObjectList();
                    list.AddRange(inventory.Objects);
                    inventory.Objects.Clear();
                    foreach (GameObject inventoryObject in list)
                    {
                        inventoryObject.RemoveFromContext();
                        if (inventoryObject.IsReal && DropOnDeathEvent.Check(inventoryObject, dropCell))
                        {
                            dropCell.AddObject(inventoryObject);
                            inventoryObject.HandleEvent(DroppedEvent.FromPool(null, inventoryObject));
                        }
                    }
                }
            }
        }

        private void UnequipAllEquipment()
        {
            Body body = ParentObject.Body;
            if (body != null)
            {
                foreach (BodyPart part in body.GetParts())
                {
                    if (part.Equipped != null)
                    {
                        part.ForceUnequip(true);
                    }
                }
            }
        }

        private void HealToFull()
        {
            RegenerateAllBodyParts();
            RemoveNegativeEffects();
            CureAllFungus();
            CureAllDisease();
            HitpointsToFull();
        }

        private void RegenerateAllBodyParts()
        {
            Body.DismemberedPart dismemberedPart;
            while ((dismemberedPart = ParentObject.Body.FindRegenerablePart()) != null)
            {
                ParentObject.Body.RegenerateLimb(true, dismemberedPart, true);
            }
        }

        private void HitpointsToFull()
        {
            const string hpStatName = "Hitpoints";
            if (ParentObject.HasStat(hpStatName))
            {
                var hitPointsStat = ParentObject.Statistics[hpStatName];
                ParentObject.Heal(hitPointsStat.Max);
            }
        }

        private void CureAllFungus()
        {
            ParentObject.RemoveEffect("FungalSporeInfection");
            ParentObject.RemoveEffect("SporeCloudPoison");

            foreach (BodyPart bodyPart in ParentObject.Body.GetParts())
            {
                if (bodyPart.Equipped != null && bodyPart.Equipped.HasTag("FungalInfection"))
                {
                    bodyPart.Equipped.Destroy();
                }
            }
        }

        private void CureAllDisease()
        {
            if (ParentObject.HasEffect("IronshankOnset"))
            {
                ParentObject.GetEffect("IronshankOnset").Duration = 0;
            }
            if (ParentObject.HasEffect("Ironshank"))
            {
                ParentObject.GetEffect("Ironshank").Duration = 0;
            }
            if (ParentObject.HasEffect("GlotrotOnset"))
            {
                ParentObject.GetEffect("GlotrotOnset").Duration = 0;
            }
            if (ParentObject.HasEffect("Glotrot"))
            {
                ParentObject.GetEffect("Glotrot").Duration = 0;
            }
            if (ParentObject.HasEffect("MonochromeOnset"))
            {
                ParentObject.GetEffect("MonochromeOnset").Duration = 0;
            }
            if (ParentObject.HasEffect("Monochrome"))
            {
                ParentObject.GetEffect("Monochrome").Duration = 0;
            }
        }

        private void RemoveNegativeEffects()
        {
            ParentObject.FireEvent("Recuperating");

            if (ParentObject.Effects != null)
            {
                foreach (Effect effect2 in ParentObject.Effects)
                {
                    effect2.Duration = 0;
                }
            }
            ParentObject.Statistics["Strength"].Penalty = 0;
            ParentObject.Statistics["Agility"].Penalty = 0;
            ParentObject.Statistics["Intelligence"].Penalty = 0;
            ParentObject.Statistics["Toughness"].Penalty = 0;
            ParentObject.Statistics["Willpower"].Penalty = 0;
            ParentObject.Statistics["Ego"].Penalty = 0;
            ParentObject.Statistics["Speed"].Penalty = 0;

            ParentObject.CleanEffects();
        }

        private void SpendEnergy()
        {
            if (MainBattery.Energy >= EnergyRequiredForResurrection)
            {
                MainBattery.Energy -= EnergyRequiredForResurrection;
            }
            else
            {
                foreach (Battery backupBattery in BackupBatteries)
                {
                    if (backupBattery.Energy >= EnergyRequiredForResurrection)
                    {
                        backupBattery.Energy = 0;
                        return;
                    }
                }
            }
        }

        private bool TeleportToTheMachine()
        {
            return ParentObject.ZoneTeleport(TheMachineZone, TheMachineLocationX, TheMachineLocationY, null, null, null, null,
                $"The Machine has resurrected you! (Energy Remaining: {GetEnergyRemainingString()})");
        }

        private string GetEnergyRemainingString()
        {
            int backupCount = 0;
            foreach (Battery backupBattery in BackupBatteries)
            {
                if (backupBattery.Energy >= EnergyRequiredForResurrection)
                {
                    backupCount++;
                }
            }

            string backupText = (backupCount == 0) ? "{{R|NO BACKUPS!}}" : $"{{{{B|{backupCount} BACKUPS}}}}";
            return $"{{{{G|{MainBattery.Energy}%}}}} [{backupText}]";
        }
    }
}