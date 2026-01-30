using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Threading;

namespace Bonsai.UI
{
    /// <summary>
    /// Small helper to bind INotifyPropertyChanged properties to UI-updating actions.
    /// Ensures handlers are invoked on the UI thread via Avalonia's Dispatcher.
    /// </summary>
    public sealed class BindingHelper : IDisposable
    {
        private readonly INotifyPropertyChanged _source;
        private readonly List<(string PropertyName, Action Action)> _bindings = new();

        public BindingHelper(INotifyPropertyChanged source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _source.PropertyChanged += OnPropertyChanged;
        }

        public void Bind(string propertyName, Action action, bool invokeImmediately = true)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("propertyName");
            if (action == null) throw new ArgumentNullException(nameof(action));

            _bindings.Add((propertyName, action));

            if (invokeImmediately)
            {
                // Invoke on UI thread so initial UI sync is safe
                Dispatcher.UIThread.Post(action);
            }
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e is null || string.IsNullOrEmpty(e.PropertyName)) return;

            foreach (var (PropertyName, Action) in _bindings)
            {
                if (PropertyName == e.PropertyName)
                {
                    Dispatcher.UIThread.Post(Action);
                }
            }
        }

        public void Dispose()
        {
            _source.PropertyChanged -= OnPropertyChanged;
            _bindings.Clear();
        }
    }
}
