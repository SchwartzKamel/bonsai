using CommunityToolkit.Mvvm.ComponentModel;

namespace Bonsai.UI.ViewModels;

using Bonsai.Services;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IDateTimeService _dateTimeService;

    public MainWindowViewModel(IDateTimeService dateTimeService)
    {
        _dateTimeService = dateTimeService;
        LastUpdated = _dateTimeService.UtcNow.ToString("u");
    }

    [ObservableProperty]
    private string title = "Bonsai";

    [ObservableProperty]
    private string message = "Hello Bonsai!";

    [ObservableProperty]
    private string lastUpdated = string.Empty;

    public void Refresh()
    {
        LastUpdated = _dateTimeService.UtcNow.ToString("u");
    }
}
