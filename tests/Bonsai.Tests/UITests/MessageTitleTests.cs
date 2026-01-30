using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Bonsai.Services;
using Bonsai.UI;
using Bonsai.UI.ViewModels;
using Xunit;

namespace Bonsai.Tests.UITests
{
    class FakeDateTimeService3 : IDateTimeService
    {
        public System.DateTime UtcNow { get; set; }
    }

    public class MessageTitleTests
    {
        [StaFact]
        public async Task MessageUpdateReflectsInUI()
        {
            var svc = new FakeDateTimeService3 { UtcNow = System.DateTime.UtcNow };
            var vm = new MainWindowViewModel(svc);
            var window = new MainWindow();
            window.DataContext = vm;
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

            var msgBlock = window.FindControl<TextBlock>("MessageTextBlock");
            Assert.Equal(vm.Message, msgBlock.Text);

            // Change message and ensure it propagates
            vm.Message = "Hello again";

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 1000)
            {
                await Task.Delay(25);
                if (msgBlock.Text == "Hello again") return;
            }

            Assert.Equal("Hello again", msgBlock.Text);
        }

        [StaFact]
        public async Task TitleUpdateReflectsInUI()
        {
            var svc = new FakeDateTimeService3 { UtcNow = System.DateTime.UtcNow };
            var vm = new MainWindowViewModel(svc);
            var window = new MainWindow();
            window.DataContext = vm;
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

            var titleBlock = window.FindControl<TextBlock>("TitleTextBlock");
            Assert.Equal(vm.Title, titleBlock.Text);

            vm.Title = "NewTitle";

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 1000)
            {
                await Task.Delay(25);
                if (titleBlock.Text == "NewTitle") return;
            }

            Assert.Equal("NewTitle", titleBlock.Text);
        }
    }
}
