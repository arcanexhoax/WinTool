using CommunityToolkit.Mvvm.ComponentModel;
using WinTool.Model;
using WinTool.Services;

namespace WinTool.ViewModel;

public class FeaturesViewModel : ObservableObject
{
    private readonly SettingsManager _settingsManager;
    private readonly SwitchLanguageViewModel _switchLanguageVm;
    private readonly Settings _settings;

    public bool EnableSwitchLanguagePopup
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                _settings.EnableSwitchLanguagePopup = value;
                _settingsManager.UpdateSettings(_settings);

                if (value)
                    _switchLanguageVm.Start();
                else
                    _switchLanguageVm.Stop();
            }
        }
    }

    public FeaturesViewModel(SettingsManager settingsManager, SwitchLanguageViewModel switchLanguageViewModel)
    {
        _settingsManager = settingsManager;
        _switchLanguageVm = switchLanguageViewModel;
        _settings = _settingsManager.GetSettings() ?? new Settings();

        EnableSwitchLanguagePopup = _settings.EnableSwitchLanguagePopup;
    }
}
