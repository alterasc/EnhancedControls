using EnhancedControls.Common;
using EnhancedControls.Settings;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;

namespace EnhancedControls.KeyboardBindings;

public class PrevCharacter : ModHotkeySettingEntry
{
    private const string _key = "prevcharacter";
    private const string _title = "Previous Character";
    private const string _tooltip = "Moves selection to the previous character";
    private const string _defaultValue = "#Tab;;WorldFullscreenUI;false";
    private const string BIND_NAME = $"{PREFIX}.newcontrols.ui.{_key}";

    public PrevCharacter() : base(_key, _title, _tooltip, _defaultValue) { }

    public override SettingStatus TryEnable() => TryEnableAndPatch(typeof(Patches));

    [HarmonyPatch]
    private static class Patches
    {
        /// <summary>
        /// Binds key after other service window keys are bound
        /// </summary>
        [HarmonyPatch(typeof(ServiceWindowsVM), nameof(ServiceWindowsVM.BindKeys))]
        [HarmonyPostfix]
        private static void Add(ServiceWindowsVM __instance)
        {
            __instance.AddDisposable(Game.Instance.Keyboard.Bind(BIND_NAME, SelectPrevCharacter));
        }

        private static void SelectPrevCharacter()
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
                CharacterSelector.SelectPrevCharacter();
            }
        }
    }
}