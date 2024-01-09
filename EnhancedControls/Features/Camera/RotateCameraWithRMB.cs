using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UI.Selection;
using Kingmaker.View;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace EnhancedControls.Features.Camera;

public class RotateCameraWithRMB : ModToggleSettingEntry
{
    private const string _key = "rotatecamerawithrmb";
    private const string _title = "Rotate camera with right mouse button (Experimental)";
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

        //[HarmonyPatch(typeof(PointerController), nameof(PointerController.Tick))]
        //[HarmonyPrefix]
        public static bool TickReplacer(PointerController __instance)
        {
            __instance.TickPointerDebug();
            if (PointerController.DebugThisFrame)
            {
                PointerController.DebugThisFrame = false;
            }
            bool isControllerGamepad = Game.Instance.IsControllerGamepad;
            bool flag = Game.Instance.IsControllerMouse || GamePad.Instance.CursorEnabled || TurnController.IsInTurnBasedCombat();
            Vector2 pointerPosition = PointerController.PointerPosition;
            Vector3 zero = Vector3.zero;
            GameObject gameObject = null;
            IClickEventHandler clickEventHandler = null;
            InteractionHighlightController instance = InteractionHighlightController.Instance;
            if (instance != null && instance.IsHighlighting && TurnController.IsInTurnBasedCombat())
            {
                return false;
            }
            if (!PointerController.InGui)
            {
                __instance.SelectClickObject(pointerPosition, out gameObject, out zero, out clickEventHandler);
                __instance.m_SimulateClickHandler = clickEventHandler;
                __instance.m_WorldPositionForSimulation = __instance.WorldPosition;
                if (gameObject != null)
                {
                    __instance.WorldPosition = zero;
                }
            }
            if (!(isControllerGamepad ? ((__instance.m_MouseDownButton == 0) ? __instance.GamePadConfirm : __instance.GamePadDecline) : Input.GetMouseButton(__instance.m_MouseDownButton)) && __instance.m_MouseDown)
            {
                __instance.m_MouseDown = false;
                if (__instance.m_MouseDrag && __instance.m_DragFrames < 2)
                {
                    __instance.m_MouseDrag = false;
                    if (UIAccess.MultiSelection)
                    {
                        UIAccess.MultiSelection.Cancel();
                    }
                }
                if (__instance.m_MouseDownButton == 1 && __instance.Mode != PointerMode.Default)
                {
                    __instance.ClearPointerMode();
                }
                else if (__instance.m_MouseDrag && __instance.Mode == PointerMode.Default)
                {
                    if (__instance.m_MouseDownButton == 0)
                    {
                        if (UIAccess.MultiSelection)
                        {
                            UIAccess.MultiSelection.SelectEntities();
                        }
                    }
                    //else
                    //{
                    //    Main.log.Log("Before IDragClickEventHandler");
                    //    IDragClickEventHandler dragClickEventHandler = __instance.m_MouseDownHandler as IDragClickEventHandler;
                    //    if (dragClickEventHandler != null && __instance.m_MouseDownOn != null && dragClickEventHandler.OnClick(__instance.m_MouseDownOn, __instance.m_MouseDownWorldPosition, zero))
                    //    {
                    //        EventBus.RaiseEvent<IClickMarkHandler>(delegate (IClickMarkHandler h)
                    //        {
                    //            h.OnClickHandled(__instance.m_MouseDownWorldPosition);
                    //        }, true);
                    //    }
                    //}
                }
                else if (flag && __instance.m_MouseDownHandler != null && __instance.m_MouseDownOn != null && __instance.m_MouseDownHandler.OnClick(__instance.m_MouseDownOn, __instance.m_MouseDownWorldPosition, __instance.m_MouseDownButton, false, false))
                {
                    EventBus.RaiseEvent<IClickMarkHandler>(delegate (IClickMarkHandler h)
                    {
                        h.OnClickHandled(__instance.m_MouseDownWorldPosition);
                    }, true);

                }
                __instance.m_MouseDownOn = null;
                __instance.m_MouseDrag = false;
            }
            if (__instance.PointerOn != gameObject)
            {
                __instance.OnHoverChanged(__instance.PointerOn, (!__instance.IgnoreUnitsColliders) ? gameObject : null);
                __instance.PointerOn = gameObject;
            }
            if (!isControllerGamepad && __instance.m_MouseDown && Vector2.Distance(__instance.m_MouseDownCoord, pointerPosition) > 4f && !__instance.m_MouseDrag && __instance.Mode == PointerMode.Default)
            {
                __instance.m_MouseDrag = true;
                __instance.m_DragFrames = 0;
                if (__instance.m_MouseDownButton == 0)
                {
                    if (UIAccess.MultiSelection && UIAccess.MultiSelection.ShouldMultiSelect)
                    {
                        UIAccess.MultiSelection.CreateBoxSelection(__instance.m_MouseDownCoord);
                    }
                }
                //else
                //{
                //    IDragClickEventHandler dragClickEventHandler2 = __instance.m_MouseDownHandler as IDragClickEventHandler;
                //    if (dragClickEventHandler2 != null)
                //    {
                //        dragClickEventHandler2.OnStartDrag(__instance.m_MouseDownOn, __instance.m_MouseDownWorldPosition);
                //    }
                //}
            }
            if (__instance.m_MouseDrag && Time.unscaledTime - __instance.m_MouseButtonTime >= 0.07f)
            {
                if (__instance.m_MouseDownButton == 0 && UIAccess.MultiSelection)
                {
                    UIAccess.MultiSelection.DragBoxSelection();
                }
                __instance.m_DragFrames++;
            }
            if (!__instance.m_MouseDown && (isControllerGamepad ? (__instance.GamePadConfirm || __instance.GamePadDecline) : ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !PointerController.InGui)))
            {
                __instance.m_MouseDownButton = (isControllerGamepad ? (__instance.GamePadConfirm ? 0 : 1) : (Input.GetMouseButtonDown(0) ? 0 : 1));
                __instance.m_MouseDown = true;
                __instance.m_MouseDownOn = gameObject;
                __instance.m_MouseDownHandler = clickEventHandler;
                __instance.m_MouseDownCoord = pointerPosition;
                __instance.m_MouseDownWorldPosition = __instance.WorldPosition;
                __instance.m_MouseButtonTime = Time.unscaledTime;
            }
            //if (!isControllerGamepad && __instance.m_MouseDown && __instance.m_MouseDownButton == 1 && !TurnController.IsInTurnBasedCombat())
            //{
            //    Main.log.Log("Before IDragClickEventHandler3");
            //    IDragClickEventHandler dragClickEventHandler3 = __instance.m_MouseDownHandler as IDragClickEventHandler;
            //    if (dragClickEventHandler3 != null && __instance.m_MouseDownOn != null)
            //    {
            //        dragClickEventHandler3.OnDrag(__instance.m_MouseDownOn, __instance.m_MouseDownWorldPosition, zero);
            //    }
            //}
            if (!__instance.m_MouseDown)
            {
                MultiplySelection multiSelection = UIAccess.MultiSelection;
                if (multiSelection == null)
                {
                    return false;
                }
                multiSelection.Cancel();
            }
            return false;
        }
    }
}
