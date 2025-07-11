using CommunityToolkit.Mvvm.ComponentModel;
using WinTool.Options;

namespace WinTool.ViewModel;

public class FeaturesViewModel : ObservableObject
{
    private readonly WritableOptions<FeaturesOptions> _featuresOptions;

    public bool EnableSwitchLanguagePopup
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                _featuresOptions.Value.EnableSwitchLanguagePopup = value;
                _featuresOptions.Update();
            }
        }
    }

    public FeaturesViewModel(WritableOptions<FeaturesOptions> featuresOptions)
    {
        _featuresOptions = featuresOptions;
        EnableSwitchLanguagePopup = _featuresOptions.Value.EnableSwitchLanguagePopup;
    }
}
