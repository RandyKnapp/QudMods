using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;
using XRL.Wish;
using XRL.World.Skills;
using XRL.World.Tinkering;

namespace XRL.World.Parts
{
    [Serializable]
    public class Randy_BitsAmmoLoader : IPoweredPart
    {
        public string ModeA;
        public string ModeB;
        public string ModeC;
        public string ModeD;
        public string Mode1;
        public string Mode2;
        public string Mode3;
        public string Mode4;
        public string Mode5;
        public string Mode6;
        public string Mode7;
        public string Mode8;
        public char CurrentLoadedBit;
        public string WeaponParts;
        public int ReloadEnergy = 1000;

        private string[] _weaponParts;

        public Randy_BitsAmmoLoader()
        {
            ChargeUse = 0;
            WorksOnEquipper = true;
            NameForStatus = "BitsWeaponSystem";
        }

        public string GetModeBlueprintName(char mode)
        {
            switch (mode)
            {
                case 'R': return ModeA;
                case 'G': return ModeB;
                case 'B': return ModeC;
                case 'C': return ModeD;
                case 'r': return Mode1;
                case 'g': return Mode2;
                case 'b': return Mode3;
                case 'c': return Mode4;
                case 'K': return Mode5;
                case 'W': return Mode6;
                case 'Y': return Mode7;
                case 'M': return Mode8;
                default: return string.Empty;
            }
        }

        public GameObjectBlueprint GetModeData(char mode)
        {
            string blueprintName = GetModeBlueprintName(mode);
            if (string.IsNullOrEmpty(blueprintName))
            {
                return null;
            }

            var blueprint = GameObjectFactory.Factory.GetBlueprint(blueprintName);
            return blueprint;
        }

        public GameObjectBlueprint GetCurrentModeData()
        {
            return GetModeData(CurrentLoadedBit);
        }

        public string GetProjectileBlueprint(char mode)
        {
            var modeData = GetModeData(mode);
            return modeData == null ? string.Empty : modeData.GetTag("ProjectileObject");
        }

        public string GetCurrentProjectileBlueprint()
        {
            return GetProjectileBlueprint(CurrentLoadedBit);
        }

        public GameObjectBlueprint GetProjectileData(string blueprint)
        {
            return string.IsNullOrEmpty(blueprint) ? null : GameObjectFactory.Factory.GetBlueprint(blueprint);
        }

        public GameObjectBlueprint GetProjectileData(char mode)
        {
            return GetProjectileData(GetProjectileBlueprint(mode));
        }

        public GameObjectBlueprint GetCurrentProjectileData()
        {
            return GetProjectileData(GetCurrentProjectileBlueprint());
        }

        public bool ModeUsesProjectile(char mode)
        {
            return GetProjectileBlueprint(mode) != "*none";
        }

        public bool CurrentModeUsesProjectile()
        {
            return ModeUsesProjectile(CurrentLoadedBit);
        }

        public string GetModeName(char mode)
        {
            var modeData = GetModeData(mode);
            return modeData?.GetPartParameter(nameof(Description), "Short") ?? "<unknown>";
        }

        public string GetCurrentModeName(bool withColor = false, bool trimModeName = false)
        {
            var modeName = GetModeName(CurrentLoadedBit);
            if (trimModeName && modeName.Length > 13)
            {
                modeName = modeName.Substring(0, 12);
            }
            return withColor ? $"{{{{{CurrentLoadedBit}|{modeName}}}}}" : modeName;
        }

        public string GetModeDescription(char mode)
        {
            var modeData = GetModeData(mode);
            return modeData?.GetPartParameter(nameof(Description), "Long") ?? "-";
        }

        public string GetCurrentModeDescription()
        {
            return GetModeDescription(CurrentLoadedBit);
        }

        public int GetAmmoCount(char mode)
        {
            var bitLocker = ParentObject.ThePlayer?.GetPart<BitLocker>();
            return bitLocker == null ? 0 : bitLocker.GetBitCount(mode);
        }

        public int GetCurrentAmmoCount()
        {
            return GetAmmoCount(CurrentLoadedBit);
        }

        public string GetRequiredSkillForMode(char mode)
        {
            var tier = BitType.GetBitTier(mode);
            if (tier == 0)
            {
                return "Tinkering";
            }
            if (tier <= 4)
            {
                return "Tinkering_Tinker1";
            }
            return tier <= 6 ? "Tinkering_Tinker2" : "Tinkering_Tinker3";
        }

        private bool SetCurrentBit(char bit)
        {
            if (bit == CurrentLoadedBit)
            {
                return false;
            }

            CurrentLoadedBit = bit;
            var modeData = GetCurrentModeData();
            if (modeData == null)
            {
                CurrentLoadedBit = (char)0;
            }

            if (CurrentLoadedBit == 0)
            {
                ParentObject.RequirePart<Render>().DetailColor = "w";
                PlayWorldSound("compartment_open_whine_down");
                return true;
            }

            // Set Shooting Sound
            ParentObject.SetStringProperty("MissileFireSound", modeData.GetTag("MissileFireSound"));

            // Set Reload Sound
            ParentObject.SetStringProperty("ReloadSound", modeData.GetTag("ReloadSound"));

            // Set Weapon Detail Color
            ParentObject.RequirePart<Render>().DetailColor = CurrentLoadedBit.ToString();

            // Set Weapon Parts
            SetupWeaponPart(modeData);

            // Set Reload Sound
            var reloadSound = GetCurrentAmmoCount() == 0 ? "compartment_open_whine_down" : ParentObject.GetPropertyOrTag("ReloadSound", "compartment_close");
            PlayWorldSound(reloadSound);
            return true;
        }

        private void SetupWeaponPart(GameObjectBlueprint modeData)
        {
            InitWeaponPartNames();
            RemoveAnyWeaponParts();
            CopyAllWeaponParts(modeData);
        }

        private void InitWeaponPartNames()
        {
            if (_weaponParts == null)
            {
                _weaponParts = WeaponParts.Split(',');
            }
        }

        private void RemoveAnyWeaponParts()
        {
            InitWeaponPartNames();
            foreach (var weaponPartName in _weaponParts)
            {
                ParentObject.RemovePart(weaponPartName);
            }
        }

        private void CopyAllWeaponParts(GameObjectBlueprint modeData)
        {
            InitWeaponPartNames();
            foreach (var weaponPartName in _weaponParts)
            {
                var weaponBlueprint = modeData.GetPart(weaponPartName);
                if (weaponBlueprint != null)
                {
                    var part = Activator.CreateInstance(weaponBlueprint.T) as IPart;
                    part.ParentObject = ParentObject;
                    weaponBlueprint.InitializePartInstance(part);
                    ParentObject.AddPart(part);
                }
            }
        }

        public string GetCurrentAmmoStatusDisplay(bool trimModeName = false)
        {
            var currentModeDisplay = CurrentLoadedBit == 0 ? "{{K|0}}" : GetCurrentModeName(true, trimModeName) + "-" + BitType.GetString(CurrentLoadedBit);
            var currentAmmoCount = GetCurrentAmmoCount();
            var currentAmmoCountDisplay = currentAmmoCount == 0 ? "{{K|empty}}" : currentAmmoCount.ToString();
            return $"[{currentModeDisplay}:{currentAmmoCountDisplay}]";
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade)
                || ID == GetDisplayNameEvent.ID
                || ID == GetMissileWeaponProjectileEvent.ID
                || ID == GetShortDescriptionEvent.ID;
        }

        public override bool HandleEvent(GetMissileWeaponProjectileEvent E)
        {
            E.Blueprint = GetCurrentProjectileBlueprint();
            return !string.IsNullOrEmpty(E.Blueprint);
        }

        public override bool HandleEvent(GetDisplayNameEvent E)
        {
            if (ParentObject.Understood())
            {
                E.AddMissileWeaponDamageTag(GetCurrentWeaponPerformanceEvent());
                E.AddTag(GetCurrentAmmoStatusDisplay());
            }
            return true;
        }

        public override bool HandleEvent(GetShortDescriptionEvent E)
        {
            E.Postfix.AppendLine();
            E.Postfix.Append($"[{{{{{CurrentLoadedBit}|Mode {BitType.GetString(CurrentLoadedBit)}}}}}: {{{{{CurrentLoadedBit}|{GetCurrentModeName()}}}}}]");
            E.Postfix.AppendLine();
            E.Postfix.Append($"- {GetCurrentModeDescription()}");
            E.Postfix.AppendLine();
            return true;
        }

        private GetMissileWeaponPerformanceEvent GetWeaponPerformanceEvent(char mode)
        {
            var projectileBlueprint = GetProjectileData(mode);
            if (projectileBlueprint == null)
            {
                return new GetMissileWeaponPerformanceEvent()
                {
                    BaseDamage = "0", BasePenetration = 0, Attributes = "", PenetrateCreatures = false
                };
            }
            else
            {
                GetMissileWeaponPerformanceEvent E = new GetMissileWeaponPerformanceEvent();
                int.TryParse(projectileBlueprint.GetPartParameter("Projectile", "BasePenetration"), out E.BasePenetration);
                E.BaseDamage = projectileBlueprint.GetPartParameter("Projectile", "BaseDamage");
                bool.TryParse(projectileBlueprint.GetPartParameter("Projectile", "PenetrateCreatures"), out E.PenetrateCreatures);
                E.Attributes = projectileBlueprint.GetPartParameter("Projectile", "Attributes");
                return E;
            }
        }

        private GetMissileWeaponPerformanceEvent GetCurrentWeaponPerformanceEvent()
        {
            return GetWeaponPerformanceEvent(CurrentLoadedBit);
        }

        private string GetWeaponPerformanceString(char mode)
        {
            return GetWeaponPerformanceString(GetWeaponPerformanceEvent(mode));
        }

        private string GetWeaponPerformanceString(GetMissileWeaponPerformanceEvent E)
        {
            StringBuilder stringBuilder = Event.NewStringBuilder();
            if (E.BasePenetration > -1)
            {
                stringBuilder.Append("{{").Append(E.PenetrateCreatures ? 'W' : 'c').Append('|').Append('\x001A').Append("}}");
                if (E.Attributes != null && E.Attributes.Contains("Vorpal"))
                {
                    stringBuilder.Append('÷');
                }
                else
                {
                    stringBuilder.Append(E.BasePenetration + 4);
                }
            }

            if (E.DamageRoll != null || !string.IsNullOrEmpty(E.BaseDamage) && E.BaseDamage != "0")
            {
                stringBuilder.Append(' ').Append("{{r|").Append('\x0003').Append("}}").Append(E.DamageRoll != null ? E.DamageRoll.ToString() : E.BaseDamage);
            }

            return stringBuilder.Length <= 0 ? string.Empty : stringBuilder.ToString();
        }

        public override bool AllowStaticRegistration()
        {
            return true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "AIWantUseWeapon");
            Object.RegisterPartEvent(this, "AmmoHasPart");
            Object.RegisterPartEvent(this, "CheckLoadAmmo");
            Object.RegisterPartEvent(this, "CheckReadyToFire");
            Object.RegisterPartEvent(this, "CommandReloadWhileEquipped");
            Object.RegisterPartEvent(this, "GenerateIntegratedHostInitialAmmo");
            Object.RegisterPartEvent(this, "GetMissileWeaponStatus");
            Object.RegisterPartEvent(this, "GetNotReadyToFireMessage");
            Object.RegisterPartEvent(this, "LoadAmmo");
            Object.RegisterPartEvent(this, "NeedsReload");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            bool continueProcessing = false;
            switch (E.ID)
            {
                case "AIWantUseWeapon": continueProcessing = AIWantUseWeapon(E); break;
                case "AmmoHasPart": continueProcessing = AmmoHasPart(E); break;
                case "CheckLoadAmmo": continueProcessing = CheckLoadAmmo(E); break;
                case "CheckReadyToFire": continueProcessing = CheckReadyToFire(E); break;
                case "CommandReloadWhileEquipped": continueProcessing = CommandReloadWhileEquipped(E); break;
                case "GetMissileWeaponStatus": continueProcessing = GetMissileWeaponStatus(E); break;
                case "GetNotReadyToFireMessage": continueProcessing = GetNotReadyToFireMessage(E); break;
                case "LoadAmmo": continueProcessing = LoadAmmo(E); break;
                case "NeedsReload": continueProcessing = NeedsReload(E); break;
            }

            return continueProcessing;
        }

        private bool AIWantUseWeapon(Event E)
        {
            var user = E.GetGameObjectParameter("Object");
            var bitLocker = user?.GetPart<BitLocker>();
            return !IsDisabled() && bitLocker != null;
        }

        private bool AmmoHasPart(Event E)
        {
            GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(GetCurrentProjectileBlueprint());
            return blueprint == null || !blueprint.HasPart(E.GetStringParameter("Part"));
        }

        private bool CheckLoadAmmo(Event E)
        {
            GameObject loader = E.GetGameObjectParameter("Loader");
            if (CurrentLoadedBit == 0)
            {
                if (loader.IsPlayer())
                {
                    AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("have") + " no bit selected!", 'r');
                }
                return false;
            }

            if (GetCurrentAmmoCount() == 0)
            {
                if (loader.IsPlayer())
                {
                    AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("have") + " no more ammo!", 'r');
                }
                return false;
            }

            return true;
        }

        private bool CheckReadyToFire(Event E)
        {
            return !IsDisabled() && GetCurrentAmmoCount() > 0;
        }

        private bool CommandReloadWhileEquipped(Event E)
        {
            List<string> optionStrings = new List<string>();
            List<char> optionHotkeys = new List<char>();
            List<char> optionBits = new List<char>();
            int currentSelection = 0;
            int index = 0;
            foreach (var bit in BitType.BitTypes)
            {
                optionStrings.Add(GetOptionStringForMode(bit.Color));
                optionHotkeys.Add((char)('a' + index));
                optionBits.Add(bit.Color);
                if (bit.Color == CurrentLoadedBit)
                {
                    currentSelection = index;
                }

                index++;
            }

            optionStrings.Add("unload");
            optionHotkeys.Add('u');
            optionBits.Add((char)0);

            var selection = Popup.ShowOptionList(
                "Mode Select:",
                optionStrings.ToArray(), optionHotkeys.ToArray(), 0,
                $"Select which bits to use in {ParentObject.the}{ParentObject.ShortDisplayName}:",
                60, false, true, currentSelection);
            if (selection >= 0 && selection < optionBits.Count)
            {
                var newMode = optionBits[selection];
                var requiredSkill = GetRequiredSkillForMode(newMode);
                var hasSkill = ParentObject.Equipped.HasSkill(requiredSkill);
                if (newMode != 0 && !hasSkill)
                {
                    Popup.Show($"You do not have the required skill to use that mode!\n\n" +
                        $"Mode: {BitType.GetString(newMode)}\n" +
                        $"Required Skill: {{{{C|{SkillFactory.GetSkillOrPowerName(requiredSkill)}}}}}");
                    ParentObject.FireEvent(E);
                }
                else
                {
                    var reloaded = SetCurrentBit(optionBits[selection]);
                    if (reloaded && ReloadEnergy > 0)
                    {
                        ParentObject.Equipped.UseEnergy(ReloadEnergy, "BitGun Reload " + CurrentLoadedBit);
                    }
                }
            }

            return true;
        }

        private string GetOptionStringForMode(char mode)
        {
            var requiredSkill = GetRequiredSkillForMode(mode);
            var hasSkill = ParentObject.Equipped.HasSkill(requiredSkill);
            if (hasSkill)
            {
                var currentAmmo = GetAmmoCount(mode);
                var ammoDisplay = currentAmmo > 0 ? currentAmmo.ToString() : "{{K|empty}}";
                return $"{BitType.GetString(mode)} - {{{{{mode}|{GetModeName(mode)}}}}} {GetWeaponPerformanceString(mode)} ({ammoDisplay})";
            }
            else
            {
                return $"{BitType.GetString(mode)} - {{{{K|Requires:}}}} {{{{c|{SkillFactory.GetSkillOrPowerName(requiredSkill)}}}}}";
            }
        }

        private bool GetMissileWeaponStatus(Event E)
        {
            if (!E.HasParameter("Override"))
            {
                StringBuilder parameter = E.GetParameter<StringBuilder>("Items");
                parameter.Append(" ").Append(GetCurrentAmmoStatusDisplay(true));
            }

            return true;
        }

        private bool GetNotReadyToFireMessage(Event E)
        {
            if (CurrentLoadedBit == 0)
            {
                E.SetParameter("Message", $"{ParentObject.The}{ParentObject.DisplayNameOnly} merely {ParentObject.GetVerb("click", false)}.");
            }

            if (GetCurrentAmmoCount() == 0)
            {
                E.SetParameter("Message", $"Out of ammo! Select a different mode.");
            }
            return true;
        }

        private bool LoadAmmo(Event E)
        {
            var currentModeUsesProjectile = CurrentModeUsesProjectile();
            var currentProjectileBlueprint = GetCurrentProjectileBlueprint();
            if (currentModeUsesProjectile && string.IsNullOrEmpty(currentProjectileBlueprint))
            {
                E.SetParameter("Ammo", null);
                return false;
            }

            if (GetCurrentAmmoCount() == 0)
            {
                E.SetParameter("Ammo", null);
                return false;
            }

            var bitLocker = ParentObject.ThePlayer?.GetPart<BitLocker>();
            bitLocker.UseBits(CurrentLoadedBit.ToString());

            E.SetParameter("Ammo", currentModeUsesProjectile ? GameObject.create(currentProjectileBlueprint) : null);
            return true;
        }

        private bool NeedsReload(Event E)
        {
            return E.GetParameter("Skip") == this;
        }

        [WishCommand(Command = "bitgun")]
        public static bool Wish()
        {
            ThePlayer.RequirePart<BitLocker>().AddAllBits(20);
            var bitgun = ThePlayer.RequirePart<Inventory>().AddObject("Randy_TinkersFriend", true);
            ThePlayer.ForceEquipObject(bitgun, "Right Missile Weapon");
            return true;
        }
    }
}