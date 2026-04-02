using System.Windows;
using WinTool.Native;
using WinTool.ViewModels.Settings;

namespace WinTool.Extensions;

public static class AnimationModeExtensions
{
    private static volatile EffectivePowerMode _effectivePowerMode = EffectivePowerMode.Balanced;
    private static readonly NativeMethods.EffectivePowerModeCallback _powerModeCallback = OnEffectivePowerModeChanged;

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
            _ => !SystemParameters.IsRemoteSession
                 && SystemParameters.ClientAreaAnimation
                 && !IsEnergySaverOn()
        };
    }

    private static void OnEffectivePowerModeChanged(EffectivePowerMode mode, nint context)
    {
        _effectivePowerMode = mode;
    }

    private static bool IsEnergySaverOn() => _effectivePowerMode is EffectivePowerMode.BatterySaver or EffectivePowerMode.BetterBattery;
}
