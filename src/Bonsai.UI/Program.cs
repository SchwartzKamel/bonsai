using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Bonsai.Services;
using Bonsai.UI.ViewModels;
using Avalonia;
using Bonsai.UI;

// Minimal Program.cs sketch for Bonsai (use this as a starting point)
// - Builds a Generic Host for DI
// - Registers services and viewmodels
// - Starts Avalonia with classic desktop lifetime

class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                // Register application services
                services.AddSingleton<IDateTimeService, SystemDateTimeService>();
                services.AddTransient<MainWindowViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        // Make the ServiceProvider available to the App (simple static container shown here)
        HostContainer.ServiceProvider = host.Services;

        // Start Avalonia
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
