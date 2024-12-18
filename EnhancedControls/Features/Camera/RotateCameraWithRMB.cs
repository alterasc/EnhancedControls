using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker.Controllers.Clicks;
using Kingmaker.View;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace EnhancedControls.Features.Camera;

public class RotateCameraWithRMB : ModToggleSettingEntry
{
    private const string _key = "rotatecamerawithrmb";
    private const string _title = "Rotate camera with right mouse button";
    private const string _tooltip = "Rotate camera with right mouse button instead of middle. You WILL lose some functionality normally done with RMB. See description.";
    private const bool _defaultValue = false;

    public RotateCameraWithRMB() : base(_key, _title, _tooltip, _defaultValue) { }

    [HarmonyPatch]
    private class Patches
    {
        /// <summary>
        /// Checks for Right Mouse Button instead of Middle Mouse Button
        /// </summary>
        [HarmonyPatch(typeof(CameraRig), nameof(CameraRig.RotateByMiddleButton))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RotateWithRMB(IEnumerable<CodeInstruction> instructions)
        {
            var newInstructions = new List<CodeInstruction>(instructions);
            var mouseButtonCall = newInstructions.FindIndex(x => x.Calls(AccessTools.Method(typeof(Input), nameof(Input.GetMouseButtonDown))));
            if (mouseButtonCall != -1)
            {
                var prev = newInstructions[mouseButtonCall - 1];
                if (prev.opcode == OpCodes.Ldc_I4_2)
                {
                    newInstructions[mouseButtonCall - 1] = new CodeInstruction(OpCodes.Ldc_I4_1);
                    return newInstructions;
                }
            }
            return instructions;
        }

        /// <summary>
        /// Checks for Right Mouse Button instead of Middle Mouse Button in this method too
        /// </summary>
        [HarmonyPatch(typeof(CameraRig), nameof(CameraRig.CheckRotate))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckRotateWithRMB(IEnumerable<CodeInstruction> instructions)
        {
            var newInstructions = new List<CodeInstruction>(instructions);
            var mouseButtonCall = newInstructions.FindIndex(x => x.Calls(AccessTools.Method(typeof(Input), nameof(Input.GetMouseButton))));
            if (mouseButtonCall != -1)
            {
                var prev = newInstructions[mouseButtonCall - 1];
                if (prev.opcode == OpCodes.Ldc_I4_2)
                {
                    newInstructions[mouseButtonCall - 1] = new CodeInstruction(OpCodes.Ldc_I4_1);
                    return newInstructions;
                }
            }
            return instructions;
        }

        /// <summary>
        /// Remove from PointerController "drag mouse while holding RMB to position party"
        /// </summary>
        [HarmonyPatch(typeof(PointerController), nameof(PointerController.Tick))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RemoveRMBFromPointerController(IEnumerable<CodeInstruction> instructions)
        {
            var newInstructions = new List<CodeInstruction>(instructions);

            var mouseDownButtonField = AccessTools.Field(typeof(PointerController), nameof(PointerController.m_MouseDownButton));

            //remove first else block
            var idx = newInstructions.FindIndex(x => x.LoadsField(mouseDownButtonField));
            while (idx != -1)
            {
                var next = newInstructions[idx + 1];
                if (next.Branches(out var labelTo))
                {
                    var jumpTo = newInstructions.FindIndex(idx + 1, x => x.labels.Contains((Label)labelTo) && x.opcode == OpCodes.Ldarg_0);
                    if (jumpTo != -1)
                    {
                        var beforeJump = newInstructions[jumpTo - 1];
                        if (beforeJump.Branches(out var uncondJump))
                        {
                            if (newInstructions[jumpTo + 1].LoadsField(AccessTools.Field(typeof(PointerController), nameof(PointerController.m_MouseDownHandler))))
                            {
                                newInstructions.Insert(jumpTo + 1, new CodeInstruction(newInstructions[jumpTo].opcode));
                                newInstructions.Insert(jumpTo + 1, new CodeInstruction(beforeJump));
                                newInstructions.Insert(jumpTo + 1, new CodeInstruction(OpCodes.Pop));
                                Main.log.Log($"PointerController.Tick first edit done, line {jumpTo + 1}");
                                break;
                            }
                        }
                    }
                }
                idx = newInstructions.FindIndex(idx + 1, x => x.LoadsField(mouseDownButtonField));
            }

            //remove second block
            idx = newInstructions.FindIndex(idx + 1, x => x.LoadsField(mouseDownButtonField));
            while (idx != -1)
            {
                var next = newInstructions[idx + 1];
                if (next.Branches(out var labelTo))
                {
                    var jumpTo = newInstructions.FindIndex(idx + 1, x => x.labels.Contains((Label)labelTo) && x.opcode == OpCodes.Ldarg_0);
                    if (jumpTo != -1)
                    {
                        var beforeJump = newInstructions[jumpTo - 1];
                        if (beforeJump.Branches(out var uncondJump))
                        {
                            if (newInstructions[jumpTo + 1].LoadsField(AccessTools.Field(typeof(PointerController), nameof(PointerController.m_MouseDownHandler))))
                            {
                                newInstructions.Insert(jumpTo + 1, new CodeInstruction(newInstructions[jumpTo].opcode));
                                newInstructions.Insert(jumpTo + 1, new CodeInstruction(beforeJump));
                                newInstructions.Insert(jumpTo + 1, new CodeInstruction(OpCodes.Pop));
                                Main.log.Log($"PointerController.Tick second edit done, line {jumpTo + 1}");
                                break;
                            }
                        }
                    }
                }
                idx = newInstructions.FindIndex(idx + 1, x => x.LoadsField(mouseDownButtonField));
            }

            //remove last if block
            idx = newInstructions.FindIndex(idx + 1, x => x.LoadsField(mouseDownButtonField));
            while (idx != -1)
            {
                var next = newInstructions[idx + 1];
                if (next.opcode == OpCodes.Ldc_I4_1)
                {
                    if (newInstructions[idx + 2].Branches(out var jumpTo))
                    {
                        newInstructions[idx + 1].opcode = OpCodes.Ldc_I4;
                        newInstructions[idx + 1].operand = -99;
                        Main.log.Log($"PointerController.Tick third edit done, line {idx + 1}");
                        break;
                    }
                }
                idx = newInstructions.FindIndex(idx + 1, x => x.LoadsField(mouseDownButtonField));
            }

            return newInstructions;
        }
    }
}
