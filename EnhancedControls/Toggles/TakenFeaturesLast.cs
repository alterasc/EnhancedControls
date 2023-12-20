using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Owlcat.Runtime.UI.MVVM;
using System.Collections.Generic;
using System.Linq;

namespace EnhancedControls.Toggles;

public class TakenFeaturesLast : ModToggleSettingEntry
{
    private const string _key = "takenfeatureslast";
    private const string _title = "Place taken talents last";
    private const string _tooltip = "Places already taken talents/abilities/etc at the bottom of selection list";
    private const bool _defaultValue = true;

    public TakenFeaturesLast() : base(_key, _title, _tooltip, _defaultValue) { }

    public override SettingStatus TryEnable() => TryEnableAndPatch(typeof(Patches));

    [HarmonyPatch]
    private class Patches
    {
        /// <summary>
        /// Removes normal setting of FilteredGroupList
        /// and replaces it with method above that sorts output by 
        /// whether feature is already taken to the max
        /// 
        /// This results in UI placing already taken features at the bottom
        /// </summary>
        [HarmonyPatch(typeof(RankEntrySelectionVM), nameof(RankEntrySelectionVM.HandleFilterChange))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RemoveEndTurnFromPauseButton(IEnumerable<CodeInstruction> instructions)
        {
            var newInstructions = new List<CodeInstruction>(instructions);
            var loadsShowGroupList = newInstructions.FindLastIndex(x => x.LoadsField(AccessTools.Field(typeof(RankEntrySelectionVM), nameof(RankEntrySelectionVM.m_ShowGroupList))));
            var storesFilteredGroupList = newInstructions.FindIndex(x => x.StoresField(AccessTools.Field(typeof(RankEntrySelectionVM), nameof(RankEntrySelectionVM.FilteredGroupList))));
            if (loadsShowGroupList != -1 && storesFilteredGroupList != -1 && loadsShowGroupList < storesFilteredGroupList)
            {
                newInstructions.RemoveRange(loadsShowGroupList, storesFilteredGroupList - loadsShowGroupList);
                var call = CodeInstruction.Call(typeof(Patches), nameof(Patches.SetOrderedByList));
                newInstructions.Insert(loadsShowGroupList, call);
                newInstructions.Insert(loadsShowGroupList, new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1));
                return newInstructions;
            }
            return instructions;
        }

        private static List<VirtualListElementVMBase> SetOrderedByList(RankEntrySelectionVM vm, FeaturesFilter.FeatureFilterType? filter)
        {
            return vm.m_ShowGroupList
                .SelectMany((gr) => gr.GetFiltered(filter))
                .OrderBy(x =>
                {
                    if (x is RankEntrySelectionFeatureVM featureVm)
                    {
                        if (featureVm.UnitProgressionVM.Unit.Value.Progression.Features.GetRank(featureVm.Feature) >= featureVm.SelectionItem.MaxRank)
                        {
                            return 1;
                        }
                    }
                    return 0;
                })
                .ToList();
        }
    }
}
