using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MMX.Infrastructure.Utilities
{
    public class BasicCommand : ICommand
    {
        Func<object, bool> _validator;
        Action<object> _action;

        public BasicCommand(Action<object> action)
            : this(new Func<object, bool>((o) => { return true; }), action)
        {
        }

        public BasicCommand(Func<object, bool> validator, Action<object> action)
        {
            _validator = validator;
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return _validator(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action.Invoke(parameter);
        }
    }
}
