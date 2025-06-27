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
        /// Removes all drag to move functionality
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(PointerController), nameof(PointerController.Tick))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RemoveRightClickMovementAfterDrag(IEnumerable<CodeInstruction> instructions)
        {
            /*  There are three times it's used. All look the same 
             *  
             *      IDragClickEventHandler dragClickEventHandler = this.m_MouseDownHandler as IDragClickEventHandler;
			 *      if (dragClickEventHandler != null ...
             *
             *  Null check is there in all three places.
             *  So I discard original value and put null into dragClickEventHandler variable
             *  
             *  Should null check disappear, things will break with NRE.
             *  Let's hope it doesn't happen
             */
            var codeMatcher = new CodeMatcher(instructions);

            for (var i = 0; i < 3; i++)
            {
                codeMatcher.MatchStartForward(
                        new CodeMatch(new CodeInstruction(OpCodes.Ldarg_0)),
                        new CodeMatch(CodeInstruction.LoadField(typeof(PointerController), nameof(PointerController.m_MouseDownHandler))),
                        new CodeMatch(new CodeInstruction(OpCodes.Isinst, typeof(IDragClickEventHandler))),
                        new CodeMatch(new CodeInstruction(OpCodes.Stloc_S))
                    )
                    .Advance(3)
                    .Insert(
                        new CodeInstruction(OpCodes.Pop),
                        new CodeInstruction(OpCodes.Ldnull)
                    );
            }
            Main.log.Log("Removed PointerController drag functionality.");
            return codeMatcher.Instructions();
        }
    }
}
