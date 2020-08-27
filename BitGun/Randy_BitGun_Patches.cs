using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using XRL.Core;
using XRL.World;
using XRL.World.Parts;

namespace BitGun
{
    [HarmonyPatch(typeof(MissileWeapon))]
    [HarmonyPatch("FireEvent")]
    public class Randy_BitGun_MissileWeapon_FireEvent_Patch
    {
        private static readonly MethodInfo GameObject_GetTagOrProperty = typeof(GameObject).GetMethod("GetTagOrStringProperty", BindingFlags.Instance | BindingFlags.Public);

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int targetInstructionIndex = -1;
            List<CodeInstruction> codeInstructions = instructions.ToList();
            for (int index = 0; index < codeInstructions.Count; index++)
            {
                var instruction = codeInstructions[index];
                if (instruction.opcode == OpCodes.Ldstr && ((string)instruction.operand == "MissileFireSound"))
                {
                    targetInstructionIndex = index + 2;
                }

                if (index == targetInstructionIndex)
                {
                    instruction.operand = GameObject_GetTagOrProperty;
                    break;
                }
            }

            return codeInstructions;
        }
    }
}