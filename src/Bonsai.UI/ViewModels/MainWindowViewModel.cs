using CommunityToolkit.Mvvm.ComponentModel;

namespace Bonsai.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Bonsai";

    [ObservableProperty]
    private string message = "Hello Bonsai!";
}
