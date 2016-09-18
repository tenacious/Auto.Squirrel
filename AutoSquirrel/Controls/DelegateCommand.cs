namespace AutoSquirrel
{
    using System;
    using System.Diagnostics;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly System.Action _execute;

        public DelegateCommand(System.Action execute) : this(execute, null)
        {
        }

        public DelegateCommand(System.Action execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = execute; _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter) => _canExecute == null ? true : _canExecute(parameter);

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}