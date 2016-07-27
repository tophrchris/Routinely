using System;
using ClockKing.Core;
using UIKit;
using MonoTouch.Dialog;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ClockKing
{
	

	public class CheckPointManager:iCheckpointCommandController,iNavigatableDialog
	{
		private CheckPointController Controller{ get; set; }
		private AppDelegate appDelegate{ get; }
		private DataModel CheckPointData	{ get{return this.appDelegate.CheckPointData; }}

		public event EventHandler<CheckPointDataChangedEventArgs> CheckPointDataChanged;

		private void DataChanged(CheckPointDataChangedEventArgs args)
		{
			var handler = CheckPointDataChanged;
			if (handler != null)
				handler (this, args);	
		}

		public CheckPointManager(CheckPointController controller)
		{
			this.Controller = controller;
			this.appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
		}

		public void ResetNavigation (bool refreshData=false)
		{
			Debug.WriteLine("cpm reset nav");
			this.Controller.ResetNavigation (refreshData);
		}
			
		public CheckPoint AddNewCheckPoint(string title, TimeSpan target,string emoji,string category="")
		{
			if (string.IsNullOrEmpty (emoji))
				emoji = title.Substring (0, 2);

			var created = this.CheckPointData.AddNewCheckPoint (title, target,emoji,category);
			if (created != null)
				DataChanged (new CheckPointDataChangedEventArgs ()
					{Entity="Goal",
						ActionOccurred=ActionType.Added,
						ConditionallyRefreshData=true});

			return created;
		}

		public Occurrence AddOccurrenceToCheckPoint(string checkPointName, int mins)
		{
			var found = this.CheckPointData.checkPoints [checkPointName];
			return this.AddOccurrenceToCheckPoint (found, mins);
		}

		public Occurrence AddOccurrenceToCheckPoint(CheckPoint checkPoint,int mins)
		{
			return AddOccurrenceToCheckPoint(checkPoint, DateTime.Now.ToLocalTime ().AddMinutes (mins));
		}

		public Occurrence AddOccurrenceToCheckPoint(CheckPoint checkPoint,DateTime when)
		{
			var o = checkPoint.CreateOccurrence(when.ToLocalTime());
			checkPoint.AddOccurrence (o);
			Debug.WriteLine("cpm add occurrence to checkpoint");
			this.CheckPointData.SaveOccurrence (o);
			DataChanged (new CheckPointDataChangedEventArgs ()
				{ Entity = "Occurrence",
				ActionOccurred = ActionType.Added,
				ConditionallyRefreshData=true
				});
			return o;
		}

		public bool CheckPointExists(string checkPointName)
		{
			return this.CheckPointData.checkPoints.ContainsKey (checkPointName);
		}

		public bool RemoveCheckpoint(CheckPoint toDelete)
		{
			var deleted =  this.CheckPointData.RemoveCheckPoint (toDelete);

			if (deleted) { 
				DataChanged (new CheckPointDataChangedEventArgs () 
					{Entity = "Goal",
					ActionOccurred = ActionType.Deleted});
				this.ResetNavigation ();
				
			} else
				DataChanged (new CheckPointDataChangedEventArgs ()
					{Entity = "Goal",
					ActionOccurred = ActionType.Deleted,
					Result = ResultType.Failure});
			return deleted;
		}
			
		public void ResaveCheckpoints()
		{
			var saved = this.CheckPointData.SaveCheckPoints ();
			if (saved) 
			{
				DataChanged (new CheckPointDataChangedEventArgs ()
					{Entity = "Goals",
					ActionOccurred = ActionType.Written});
			}
		}

		public void RewriteOccurrences()
		{
			this.CheckPointData.SaveOccurrences ();
			DataChanged (new CheckPointDataChangedEventArgs () 
				{Entity = "Ouccrrences",
				ActionOccurred = ActionType.Written,
				ConditionallyRefreshData=true,
				RespondToModelChanges=false});
		}








		public void PresentChoices (string Title, string Instructions, IEnumerable<ModalChoice> choices)
		{
			var modal = new ModalChoices ();
			modal.Title = Title;
			modal.Instructions = Instructions;
			modal.Choices = choices;
			modal.Controller = this.Controller;
			modal.Display ();
		}

		public void PresentConfirmationDialog (Action handler, string Title, string Message, string yes, string no, bool YesIsDestructive)
		{
			var d = SharedDialogs.ConfirmationDialog ((a) => handler (), Title, Message, yes, no, YesIsDestructive);
			this.Controller.PresentModalViewController (d, true);
		}

		public void NavigateToDialog (iNavigatableDialog dialog)
		{
			var dvc = dialog as MonoTouch.Dialog.DialogViewController;
			this.Controller.NavigationController.PushViewController (dvc, true);
		}

		public void PresentHistoricOccurrenceDialogFor (CheckPoint checkpoint)
		{
			var custom = new AddHistoricInstanceDialog(this,new MonoTouch.Dialog.RootElement("Add Occurrence"),checkpoint,true);
			this.NavigateToDialog(custom);
		}

		public void PresentEditDialogFor (CheckPoint checkpoint)
		{
			var editDialog = new CheckPointEditingDialog(this,new RootElement("Edit"),true);
			editDialog.RenderForCheckPoint(checkpoint);
			this.NavigateToDialog (editDialog);
		}

		public void InjectEditDialogIntoExistingDialogFor (CheckPoint checkpoint, iNavigatableDialog ExistingDialog)
		{
			var editDialog = new CheckPointEditingDialog(this,new RootElement("Edit"),true);
			editDialog.RenderForCheckPoint(checkpoint);

			var existing = ExistingDialog as DialogViewController;
			existing.Root.Insert (0,editDialog.Root.First());

			existing.NavigationItem.SetLeftBarButtonItem(
				new UIBarButtonItem(UIBarButtonSystemItem.Cancel,
					(s,e)=>
					{
						UIView.Animate(.25d,()=> existing.Root.RemoveAt(0));
						ExistingDialog.ResetNavigation();
					}),true);

			existing.NavigationItem.SetRightBarButtonItem 
			(editDialog.NavigationItem.RightBarButtonItem,true);
		}

		public void PresentScheduledTargetDialogForCheckpoint (CheckPoint checkpoint, iNavigatableDialog dialog)
		{
			var e = dialog as CheckPointDetailDialog;
			var n = checkpoint.AddScheduledtarget(null,new List<DayOfWeek>(){DateTime.Today.DayOfWeek});
			var d = new ScheduledTargetDialog(new RootElement("Scheduled Target"),n,checkpoint,this,e);
			this.NavigateToDialog (d);
		}

		public void PresentCheckPointActionsFor(CheckPoint checkpoint, iNavigatableDialog existing)
		{
			var dialog = existing as CheckPointDetailDialog;//TODO: i hate this

			var handler = new Action<Command> ((c) => 
				{
					if(c.ExecuteFor(this,checkpoint))
					{
						if(c.ChangesCheckpoint)
							this.ResaveCheckpoints();
						dialog.RespondToChanges(false);
					}
				});

			var acs = UIAlertController.Create (string.Format("options for {0}",checkpoint.Name), "stuff to do", UIAlertControllerStyle.ActionSheet);

			this.Controller.Commands.GetAlertActionsForCheckpoint(checkpoint,handler,dialog)
				.ToList()
				.ForEach(cmd=>acs.AddAction (cmd));

			acs.AddAction(UIAlertAction.Create("Nevermind!",UIAlertActionStyle.Cancel,null));

			dialog.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem ("Actions",UIBarButtonItemStyle.Plain,
				(s, e) => 
				this.Controller.PresentViewController(acs,true,null)
			), true);
		}
	}
}

