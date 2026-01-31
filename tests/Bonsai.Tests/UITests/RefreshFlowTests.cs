using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Bonsai.Services;
using Bonsai.UI;
using Bonsai.UI.ViewModels;
using Xunit;

namespace Bonsai.Tests.UITests
{
    [Collection("UI")]
    public class RefreshFlowTests : UIAvaloniaTestBase
    {
        public RefreshFlowTests(UIFixture fixture) : base(fixture.Session) { }

        class FakeDateTimeService : IDateTimeService
        {
            public DateTime UtcNow { get; set; }
        }

        [Fact]
        public void RefreshButtonUpdatesLastUpdatedText()
        {
            if (Session is null) return;

            Dispatch(() =>
            {
                // Arrange: set deterministic times
                var fake = new FakeDateTimeService { UtcNow = new DateTime(2026, 1, 30, 12, 0, 0, DateTimeKind.Utc) };
                var vm = new MainWindowViewModel(fake);

                // Simulate later time for refresh
                var refreshedTime = new DateTime(2026, 1, 30, 12, 5, 0, DateTimeKind.Utc);

                // Create window and attach DataContext
                var window = new MainWindow();
                window.DataContext = vm;
                window.Show();

                // Ensure template initialized and names available
                Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render).Wait();

                // Find controls
                var refreshBtn = window.FindControl<Button>("RefreshButton") ?? throw new InvalidOperationException("RefreshButton not found");
                var lastUpdated = window.FindControl<TextBlock>("LastUpdatedTextBlock") ?? throw new InvalidOperationException("LastUpdatedTextBlock not found");

                // Initial LastUpdated set by ctor
                Assert.Equal(fake.UtcNow.ToString("u"), vm.LastUpdated);

                // Act: change time and click refresh
                fake.UtcNow = refreshedTime;
                // Raise click event
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var args = new RoutedEventArgs(Button.ClickEvent);
                    refreshBtn.RaiseEvent(args);
                }).Wait();

                // Poll for UI to update (posted via dispatcher)
                int attempts = 0;
                const int maxAttempts = 40;
                while (attempts < maxAttempts)
                {
                    Dispatcher.UIThread.RunJobs();
                    var text = lastUpdated.Text;
                    if (text == refreshedTime.ToString("u"))
                    {
                        // Success
                        break;
                    }
                    attempts++;
                }

                // If we get here, test failed
                Assert.Equal(refreshedTime.ToString("u"), lastUpdated.Text);
            });
        }
    }
}
