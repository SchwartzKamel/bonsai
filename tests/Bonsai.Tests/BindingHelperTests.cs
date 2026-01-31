using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using Bonsai.UI;
using Xunit;

namespace Bonsai.Tests
{
    [Collection("UI")]
    public class BindingHelperTests : UIAvaloniaTestBase
    {
        public BindingHelperTests(UIFixture fixture) : base(fixture.Session) { }

        class TestNotify : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            private string _value = string.Empty;
            public string Value
            {
                get => _value;
                set
                {
                    if (_value == value) return;
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        [Fact]
        public async Task BindingHelper_InvokesAction_OnPropertyChange()
        {
            if (Session is null) return;

            await DispatchAsync(async () =>
            {
                var src = new TestNotify();
                var tcs = new TaskCompletionSource<bool>();

                using var bh = new BindingHelper(src);
                bh.Bind(nameof(TestNotify.Value), () => tcs.TrySetResult(true));

                Dispatcher.UIThread.Post(() => src.Value = "hello");
                // Ensure posted jobs run on the headless dispatcher and wait with a timeout
                Dispatcher.UIThread.RunJobs();
                var finished = await Task.WhenAny(tcs.Task, Task.Delay(500));
                Assert.True(tcs.Task.IsCompleted, "BindingHelper did not invoke action within timeout");
            });
        }

        [Fact]
        public async Task BindingHelper_Dispose_StopsInvoking()
        {
            if (Session is null) return;

            await DispatchAsync(async () =>
            {
                var src = new TestNotify();
                using var bh = new BindingHelper(src);

                bool invoked = false;
                bh.Bind(nameof(TestNotify.Value), () => invoked = true);

                // Dispose and then change property
                bh.Dispose();
                Dispatcher.UIThread.Post(() => src.Value = "world");
                Dispatcher.UIThread.RunJobs();

                Assert.False(invoked);
            });
        }
    }
}
