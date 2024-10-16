using Code.Visual.Animation;
using HarmonyLib;
using Kingmaker;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;

namespace EnhancedControls.Features;

public static class MovementManager
{
    public static bool IsToggleEnabled = false;

    public static bool IsShiftClickToMoveEnabled = false;

    private static WalkSpeedToggle MovementStyle = WalkSpeedToggle.NoOverride;

    public static void ToggleWalk()
    {
        if (Game.Instance.Player.IsInCombat) return;

        MovementStyle = (WalkSpeedToggle)(((int)MovementStyle + 1) % 4);

        if (MovementStyle != WalkSpeedToggle.NoOverride)
        {
            var units = Game.Instance.Player.Party;
            foreach (var unit in units)
            {
                var cmd = unit.Commands.Current;
                if (cmd is UnitMoveTo unitMoveTo)
                {
                    unitMoveTo.Params.MovementType = FromToggle(MovementStyle);
                }
            }
        }
    }

    private static WalkSpeedType FromToggle(WalkSpeedToggle toggle)
    {
        return toggle switch
        {
            WalkSpeedToggle.ForceWalk => WalkSpeedType.Walk,
            WalkSpeedToggle.ForceRun => WalkSpeedType.Run,
            WalkSpeedToggle.ForceSprint => WalkSpeedType.Sprint,
            _ => WalkSpeedType.Sprint
        };
    }

    private enum WalkSpeedToggle
    {
        NoOverride = 0,
        ForceWalk = 1,
        ForceRun = 2,
        ForceSprint = 3,
    }

    [HarmonyPatch]
    public static class MovementPatches
    {
        /// <summary>
        /// Allows override of movement type in real-time
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(UnitHelper), nameof(UnitHelper.WalkSpeedTypeByDistanceRT))]
        [HarmonyPostfix]
        public static WalkSpeedType OverrideMovementTypeInRealTime(WalkSpeedType result)
        {
            if (IsShiftClickToMoveEnabled && KeyboardAccess.IsShiftHold())
            {
                return WalkSpeedType.Walk;
            }
            if (IsToggleEnabled && MovementStyle != WalkSpeedToggle.NoOverride)
            {
                return FromToggle(MovementStyle);
            }
            return result;
        }
    }
}
