using System;

namespace tulo.ResourcesWpfLib.Commands
{
    public class SearchFilterControlCommand(Action<string> action) : BaseCommand
    {
        private readonly Action<string> _action = action;

        public override void Execute(object parameter)
        {
            _action?.Invoke((string)parameter);
        }
    }
}
