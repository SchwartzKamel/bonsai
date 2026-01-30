using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Bonsai.Services;
using Bonsai.UI;
using Bonsai.UI.ViewModels;
using Xunit;

namespace Bonsai.Tests.UITests
{
    class FakeDateTimeService : IDateTimeService { public DateTime UtcNow { get; set; } }

    public class MessageAndTitleTests
    {
        [StaFact]
        public async Task MessageChangeUpdatesUI()
        {
            var fake = new FakeDateTimeService { UtcNow = DateTime.UtcNow };
            var vm = new MainWindowViewModel(fake);
            var window = new MainWindow();
            window.DataContext = vm;

            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

            var msgBlock = window.FindControl<TextBlock>("MessageTextBlock");
            Assert.Equal(vm.Message, msgBlock.Text);

            // Change VM.Message and assert UI updates
            vm.Message = "A new message";

            var timeout = TimeSpan.FromSeconds(2);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                await Task.Delay(50);
                if (msgBlock.Text == "A new message") return;
            }

            Assert.Equal("A new message", msgBlock.Text);
        }

        [StaFact]
        public async Task TitleChangeUpdatesWindowAndTitleTextBlock()
        {
            var fake = new FakeDateTimeService { UtcNow = DateTime.UtcNow };
            var vm = new MainWindowViewModel(fake);
            var window = new MainWindow();
            window.DataContext = vm;

            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

            var titleBlock = window.FindControl<TextBlock>("TitleTextBlock");
            Assert.Equal(vm.Title, titleBlock.Text);

            vm.Title = "New Title";

            var timeout = TimeSpan.FromSeconds(2);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                await Task.Delay(50);
                if (titleBlock.Text == "New Title" && window.Title == "New Title") return;
            }

            Assert.Equal("New Title", titleBlock.Text);
            Assert.Equal("New Title", window.Title);
        }
    }
}
