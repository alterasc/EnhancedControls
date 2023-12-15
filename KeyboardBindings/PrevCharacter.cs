using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Sound;
using System;
using System.Collections.Generic;

namespace EnhancedControls.KeyboardBindings;

internal static class PrevCharacter
{
    private const string BIND_NAME = "EnhancedControls.PrevCharacter";

    internal static void RegisterBinding(KeyBindingData keyData)
    {
        Game.Instance.Keyboard.RegisterBinding(
            BIND_NAME,
            keyData.Key,
            new GameModeType[] { GameModeType.Default, GameModeType.Pause, GameModeType.StarSystem },
            keyData.IsCtrlDown,
            keyData.IsAltDown,
            keyData.IsShiftDown);
    }

    internal static IDisposable Bind()
    {
        return Game.Instance.Keyboard.Bind(BIND_NAME, delegate
            {
                var uiContext = Game.Instance.RootUiContext;
                var currentWindow = Game.Instance.RootUiContext.CurrentServiceWindow;
                if (currentWindow == ServiceWindowsType.CharacterInfo)
                {
                    var serviceWindowsVM = uiContext.IsSpace ? uiContext.SpaceVM.StaticPartVM.ServiceWindowsVM : uiContext.SurfaceVM.StaticPartVM.ServiceWindowsVM;
                    var characterInfoVM = serviceWindowsVM.CharacterInfoVM.Value;
                    var nameAndPortraitVM = (CharInfoNameAndPortraitVM)characterInfoVM.ComponentVMs[CharInfoComponentType.NameAndPortrait].Value;
                    nameAndPortraitVM.SelectPrevCharacter();
                }
                else if (currentWindow == ServiceWindowsType.Inventory)
                {
                    var serviceWindowsVM = uiContext.IsSpace ? uiContext.SpaceVM.StaticPartVM.ServiceWindowsVM : uiContext.SurfaceVM.StaticPartVM.ServiceWindowsVM;
                    var inventoryVM = serviceWindowsVM.InventoryVM.Value;

                    var nameAndPortraitVM = inventoryVM.NameAndPortraitVM;
                    nameAndPortraitVM.SelectPrevCharacter();
                }
                else if (uiContext.m_FullScreenUIType == Kingmaker.UI.Models.FullScreenUIType.Unknown)
                {
                    List<BaseUnitEntity> actualGroup = Game.Instance.SelectionCharacter.ActualGroup;
                    var curUnit = Game.Instance.SelectionCharacter.SelectedUnit;
                    int num = (actualGroup.IndexOf(curUnit.Value) - 1) % actualGroup.Count;
                    if (num < 0)
                    {
                        num += actualGroup.Count;
                    }
                    Game.Instance.SelectionCharacter.SetSelected(actualGroup[num], false, false);
                    if (actualGroup.Count == 1)
                    {
                        UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play(null);
                    }
                }
            });
    }

    [HarmonyPatch(typeof(ServiceWindowsVM), nameof(ServiceWindowsVM.BindKeys))]
    public static class Patches
    {
        [HarmonyPostfix]
        public static void Add(ServiceWindowsVM __instance)
        {
            __instance.AddDisposable(Bind());
        }
    }
}