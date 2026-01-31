using Avalonia.Controls;
using Avalonia.Threading;
using Bonsai.Services;
using Bonsai.UI;
using Bonsai.UI.ViewModels;
using Xunit;

namespace Bonsai.Tests.UITests
{
    [Collection("UI")]
    public class MessageTitleTests : UIAvaloniaTestBase
    {
        public MessageTitleTests(UIFixture fixture) : base(fixture.Session) { }

        class FakeDateTimeService3 : IDateTimeService
        {
            public System.DateTime UtcNow { get; set; }
        }

        [Fact]
        public void MessageUpdateReflectsInUI()
        {
            if (Session is null) return;

            Dispatch(() =>
            {
                var svc = new FakeDateTimeService3 { UtcNow = System.DateTime.UtcNow };
                var vm = new MainWindowViewModel(svc);
                var window = new MainWindow();
                window.DataContext = vm;
                window.Show();
                Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render).Wait();

                var msgBlock = window.FindControl<TextBlock>("MessageTextBlock");
                Assert.NotNull(msgBlock);
                Assert.Equal(vm.Message, msgBlock.Text);

                // Change message and ensure it propagates
                vm.Message = "Hello again";

                int attempts = 0;
                const int maxAttempts = 40;
                while (attempts < maxAttempts)
                {
                    Dispatcher.UIThread.RunJobs();
                    if (msgBlock.Text == "Hello again") break;
                    attempts++;
                }

                Assert.Equal("Hello again", msgBlock.Text);
            });
        }

        [Fact]
        public void TitleUpdateReflectsInUI()
        {
            if (Session is null) return;

            Dispatch(() =>
            {
                var svc = new FakeDateTimeService3 { UtcNow = System.DateTime.UtcNow };
                var vm = new MainWindowViewModel(svc);
                var window = new MainWindow();
                window.DataContext = vm;
                window.Show();
                Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render).Wait();

                var titleBlock = window.FindControl<TextBlock>("TitleTextBlock");
                Assert.NotNull(titleBlock);
                Assert.Equal(vm.Title, titleBlock.Text);

                vm.Title = "NewTitle";

                int attempts = 0;
                const int maxAttempts = 40;
                while (attempts < maxAttempts)
                {
                    Dispatcher.UIThread.RunJobs();
                    if (titleBlock.Text == "NewTitle") break;
                    attempts++;
                }

                Assert.Equal("NewTitle", titleBlock.Text);
            });
        }
    }
}
