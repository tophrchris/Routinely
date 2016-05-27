using System;
using Foundation;
using UIKit;
using SWTableViewCells;
using System.Collections.Generic;
using ClockKing.Model;
using System.Linq;

namespace ClockKing
{
	public class CommandManager
	{
		public Dictionary<string,Command> Commands { get; set; }

		public CommandManager()
		{
			var cmds = CreateCommands ();
			Commands = cmds.ToDictionary (u => u.Name);
		}

		private List<Command> CreateCommands(){
			return new List<Command> () {
				new EnableCheckPointCommand(),
				new DisableCheckPointCommand(),
				new AddOccurrenceCommand(),
				new AddHistoricOccurrenceCommand(),
				new DeleteCheckPointCommand()
			};
		}

		public IEnumerable<UIPreviewAction> GetPreviewActionsForCheckpoint(CheckPoint toView,Action<Command> handler)
		{
			return this.Commands.Values
				.Where (u => u.ShouldDecorate (toView))
				.Select (u => u.AsPreviewAction (handler));
		}

		public bool AttachUtilityButtonsToCell(CheckPointTableCell cell)
		{

			var cmdsForThisCell = this.CreateCommands ();
			var active = cmdsForThisCell
				.Where (u => u.ShouldDecorate (cell.CheckPoint))
				.Select(u=>u);
			var grouped = active.GroupBy (u => u.Category);

			foreach(var g in grouped)
			{
				if (g.Key == "Left")
					cell.LeftUtilityButtons = g.ToList().ToArray ();
				else if (g.Key == "Right")
					cell.RightUtilityButtons = g.ToList().ToArray ();
			}
			return true;
		}
	}
}

