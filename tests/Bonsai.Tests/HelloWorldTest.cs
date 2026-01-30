using Xunit;
using Bonsai.UI.ViewModels;

namespace Bonsai.Tests;

public class HelloWorldTest
{
    [Fact]
    public void ViewModelHasDefaultMessage()
    {
        // pass a placeholder dateTimeService (null-forgiving) to satisfy the constructor dependency in tests
        var vm = new MainWindowViewModel(null!);
        Assert.Equal("Hello Bonsai!", vm.Message);
    }
}
