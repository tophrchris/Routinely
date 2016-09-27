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

		public CheckPoint AddNewCheckPoint(string title, TimeSpan target, string emoji, string category = "")
		{
			if (string.IsNullOrEmpty(emoji))
				emoji = title.Substring(0, 2);

			CheckPoint created = null;
			using (new TrackingBenchmark() { Category = "DataModel", Name = "AddGoal" })
				created = this.CheckPointData.AddNewCheckPoint (title, target,emoji,category);

			if (created != null)
			{
				DataChanged(new CheckPointDataChangedEventArgs()
				{
					Entity = "Goal",
					ActionOccurred = ActionType.Added,
					ConditionallyRefreshData = true
				});
				appDelegate.Track("Dialog", "save", created.Name);
			}

			return created;
		}

		public CheckPoint AddNewCheckPoint(CheckPoint toAdd)
		{
			if (string.IsNullOrEmpty(toAdd.Emoji))
				toAdd.Emoji = toAdd.Name.Substring(0, 2);

			CheckPoint added = null;
			using (new TrackingBenchmark() { Category = "DataModel", Name = "AddGoal" })
				added = this.CheckPointData.AddNewCheckPoint(toAdd);
			
			if (added != null)
			{
				DataChanged(new CheckPointDataChangedEventArgs()
				{
					Entity = "Goal",
					ActionOccurred = ActionType.Added,
					ConditionallyRefreshData = true
				});
				appDelegate.Track("Dialog", "save(adapted)", toAdd.Name);
			}

			return added;
		}

		public Occurrence SkipCheckpoint(CheckPoint checkpoint)
		{
			var o = checkpoint.CreateOccurrence();
			o.IsSkipped = true;
			checkpoint.AddOccurrence(o);
			using (new TrackingBenchmark() { Category = "DataModel", Name = "SaveOccurrence" })
				this.CheckPointData.SaveOccurrence(o);
			NotificationManager.PresentMotivationalNotification(checkpoint);
			DataChanged(new CheckPointDataChangedEventArgs()
			{
				Entity = "Skip",
				ActionOccurred = ActionType.Added
			});
			HapticsManager.Denounce();
			return o;
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
			using (new TrackingBenchmark() { Category = "DataModel", Name = "SaveOccurrence" })
				this.CheckPointData.SaveOccurrence (o);
			NotificationManager.PresentMotivationalNotification(checkPoint);
			DataChanged (new CheckPointDataChangedEventArgs ()
				{ Entity = "Completion",
				ActionOccurred = ActionType.Added
				});
			HapticsManager.Celebrate();
			return o;
		}

		public bool CheckPointExists(string checkPointName)
		{
			return this.CheckPointData.checkPoints.ContainsKey (checkPointName);
		}

		public bool CheckPointExists(Guid checkPointGuid)
		{
			return this.CheckPointData.checkPoints.Any(cp => cp.Value.UniqueIdentifier == checkPointGuid);
		}

		public bool RemoveCheckpoint(CheckPoint toDelete)
		{
			bool deleted = false;

			using (new TrackingBenchmark() { Category = "DataModel", Name = "RemoveGoal" })
				deleted = this.CheckPointData.RemoveCheckPoint (toDelete);

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
			var saved = false;
			using (new TrackingBenchmark() { Category = "DataModel", Name = "SaveGoals" })
				saved=this.CheckPointData.SaveCheckPoints ();
			
			if (saved) 
			{
				DataChanged (new CheckPointDataChangedEventArgs ()
					{Entity = "Goals",
					ActionOccurred = ActionType.Written});
				HapticsManager.ChangeCompleted();
			}
		}

		public void RewriteOccurrences()
		{
			using (new TrackingBenchmark() { Category = "DataModel", Name = "rewriteOccurrences" })
				this.CheckPointData.SaveOccurrences ();
			DataChanged (new CheckPointDataChangedEventArgs () 
				{Entity = "Completions",
				ActionOccurred = ActionType.Written});
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
			HapticsManager.Warn();
		}

		public void NavigateToDialog (iNavigatableDialog dialog)
		{
			var dvc = dialog as MonoTouch.Dialog.DialogViewController;
			this.Controller.NavigationController.PushViewController (dvc, true);
			HapticsManager.NavigationCompleted();
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

		public void PresentRelativeTargetDialogForCheckpoint(CheckPoint checkpoint, iNavigatableDialog dialog)
		{
			var d = new RelativeTargetDialog(checkpoint);
			this.NavigateToDialog(d);
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
			  {
				  HapticsManager.Trigger();
				this.Controller.PresentViewController(acs,true,null);
			}
			), true);
		}
	}
}

