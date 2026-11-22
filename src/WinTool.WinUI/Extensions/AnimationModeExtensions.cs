using Windows.UI.ViewManagement;
using WinTool.WinUI.Native;
using WinTool.WinUI.Options;

namespace WinTool.WinUI.Extensions;

public static class AnimationModeExtensions
{
    private const int SM_REMOTESESSION = 0x1000;

    private static readonly UISettings s_uiSettings = new();
    private static readonly NativeMethods.EffectivePowerModeCallback _powerModeCallback = OnEffectivePowerModeChanged;

    private static volatile EffectivePowerMode _effectivePowerMode = EffectivePowerMode.Balanced;

    static AnimationModeExtensions()
    {
        NativeMethods.PowerRegisterForEffectivePowerModeNotifications(1, _powerModeCallback, 0, out _);
    }

    extension(AnimationMode mode)
    {
        public bool ShouldAnimate => mode switch
        {
            AnimationMode.On => true,
            AnimationMode.Off => false,
            _ => NativeMethods.GetSystemMetrics(SM_REMOTESESSION) == 0
                 && s_uiSettings.AnimationsEnabled
                 && !IsEnergySaverOn()
        };
    }

    private static void OnEffectivePowerModeChanged(EffectivePowerMode mode, nint context)
    {
        _effectivePowerMode = mode;
    }

    private static bool IsEnergySaverOn() => _effectivePowerMode is EffectivePowerMode.BatterySaver or EffectivePowerMode.BetterBattery;
}
