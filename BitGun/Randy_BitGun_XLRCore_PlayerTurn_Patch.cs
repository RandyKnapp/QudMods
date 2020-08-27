using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using XRL.Core;

namespace BitGun
{
    [HarmonyPatch(typeof(XRLCore))]
    [HarmonyPatch("PlayerTurn")]
    public class Randy_BitGun_XLRCore_PlayerTurn_Patch
    {
        private static readonly MethodInfo PatchMethod = typeof(Randy_PlayerTurn_CmdFire_Patch).GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);

        private enum State { LookingForBody, LookingForBreak, InsertingCode, Complete };

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            State state = State.LookingForBody;
            object jumpTarget = null;
            foreach (var instruction in instructions)
            {
                if (state == State.LookingForBody)
                {
                    if (instruction.opcode == OpCodes.Ldstr && ((string)instruction.operand == "Body"))
                    {
                        XRLCore.Log("BitGun Patch Found Body");
                        state = State.LookingForBreak;
                    }
                }
                else if (state == State.LookingForBreak)
                {
                    if (instruction.opcode == OpCodes.Brfalse)
                    {
                        XRLCore.Log("BitGun Patch Found Break");
                        jumpTarget = instruction.operand;
                        state = State.InsertingCode;
                    }
                }
                else if (state == State.InsertingCode)
                {
                    XRLCore.Log("BitGun Patch Inserting Code");
                    yield return new CodeInstruction(OpCodes.Call, PatchMethod);
                    yield return new CodeInstruction(OpCodes.Br, jumpTarget);
                    state = State.Complete;
                }

                yield return instruction;
            }

            if (state != State.Complete)
            {
                XRLCore.Log("BitGun Patch Error");
            }
        }
    }
}