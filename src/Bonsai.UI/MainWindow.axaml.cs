using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Bonsai.UI.ViewModels;

namespace Bonsai.UI;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _vm;
    private BindingHelper? _bindingHelper;

    public MainWindow()
    {
        InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;
        this.Closed += OnClosed;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _bindingHelper?.Dispose();
        _bindingHelper = null;

        _vm = this.DataContext as MainWindowViewModel;
        if (_vm is not null)
        {
            // Use BindingHelper to minimize boilerplate and ensure UI-thread updates
            _bindingHelper = new BindingHelper(_vm);
            _bindingHelper.Bind(nameof(MainWindowViewModel.Message), () => MessageTextBlock.Text = _vm.Message ?? string.Empty);
            _bindingHelper.Bind(nameof(MainWindowViewModel.Title), () =>
            {
                TitleTextBlock.Text = _vm.Title ?? string.Empty;
                this.Title = _vm.Title ?? this.Title;
            });
            _bindingHelper.Bind(nameof(MainWindowViewModel.LastUpdated), () => LastUpdatedTextBlock.Text = _vm.LastUpdated ?? string.Empty);

            // Wire Refresh button to VM
            try
            {
                RefreshButton.Click -= OnRefreshClicked;
            }
            catch { }
            RefreshButton.Click += OnRefreshClicked;
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        try
        {
            RefreshButton.Click -= OnRefreshClicked;
        }
        catch { }

        _bindingHelper?.Dispose();
        _bindingHelper = null;
        _vm = null;
    }

    private void OnRefreshClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm?.Refresh();
    }
}
