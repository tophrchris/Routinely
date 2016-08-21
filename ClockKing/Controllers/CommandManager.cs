using System;
using Foundation;
using UIKit;
using SWTableViewCells;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;
using MonoTouch.Dialog;

namespace ClockKing
{
	public class CommandManager
	{
		public Dictionary<string,Command> Commands { get; set; }

		public CommandManager()
		{
			this.ConstructCommands();
		}

		public void ConstructCommands()
		{
			var cmds = CreateCommands();
			Commands = cmds.ToDictionary(u => u.Name);
		}

		private List<Command> CreateCommands()
		{
			
			return new List<Command>() {
				new EditCheckPointCommand(),
				new InPlaceEditCheckPointCommand(),
				new EnableCheckPointCommand(),
				new DisableCheckPointCommand(),
				new AddOccurrenceCommand(),
				new AddHistoricOccurrenceCommand(),
				new DeleteCheckPointCommand(),
				new AddScheduledTargetCommand(),
				new SkipCheckPointCommand(){AllowForAllIncompleteGoals=!ClockKingOptions.OnlySkipMissed},
				new UndoOccurrenceCommand()
			};
		}
			
		public IEnumerable<UIAlertAction> GetAlertActionsForCheckpoint(CheckPoint selected,Action<Command> handler,iNavigatableDialog dialog=null)
		{
			var commands = this.Commands.AsEnumerable();

			if (dialog != null)
				commands = commands.Where(c=>c.Key!="Edit");

			return commands
				.Select(c=>c.Value)
				.Where (c => c.ShouldDecorate (selected))
				.Select(c=>{
					if(c is IDialogBoundCommand  && dialog!=null)
						((IDialogBoundCommand)c).ExistingDialog=(iNavigatableDialog)dialog;
					return c;
				})
				.Select (c => c.AsAlertAction (handler));
		}


		public IEnumerable<UIPreviewAction> GetPreviewActionsForCheckpoint(CheckPoint toView,Action<Command> handler)
		{
			return this.Commands.Values
				.Where (u => u.ShouldDecorate (toView))
				.Where(u=> (u as IDialogBoundCommand)==null)
				.Select (u => u.AsPreviewAction (handler));
		}

		public bool AttachUtilityButtonsToCell(CheckPointTableCell cell)
		{
			var cmdsForThisCell = this.CreateCommands ();
			var grouped = cmdsForThisCell
				.Where (u => u.ShouldDecorate (cell.CheckPoint))
				.Where(u=>(u as IDialogBoundCommand)==null)
				.GroupBy (u => u.Category, c => c.AsButton ());
	
			foreach(var g in grouped)
			{
				if (g.Key == "Left")
					cell.LeftUtilityButtons = g.ToArray ();
				else if (g.Key == "Right")
					cell.RightUtilityButtons = g.ToArray ();
			}
			return true;
		}
	}
}

