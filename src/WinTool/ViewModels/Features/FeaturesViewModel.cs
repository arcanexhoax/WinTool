using CommunityToolkit.Mvvm.ComponentModel;
using WinTool.Options;

namespace WinTool.ViewModels.Features;

public class FeaturesViewModel : ObservableObject
{
    private readonly WritableOptions<FeaturesOptions> _featuresOptions;

    public bool EnableSwitchLanguagePopup
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                _featuresOptions.Update(o => o.EnableInputPopup = value);
            }
        }
    }

    public FeaturesViewModel(WritableOptions<FeaturesOptions> featuresOptions)
    {
        _featuresOptions = featuresOptions;
        EnableSwitchLanguagePopup = _featuresOptions.CurrentValue.EnableInputPopup;
    }
}
