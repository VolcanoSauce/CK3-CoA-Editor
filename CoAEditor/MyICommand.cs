using System;
using System.Windows.Input;

namespace CoAEditor {
    public class MyICommand : ICommand {
        Action _TargetExecuteMethod;
        Func<bool> _TargetCanExecuteMethod;

        public MyICommand(Action executeMethod) {
            _TargetExecuteMethod = executeMethod;
        }

        public MyICommand(Action executeMethod, Func<bool> canExecuteMethod) {
            _TargetExecuteMethod = executeMethod;
            _TargetCanExecuteMethod = canExecuteMethod;
        }

        public void OnCanExecuteChanged() {
            CanExecuteChanged(this, EventArgs.Empty);
        }

        bool ICommand.CanExecute(object parameter) {
            if(_TargetCanExecuteMethod != null) {
                return _TargetCanExecuteMethod();
            }

            if(_TargetExecuteMethod != null) {
                return true;
            }

            return false;
        }

        // Should use weak refs if command instance lifetime is longer than lifetime of UI object that get hooked up to command
        public event EventHandler CanExecuteChanged = delegate { };

        void ICommand.Execute(object parameter) {
            if (_TargetExecuteMethod != null)
                _TargetExecuteMethod();
        }
    }
}
