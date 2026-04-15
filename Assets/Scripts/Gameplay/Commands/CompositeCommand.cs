using System.Collections;
using System.Collections.Generic;
using Interfaces;

namespace Gameplay.Commands
{
    public class CompositeCommand : ICommand
    {
        private readonly List<ICommand> _commands;

        public CompositeCommand(List<ICommand> commands)
        {
            _commands = commands;
        }

        public IEnumerator ExecuteAsync()
        {
            foreach (var cmd in _commands)
            {
                var enumerator = cmd.ExecuteAsync();
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                }
            }
        }
    }
}
