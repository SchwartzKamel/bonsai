using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Bonsai.Services;
using Bonsai.UI;
using Bonsai.UI.ViewModels;
using Xunit;

namespace Bonsai.Tests.UITests
{
    class FakeDateTimeService : IDateTimeService
    {
        public DateTime UtcNow { get; set; }
    }

    public class RefreshFlowTests
    {
        [StaFact]
        public async Task RefreshButtonUpdatesLastUpdatedText()
        {
            // Arrange: set deterministic times
            var fake = new FakeDateTimeService { UtcNow = new DateTime(2026, 1, 30, 12, 0, 0, DateTimeKind.Utc) };
            var vm = new MainWindowViewModel(fake);

            // Simulate later time for refresh
            var refreshedTime = new DateTime(2026, 1, 30, 12, 5, 0, DateTimeKind.Utc);

            // Create window and attach DataContext
            var window = new MainWindow();
            window.DataContext = vm;

            // Ensure template initialized and names available
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

            // Find controls
            var refreshBtn = window.FindControl<Button>("RefreshButton");
            var lastUpdated = window.FindControl<TextBlock>("LastUpdatedTextBlock");

            // Initial LastUpdated set by ctor
            Assert.Equal(fake.UtcNow.ToString("u"), vm.LastUpdated);

            // Act: change time and click refresh
            fake.UtcNow = refreshedTime;
            // Raise click event
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var args = new RoutedEventArgs(Button.ClickEvent);
                refreshBtn.RaiseEvent(args);
            });

            // Wait/poll for UI to update (posted via dispatcher)
            var timeout = TimeSpan.FromSeconds(2);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                await Task.Delay(50);
                var text = lastUpdated.Text;
                if (text == refreshedTime.ToString("u"))
                {
                    // Success
                    return;
                }
            }

            // If we get here, test failed
            Assert.Equal(refreshedTime.ToString("u"), lastUpdated.Text);
        }
    }
}
