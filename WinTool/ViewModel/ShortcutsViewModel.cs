using CommunityToolkit.Mvvm.ComponentModel;
using WinTool.Model;
using WinTool.Services;

namespace WinTool.ViewModel;

public class ShortcutsViewModel : ObservableObject
{
    private readonly SettingsManager _settingsManager;
    private readonly Settings _settings;

    public string? NewFileTemplate
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                _settings.NewFileTemplate = value;
                _settingsManager.UpdateSettings(_settings);
            }
        }
    }

    public ShortcutsViewModel(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        _settings = _settingsManager.GetSettings() ?? new Settings();

        NewFileTemplate = _settings.NewFileTemplate;
    }
}
