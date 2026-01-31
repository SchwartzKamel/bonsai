using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Bonsai.UI.ViewModels;

namespace Bonsai.UI;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Resolve MainWindow and ViewModel via DI
            var services = HostContainer.ServiceProvider ?? throw new InvalidOperationException("Host service provider not set. Ensure the host is built and HostContainer.ServiceProvider is assigned before starting the app.");
            var vm = services.GetRequiredService<MainWindowViewModel>();
            var mw = services.GetRequiredService<MainWindow>();
            mw.DataContext = vm;
            desktop.MainWindow = mw;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
