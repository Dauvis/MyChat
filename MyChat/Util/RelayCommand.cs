using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyChat.Util
{
    public class RelayCommand : ICommand
    {
        private readonly Func<Task>? _asyncExecute;
        private readonly Action<object>? _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        // Constructor for asynchronous commands
        public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _asyncExecute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Constructor for synchronous commands
        public RelayCommand(Action<object> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public async void Execute(object? parameter)
        {
            if (_asyncExecute != null)
            {
                await _asyncExecute();
            }
            else if (_execute != null)
            {
                if (parameter is null)
                {
                    return;
                }

                _execute(parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
