using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace BitGun
{
    public static class Randy_PlayerTurn_CmdFire_Patch
    {
        public static void Execute()
        {
            //XRLCore.Log("BitGun Executing Patch");

            Body playerBody = XRLCore.Core.Game.Player.Body.GetPart("Body") as Body;

            bool needToReload = false;
            string needToReloadMessage = null;

            Event wantsFireEvent = Event.New("WantsFireEvent", "WantsEvent", new List<GameObject>());
            playerBody.FireEventOnBodyparts(wantsFireEvent);
            var objectsToFire = wantsFireEvent.GetParameter("WantsEvent") as List<GameObject>;
            foreach (var obj in objectsToFire)
            {
                if (!obj.FireEvent(Event.New("CheckReadyToFire")))
                {
                    needToReload = true;

                    Event E = Event.New("GetNotReadyToFireMessage");
                    obj.FireEvent(E);
                    needToReloadMessage = E.GetStringParameter("Message");
                    break;
                }
            }

            bool cmdFireWeaponEquipped = objectsToFire.Count > 0;
            bool missileWeaponEquipped = false;

            List<GameObject> missileWeaponList = Event.NewGameObjectList();
            playerBody.GetMissileWeapons(missileWeaponList);

            int index = 0;
            for (int count = missileWeaponList.Count; index < count; ++index)
            {
                if (missileWeaponList[index].GetPart("MissileWeapon") is MissileWeapon missileWeapon)
                {
                    missileWeaponEquipped = true;
                    if (missileWeapon.ReadyToFire())
                    {
                        needToReload = true;
                        break;
                    }

                    if (needToReloadMessage == null)
                    {
                        needToReloadMessage = missileWeapon.GetNotReadyToFireMessage();
                    }
                }
            }

            /*var log = $"BitGun Patch cmdFireWeaponEquipped:{cmdFireWeaponEquipped} missileWeaponEquipped:{missileWeaponEquipped}";
            log += "\nCommand Objects";
            foreach (var obj in objectsToFire)
            {
                log += $"\n  - {obj.DisplayName}";
            }
            log += "\nMissile Wepaons:";
            foreach (var missileWeapon in missileWeaponList)
            {
                log += $"\n  - {missileWeapon.DisplayName}";
            }
            XRLCore.Log(log);*/

            if (!cmdFireWeaponEquipped && !missileWeaponEquipped)
            {
                Popup.Show("You do not have a missile weapon equipped!");
                return;
            }

            if (!needToReload)
            {
                Popup.Show(needToReloadMessage ?? "You need to reload!");
                return;
            }

            if (cmdFireWeaponEquipped)
            {
                foreach (var obj in objectsToFire)
                {
                    obj.FireEvent(Event.New("CommandFireMissileWeapon"));
                }
            }
            else
            {
                XRLCore.Core.Game.Player.Body.FireEvent(Event.New("CommandFireMissileWeapon"));
            }
        }
    }
}