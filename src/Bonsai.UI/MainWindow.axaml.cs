using System;
using System.ComponentModel;
using Avalonia.Controls;
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
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _bindingHelper?.Dispose();
        _bindingHelper = null;
        _vm = null;
    }
}
