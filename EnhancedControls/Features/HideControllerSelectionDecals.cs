using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker.UI.Selection.UnitMark;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EnhancedControls.Features;

public class HideControllerSelectionDecals : ModToggleSettingEntry
{
    private const string _key = "hidecontrollerselectiondecals";
    private const string _title = "Hide controller selection circles around characters during exploration";
    private const string _tooltip = "Hides controller circles around characters during exploration.";
    private const bool _defaultValue = false;

    public HideControllerSelectionDecals() : base(_key, _title, _tooltip, _defaultValue) { }

    [HarmonyPatch]
    private static class Patches
    {
        /// <summary>
        /// Always sets exploration selected decal .Active to false
        /// </summary>
        /// <returns></returns>
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CharacterUnitMark), nameof(CharacterUnitMark.HandleStateChanged))]
        private static IEnumerable<CodeInstruction> ReplaceHighlightCheckForUnit(IEnumerable<CodeInstruction> instructions)
        {
            var mGamepadSelectedField = AccessTools.Field(typeof(CharacterUnitMark), nameof(CharacterUnitMark.m_GamepadSelectedDecal));
            var setActiveMethod = AccessTools.Method(typeof(UnitMarkDecal), nameof(UnitMarkDecal.SetActive));
            var skipping = false;
            foreach (var instruction in instructions)
            {
                if (skipping)
                {
                    if (!instruction.Calls(setActiveMethod))
                    {
                        continue;
                    }
                    else
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return instruction;
                        skipping = false;
                    }
                }
                else
                {
                    yield return instruction;
                    if (instruction.LoadsField(mGamepadSelectedField))
                    {
                        skipping = true;
                    }
                }
            }
        }
    }
}
