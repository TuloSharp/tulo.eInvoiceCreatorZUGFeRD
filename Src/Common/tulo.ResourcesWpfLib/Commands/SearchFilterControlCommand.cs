using System;
using tulo.CommonMVVM.Commands;

namespace tulo.ResourcesWpfLib.Commands
{
    public class SearchFilterControlCommand : BaseCommand
    {
        Action<string> _action;

        public SearchFilterControlCommand(Action<string> action)
        {
            _action = action;
        }

        public override void Execute(object parameter)
        {
            _action?.Invoke((string)parameter);
        }
    }
}
