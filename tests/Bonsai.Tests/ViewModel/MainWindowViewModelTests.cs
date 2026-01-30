using System;
using Bonsai.Services;
using Bonsai.UI.ViewModels;
using Xunit;

namespace Bonsai.Tests.ViewModel
{
    class FakeDateTimeService : IDateTimeService
    {
        public DateTime UtcNow { get; set; }
    }

    public class MainWindowViewModelTests
    {
        [Fact]
        public void ConstructorSetsLastUpdated()
        {
            var fake = new FakeDateTimeService { UtcNow = new DateTime(2026,1,30,13,0,0, DateTimeKind.Utc) };
            var vm = new MainWindowViewModel(fake);
            Assert.Equal(fake.UtcNow.ToString("u"), vm.LastUpdated);
        }

        [Fact]
        public void RefreshUpdatesLastUpdated()
        {
            var fake = new FakeDateTimeService { UtcNow = new DateTime(2026,1,30,13,0,0, DateTimeKind.Utc) };
            var vm = new MainWindowViewModel(fake);
            var first = vm.LastUpdated;

            fake.UtcNow = fake.UtcNow.AddMinutes(5);
            vm.Refresh();

            Assert.Equal(fake.UtcNow.ToString("u"), vm.LastUpdated);
            Assert.NotEqual(first, vm.LastUpdated);
        }

        [Fact]
        public void MessagePropertyNotifiesAndDefaults()
        {
            var fake = new FakeDateTimeService { UtcNow = DateTime.UtcNow };
            var vm = new MainWindowViewModel(fake);
            Assert.Equal("Hello Bonsai!", vm.Message);

            bool changed = false;
            vm.PropertyChanged += (s,e) => { if (e.PropertyName == nameof(vm.Message)) changed = true; };
            vm.Message = "Updated";
            Assert.True(changed);
            Assert.Equal("Updated", vm.Message);
        }
    }
}
