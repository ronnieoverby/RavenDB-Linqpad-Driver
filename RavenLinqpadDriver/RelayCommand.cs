using System;
using System.Windows.Input;
using RavenLinqpadDriver.Annotations;

namespace RavenLinqpadDriver
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Func<object, bool> _guard;

        public RelayCommand([NotNull] Action action, Func<bool> guard = null)
        {
            if (action == null) throw new ArgumentNullException("action");
            _action = o => action();
            
            if (guard != null)
                _guard = o => guard();
        }

        public RelayCommand([NotNull] Action<object> action, Func<object,bool> guard = null)
        {
            if (action == null) throw new ArgumentNullException("action");
            _action = action;
            _guard = guard;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return _guard == null || _guard(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public virtual void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}