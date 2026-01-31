using System;
using Xunit;
using Bonsai.Services;
using Bonsai.UI.ViewModels;

namespace Bonsai.Tests;

public class HelloWorldTest
{
    private class FakeDateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    [Fact]
    public void ViewModelHasDefaultMessage()
    {
        var vm = new MainWindowViewModel(new FakeDateTimeService());
        Assert.Equal("Hello Bonsai!", vm.Message);
    }

    [Fact]
    public void ConstructorThrowsOnNullDateTimeService()
    {
        Assert.Throws<ArgumentNullException>(() => new MainWindowViewModel(null!));
    }
}
