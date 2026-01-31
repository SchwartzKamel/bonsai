using System;
using Avalonia.Controls;
using Avalonia.Threading;
using Bonsai.Services;
using Bonsai.UI;
using Bonsai.UI.ViewModels;
using Xunit;

namespace Bonsai.Tests.UITests
{
    [Collection("UI")]
    public class MessageAndTitleTests : UIAvaloniaTestBase
    {
        public MessageAndTitleTests(UIFixture fixture) : base(fixture.Session) { }

        private class FakeDateTimeService : IDateTimeService { public DateTime UtcNow { get; set; } }

        [Fact]
        public void MessageChangeUpdatesUI()
        {
            if (Session is null) return;

            Dispatch(() =>
            {
                var fake = new FakeDateTimeService { UtcNow = DateTime.UtcNow };
                var vm = new MainWindowViewModel(fake);
                var window = new MainWindow();
                window.DataContext = vm;
                window.Show();
                Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render).Wait();

                var msgBlock = window.FindControl<TextBlock>("MessageTextBlock");
                Assert.NotNull(msgBlock);
                Assert.Equal(vm.Message, msgBlock.Text);

                // Change VM.Message and assert UI updates
                vm.Message = "A new message";

                int attempts = 0;
                const int maxAttempts = 40;
                while (attempts < maxAttempts)
                {
                    Dispatcher.UIThread.RunJobs();
                    if (msgBlock.Text == "A new message") break;
                    attempts++;
                }

                Assert.Equal("A new message", msgBlock.Text);
            });
        }

        [Fact]
        public void TitleChangeUpdatesWindowAndTitleTextBlock()
        {
            if (Session is null) return;

            Dispatch(() =>
            {
                var fake = new FakeDateTimeService { UtcNow = DateTime.UtcNow };
                var vm = new MainWindowViewModel(fake);
                var window = new MainWindow();
                window.DataContext = vm;
                window.Show();
                Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render).Wait();

                var titleBlock = window.FindControl<TextBlock>("TitleTextBlock");
                Assert.NotNull(titleBlock);
                Assert.Equal(vm.Title, titleBlock.Text);

                vm.Title = "New Title";

                int attempts = 0;
                const int maxAttempts = 40;
                while (attempts < maxAttempts)
                {
                    Dispatcher.UIThread.RunJobs();
                    if (titleBlock.Text == "New Title" && window.Title == "New Title") break;
                    attempts++;
                }

                Assert.Equal("New Title", titleBlock.Text);
                Assert.Equal("New Title", window.Title);
            });
        }
    }
}
