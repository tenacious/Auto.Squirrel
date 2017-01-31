namespace AutoSquirrel
{
    using System;
    using System.Diagnostics;
    using System.Windows.Input;

    /// <summary>
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand"/>
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly System.Action _execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute.</param>
        public DelegateCommand(System.Action execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute.</param>
        /// <param name="canExecute">The can execute.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DelegateCommand(System.Action execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            this._execute = execute; this._canExecute = canExecute;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object
        /// can be set to null.
        /// </param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter) => this._canExecute == null ? true : this._canExecute(parameter);

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object
        /// can be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            this._execute();
        }
    }
}