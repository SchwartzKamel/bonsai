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
            // Resolve MainWindow and ViewModel via DI only if host is available (skip in headless/test mode)
            if (HostContainer.ServiceProvider != null)
            {
                var services = HostContainer.ServiceProvider;
                var vm = services.GetRequiredService<MainWindowViewModel>();
                var mw = services.GetRequiredService<MainWindow>();
                mw.DataContext = vm;
                desktop.MainWindow = mw;
            }
            // In headless/test mode, no main window is set; tests create their own
        }

        base.OnFrameworkInitializationCompleted();
    }
}
