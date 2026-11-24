using CommunityToolkit.Mvvm.ComponentModel;
using WinTool.WinUI.Options;

namespace WinTool.WinUI.ViewModels.Features;

public partial class FeaturesViewModel : ObservableObject
{
    private readonly WritableOptions<FeaturesOptions> _featuresOptions;

    private bool _isInitializing;

    public bool EnableSwitchLanguagePopup
    {
        get; set
        {
            if (SetProperty(ref field, value) && !_isInitializing)
            {
                _featuresOptions.Update(o => o.EnableInputPopup = value);
            }
        }
    }

    public FeaturesViewModel(WritableOptions<FeaturesOptions> featuresOptions)
    {
        _featuresOptions = featuresOptions;
        _isInitializing = true;

        EnableSwitchLanguagePopup = _featuresOptions.CurrentValue.EnableInputPopup;

        _isInitializing = false;
    }
}
