using EnhancedControls.Settings;
using System;

namespace EnhancedControls.Features;

public class ShiftClickToWalk : ModToggleSettingEntry
{
    private const string _key = "shiftclicktowalk";
    private const string _title = "Shift + Click to force walk";
    private const string _tooltip = "Makes Shift + Click send character walking to destination instead of walk/run/sprint being determined based on distance. Ignores walk toggle state.";
    private const bool _defaultValue = false;

    protected override SettingStatus TryEnableAndPatch(params Type[] type)
    {
        return base.TryEnableAndPatch(type);
    }

    public override SettingStatus TryEnable()
    {
        var currentValue = SettingEntity.GetValue();
        MovementManager.IsShiftClickToMoveEnabled = currentValue;
        return SettingStatus.WORKING;
    }

    public ShiftClickToWalk() : base(_key, _title, _tooltip, _defaultValue) { }
}
