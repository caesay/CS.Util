using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CS.Wpf.Annotations;

namespace CS.Wpf
{
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        { }
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute(object parameter) { _execute(parameter); }
    }
    public class RelayUICommand : RelayCommand, INotifyPropertyChanged
    {
        private DataTemplate _iconTemplate;
        private string _text;

        public DataTemplate IconTemplate
        {
            get { return _iconTemplate; }
            set
            {
                if (Equals(value, _iconTemplate)) return;
                _iconTemplate = value;
                OnPropertyChanged();
            }
        }
        public string Text
        {
            get { return _text; }
            set
            {
                if (value == _text) return;
                _text = value;
                OnPropertyChanged();
            }
        }

        public RelayUICommand(Action<object> execute)
            : this(execute, null)
        { }
        public RelayUICommand(Action<object> execute, Predicate<object> canExecute)
            : this(execute, canExecute, null)
        { }
        public RelayUICommand(Action<object> execute, Predicate<object> canExecute, string text)
            : this(execute, canExecute, text, null)
        { }
        public RelayUICommand(Action<object> execute, Predicate<object> canExecute, string text, DataTemplate icon)
            : base(execute, canExecute)
        {
            Text = text;
            IconTemplate = icon;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
