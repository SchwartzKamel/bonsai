using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using Bonsai.UI;
using Xunit;

namespace Bonsai.Tests
{
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

    public class BindingHelperTests
    {
        [Fact]
        public async Task BindingHelper_InvokesAction_OnPropertyChange()
        {
            var src = new TestNotify();
            var tcs = new TaskCompletionSource<bool>();

            using var bh = new BindingHelper(src);
            bh.Bind(nameof(TestNotify.Value), () => tcs.TrySetResult(true));

            src.Value = "hello";

            // Wait for the action to be executed on the Avalonia UI thread
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(2000));
            Assert.True(completed == tcs.Task, "BindingHelper did not invoke action within timeout");
        }

        [Fact]
        public async Task BindingHelper_Dispose_StopsInvoking()
        {
            var src = new TestNotify();
            using var bh = new BindingHelper(src);

            bool invoked = false;
            bh.Bind(nameof(TestNotify.Value), () => invoked = true);

            // Dispose and then change property
            bh.Dispose();
            src.Value = "world";

            // Allow some time for any pending dispatcher actions
            await Task.Delay(250);
            Assert.False(invoked, "BindingHelper invoked action after being disposed");
        }
    }
}
