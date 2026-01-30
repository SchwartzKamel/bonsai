using System;
using Bonsai.Services;
using Bonsai.UI.ViewModels;
using Xunit;

namespace Bonsai.Tests
{
    class FakeDateTimeService2 : IDateTimeService
    {
        public DateTime UtcNow { get; set; }
    }

    public class ViewModelTests
    {
        [Fact]
        public void ConstructorSetsLastUpdated()
        {
            var now = new DateTime(2026,1,30,13,0,0, DateTimeKind.Utc);
            var svc = new FakeDateTimeService2 { UtcNow = now };
            var vm = new MainWindowViewModel(svc);
            Assert.Equal(now.ToString("u"), vm.LastUpdated);
        }

        [Fact]
        public void RefreshUpdatesLastUpdated()
        {
            var now = new DateTime(2026,1,30,13,0,0, DateTimeKind.Utc);
            var svc = new FakeDateTimeService2 { UtcNow = now };
            var vm = new MainWindowViewModel(svc);

            var later = new DateTime(2026,1,30,13,30,0, DateTimeKind.Utc);
            svc.UtcNow = later;
            vm.Refresh();
            Assert.Equal(later.ToString("u"), vm.LastUpdated);
        }

        [Fact]
        public void MessageAndTitleSettersWork()
        {
            var svc = new FakeDateTimeService2 { UtcNow = DateTime.UtcNow };
            var vm = new MainWindowViewModel(svc);
            vm.Message = "New message";
            vm.Title = "New title";
            Assert.Equal("New message", vm.Message);
            Assert.Equal("New title", vm.Title);
        }
    }
}
