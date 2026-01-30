using Xunit;
using Bonsai.UI.ViewModels;

namespace Bonsai.Tests;

public class HelloWorldTest
{
    [Fact]
    public void ViewModelHasDefaultMessage()
    {
        var vm = new MainWindowViewModel();
        Assert.Equal("Hello Bonsai!", vm.Message);
    }
}
