using System;
using System.Collections.Generic;

namespace DaddysArcade
{
    // Shared by arcade games; game-specific options append after the standard routes.
    public sealed class ArcadePauseMenu
    {
        public sealed class Option
        {
            public string Label { get; set; }
            readonly Action action;
            public Option(string label, Action action) { Label = label; this.action = action; }
            public void Invoke() => action();
        }
        public readonly List<Option> Options = new List<Option>();
        public int Selected { get; private set; }
        public ArcadePauseMenu(Action resume, Action mainMenu, Action exit, params Option[] gameOptions)
        {
            Options.Add(new Option("Resume", resume));
            Options.Add(new Option("Main Menu", mainMenu));
            Options.Add(new Option("Exit to Arcade", exit));
            Options.AddRange(gameOptions);
        }
        public void Move(int direction) => Selected = (Selected + direction + Options.Count) % Options.Count;
        public void Confirm() => Options[Selected].Invoke();
    }
}
