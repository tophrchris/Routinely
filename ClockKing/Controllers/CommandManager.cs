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
			var cmds = CreateCommands ();
			Commands = cmds.ToDictionary (u => u.Name);
		}

		private List<Command> CreateCommands(){
			return new List<Command> () {
				new EditCheckPointCommand(),
				new InPlaceEditCheckPointCommand(),
				new EnableCheckPointCommand(),
				new DisableCheckPointCommand(),
				new AddOccurrenceCommand(),
				new AddHistoricOccurrenceCommand(),
				new DeleteCheckPointCommand(),
				new AddScheduledTargetCommand()
			};
		}


		public IEnumerable<UIAlertAction> GetAlertActionsForCheckpoint(CheckPoint selected,Action<Command> handler,UIViewController dialog=null)
		{
			return this.Commands.Values
				.Where (c => c.ShouldDecorate (selected))
				.Select(c=>{
					if(c is IDialogBoundCommand  && dialog!=null)
						((IDialogBoundCommand)c).ExistingDialog=(DialogViewController)dialog;
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

	public static class CommandExtensions
	{
		public static UIAlertAction AsAlertAction(this Command cmd, Action<Command> handler)
		{

			var title = string.IsNullOrEmpty(cmd.LongName) ? cmd.Name : cmd.LongName;

			return UIAlertAction.Create(
				title,
				cmd.IsDestructive?UIAlertActionStyle.Destructive:UIAlertActionStyle.Default,
				(a)=>handler(cmd));
		}

		public static UIPreviewAction AsPreviewAction(this Command cmd, Action<Command> handler,UIPreviewActionStyle style)
		{
			var title = string.IsNullOrEmpty(cmd.LongName) ? cmd.Name : cmd.LongName;

			return UIPreviewAction.Create (title,style,(a,c)=>{handler(cmd);});
		}

		public static UIPreviewAction AsPreviewAction(this Command cmd,Action<Command> handler )
		{
			return cmd.AsPreviewAction (handler, 
				(cmd.IsDestructive)?
				UIPreviewActionStyle.Destructive:
				UIPreviewActionStyle.Default);	
		}
		public static UIButton AsButton(this Command cmd)
		{
			var button = new UIButton (UIButtonType.Custom);
			button.BackgroundColor = cmd.Color;
			button.SetTitle (cmd.Name, UIControlState.Normal);
			button.SetTitleColor (UIColor.White, UIControlState.Normal);
			button.TitleLabel.AdjustsFontSizeToFitWidth = true;
			return button;

		}
	}
}

