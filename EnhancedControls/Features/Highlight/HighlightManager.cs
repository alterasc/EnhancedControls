using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.Utility.GameConst;
using Kingmaker.View.Mechanics.Entities;

namespace EnhancedControls.Features.Highlight;
public static class HighlightManager
{
    public static bool IsBasicHighlightToggledOn = false;
    private static bool isBasicHighlightSuppressed = false;
    private static bool isFullHighlightOn = false;

    public static bool PartialHighlightNPCs = false;
    private static bool BasicHiglightActive => IsBasicHighlightToggledOn && !isBasicHighlightSuppressed && Game.Instance.ControllerMode == Game.ControllerModeType.Mouse;

    public static bool UnitHighlight(InteractionHighlightController _, AbstractUnitEntityView view)
    {
        return isFullHighlightOn || BasicHiglightActive && view.EntityData.IsDeadAndHasLoot;
    }

    public static void FullHighlightOn()
    {
        isFullHighlightOn = true;
        UpdateHighlight();
    }

    public static void FullHighlightOff()
    {
        isFullHighlightOn = false;
        UpdateHighlight(true);
    }

    /// <summary>
    /// Enable highlight at level
    /// 1 - map objects + bodies with loot
    /// 2 - everything
    /// </summary>
    /// <param name="level"></param>
    public static void UpdateHighlight(bool fullToggleOff = false)
    {
        var baseGameController = InteractionHighlightController.Instance;
        if (baseGameController == null || baseGameController.m_Inactive)
        {
            return;
        }
        if (!isFullHighlightOn && !BasicHiglightActive)
        {
            if (baseGameController.m_IsHighlighting)
            {
                HighlightOff();
            }
            return;
        }
        // disable highlight in combat unless it's full highlight
        if (Game.Instance.Player.IsInCombat && !isFullHighlightOn)
        {
            HighlightOff();
            return;
        }
        if (fullToggleOff)
        {
            // resetting highlighting to hide all objects previously highlighted by full highlight
            HighlightOff();
        }
        baseGameController.m_IsHighlighting = true;
        foreach (MapObjectEntity mapObjectEntity in Game.Instance.State.MapObjects)
        {
            if (mapObjectEntity.View != null)
            {
                mapObjectEntity.View.UpdateHighlight();
            }
        }
        foreach (AbstractUnitEntity abstractUnitEntity in Game.Instance.State.AllUnits)
        {
            if (isFullHighlightOn || abstractUnitEntity.IsDeadAndHasLoot)
            {
                if (abstractUnitEntity.View != null)
                {
                    abstractUnitEntity.View.UpdateHighlight(false);
                }
            }
        }
        EventBus.RaiseEvent(delegate (IInteractionHighlightUIHandler h)
        {
            if (BasicHiglightActive && !isFullHighlightOn && h is UnitState us)
            {
                if (PartialHighlightNPCs && !us.Unit.IsDeadOrUnconscious && !us.Unit.IsPlayerFaction)
                {
                    h.HandleHighlightChange(true);
                }
            }
            else
            {
                h.HandleHighlightChange(true);
            }
        }, true);

    }

    /// <summary>
    /// Disables all highlighting
    /// Replacer method for <see cref="InteractionHighlightController.HighlightOff"/> 
    /// because original can throw NRE
    /// TODO: remove when Owlcat fixes this
    /// 
    /// </summary>
    public static void HighlightOff()
    {
        var baseGameController = InteractionHighlightController.Instance;
        if (baseGameController == null)
        {
            return;
        }
        if (baseGameController.m_IsHighlighting && !baseGameController.m_Inactive)
        {
            baseGameController.m_IsHighlighting = false;
            foreach (MapObjectEntity mapObjectEntity in Game.Instance.State.MapObjects)
            {
                if (mapObjectEntity.View != null)
                {
                    mapObjectEntity.View.UpdateHighlight();
                }
            }
            foreach (AbstractUnitEntity abstractUnitEntity in Game.Instance.State.AllUnits)
            {
                if (abstractUnitEntity.View != null)
                {
                    abstractUnitEntity.View.UpdateHighlight(false);
                }
            }
            EventBus.RaiseEvent(delegate (IInteractionHighlightUIHandler h)
            {
                h.HandleHighlightChange(false);
            }, true);
        }
    }

    public static void OnEnableController(InteractionHighlightController instance)
    {
        Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOn, FullHighlightOn);
        Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOff, FullHighlightOff);
        Game.Instance.Keyboard.Bind(HighlightToggle.BIND_NAME, TogglePassiveHighlight);
        InteractionHighlightController.Instance = instance;
        instance.m_Inactive = false;
        if (BasicHiglightActive)
        {
            UpdateHighlight();
        }
    }

    public static void OnDisableController(InteractionHighlightController instance)
    {
        Game.Instance.Keyboard.Unbind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOn, FullHighlightOn);
        Game.Instance.Keyboard.Unbind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOff, FullHighlightOff);
        Game.Instance.Keyboard.Unbind(HighlightToggle.BIND_NAME, TogglePassiveHighlight);
        HighlightOff();
        instance.m_Inactive = true;
        InteractionHighlightController.Instance = null;
    }

    public static void TogglePassiveHighlight()
    {
        IsBasicHighlightToggledOn = !IsBasicHighlightToggledOn;
        if (Game.Instance.Player.IsInCombat) return;
        UpdateHighlight();
    }

    public static void SuppressPassiveHighlight()
    {
        isBasicHighlightSuppressed = true;
        UpdateHighlight();
    }

    public static void RestorePassiveHighlight()
    {
        isBasicHighlightSuppressed = false;
        UpdateHighlight();
    }
}
