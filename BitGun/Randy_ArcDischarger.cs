using System;
using System.Collections.Generic;
using Genkit;
using XRL.Rules;

namespace XRL.World.Parts
{
    [Serializable]
    public class Randy_ArcDischarger : IActivePart
    {
        public int NumberOfArcs = 4;
        public string Damage = "1d2+2";
        public int Voltage = 4;
        public int Range = 1;
        public int EnergyCost = 1000;

        public Randy_ArcDischarger()
        {
            WorksOnSelf = true;
            ChargeUse = 0;
            IsEMPSensitive = false;
            IsBootSensitive = false;
            IsTechScannable = false;
            NameForStatus = "ArcDischarger";
        }

        public override bool AllowStaticRegistration()
        {
            return true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "WantsFireEvent");
            Object.RegisterPartEvent(this, "CommandFireMissileWeapon");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WantsFireEvent")
            {
                var list = E.GetParameter("WantsEvent") as List<GameObject>;
                if (!list.Contains(ParentObject))
                {
                    list.Add(ParentObject);
                }
            }
            if (E.ID == "CommandFireMissileWeapon")
            {
                TryDischarge(E);
                return false;
            }

            return base.FireEvent(E);
        }

        private bool TryDischarge(Event E)
        {
            var owner = ParentObject.Equipped;
            if (!ParentObject.FireEvent(Event.New("CheckLoadAmmo", "Loader", owner)))
            {
                return false;
            }

            if (!ParentObject.FireEvent(Event.New("LoadAmmo", "Loader", owner, "Ammo", null, "AmmoObject", null)))
            {
                return false;
            }

            Discharge(owner);
            return true;
        }

        private void Discharge(GameObject owner)
        {
            NumberOfArcs = Calc.Clamp(NumberOfArcs, 0, 8);
            List<Point> directions = new List<Point>
            {
                new Point(-1, 1),
                new Point(0, 1),
                new Point(1, 1),
                new Point(-1, 0),
                new Point(1, 0),
                new Point(-1, -1),
                new Point(0, -1),
                new Point(1, -1)
            };

            for (var i = 0; i < NumberOfArcs; ++i)
            {
                var randomDir = Stat.Random(0, directions.Count - 1);
                var dir = directions[randomDir];
                directions.RemoveAt(randomDir);

                owner.Discharge(owner.CurrentCell.GetCellFromOffset(dir.X * Range, dir.Y * Range), Voltage, Damage, owner);
            }

            owner.UseEnergy(EnergyCost, "Arc Discharge");
            PlayWorldSound(ParentObject.GetTagOrStringProperty("MissileFireSound", "Electric"));
        }
    }
}