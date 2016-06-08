using System;
using Foundation;
using UIKit;
using ClockKing.Core;
using MonoTouch.Dialog;
using System.Linq;
using System.Collections.Generic;
using Humanizer;
using ClockKing.Extensions;

namespace ClockKing
{

	public class CheckPointDetailDialog:DialogViewController
	{

		private List<UIPreviewAction> actions { get; set;}
		private CheckPointController Controller{ get; set;}
		private CheckPoint toDetail { get; set;}

		public CheckPointDetailDialog(UIViewController Parent,CheckPoint toDetail,RootElement root):base(root)
		{
			var parent = Parent as CheckPointController;
			this.Controller = parent;
			this.toDetail = toDetail;
			this.Render ();

		}

		public void RespondToChanges()
		{
			this.Render();
			this.Controller.ConditionallyRefreshData (true);
			CreateOptions (this,toDetail);
		}

		protected void Render()
		{
			var sections = GetDetailSections (toDetail, Controller,this);

			if (!Root.Any ())
				Root.Add (sections);
			else
			{
				var headers = sections.Select (s => s.Header);
				var toRemove = Root.Where (s => headers.Contains (s.Header));
				toRemove.ToList ().ForEach (r => Root.Remove (r,UITableViewRowAnimation.Right));
				Root.Insert (Root.Count (), UITableViewRowAnimation.Left, sections);
			}


			var executor = new Action<Command>((ub)=>
				{
					if(ub.ExecuteFor(this.Controller,toDetail))
						RespondToChanges();
				});

			this.actions = this.Controller.Commands.GetPreviewActionsForCheckpoint (toDetail, executor).ToList();

		}

		public override void ViewDidLoad ()
		{
			ResetNavigation ();
		}

		public void ResetNavigation()
		{
			this.NavigationItem.SetLeftBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Done,
				(s, e) => this.Controller.ResetNavigation(true)
			), true);

			CreateOptions (this, toDetail);
		}


		public void CreateOptions(UIViewController dialog,CheckPoint Data)
		{
			var acs = UIAlertController.Create (string.Format("options for {0}",Data.Name), "stuff to do", UIAlertControllerStyle.ActionSheet);

			var handler = new Action<Command> ((c) => 
				{
					if(c.ExecuteFor(this.Controller,Data))
						RespondToChanges();
				});

			acs.AddAction (new InPlaceEditCheckPointCommand (this).AsAlertAction (handler));

			this.Controller.Commands.GetAlertActionsForCheckpoint(Data,handler)
				.Where(a=>a.Title!="Edit Goal")
				.ToList()
				.ForEach(cmd=>acs.AddAction (cmd));

			acs.AddAction(UIAlertAction.Create("Add Alternative Target",UIAlertActionStyle.Default,
				(a)=>
				{
					var n = Data.AddScheduledtarget(null,new List<DayOfWeek>(){DateTime.Today.DayOfWeek});
					var d = new ScheduledTargetDialog(new RootElement("Scheduled Target"),n,Data,this.Controller,this);
					this.NavigationController.PushViewController(d,true);
				}));

			acs.AddAction(UIAlertAction.Create("Nevermind!",UIAlertActionStyle.Cancel,null));

			dialog.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Edit,
				(s, e) => 
				this.Controller.PresentViewController(acs,true,null)
			), true);
		}


		public static Section[] GetDetailSections(CheckPoint checkpoint,CheckPointController Controller,CheckPointDetailDialog dialog=null)
		{
			var distinctTimes = checkpoint.Occurrences.Select (o => o.Time).Distinct();

			var sectionsToReturn = new List<Section> ();

			var tableCell = new CheckPointElement (checkpoint,Controller);

			var cellHolder = new Section ("Goal:"){ tableCell };
 		
			var Stats = new Section ("Stats:");
			Stats.Add (new StringElement ("Enabled?", checkpoint.Enabled?"Yes":"No"));

			Action reloader = () => UIView.Animate(.01d,()=>dialog.ReloadData());

			var createdElement = new ToggledStringElement ("Created");
			createdElement.PrimaryGenerator = () => checkpoint.CreatedOn.Humanize ();
			createdElement.SecondaryGenerator = () => checkpoint.CreatedOn.ToString ("G");
			createdElement.Tapped += reloader;
			createdElement.Toggle ();
			Stats.Add (createdElement);

			var nextElement = new ToggledStringElement ("Next");
			nextElement.PrimaryGenerator = () => checkpoint.UntilNextTargetTime.Humanize (2);
			nextElement.SecondaryGenerator = () => checkpoint.UntilNextTargetTime.ToAMPMString();
			nextElement.Tapped += reloader;
			nextElement.Toggle ();
			Stats.Add (nextElement);

			sectionsToReturn.Add (cellHolder);
			sectionsToReturn.Add (Stats);

			if (checkpoint.ScheduledTargets.Any ()) 
			{
				Stats.Add (new StringElement ("Target", (DateTime.Today+ checkpoint.TargetTime).ToString ("t")));
				var alts = new Section ("Alternative Targets");

				foreach (var at in checkpoint.ScheduledTargets) 
				{
					var maxDayLength = (at.ApplicableDays.Count () > 3) ? 3 : 10;	
					var se = new StringElement( string.Join(", ",
						at.ApplicableDays.Select(t=>t.ToString().Truncate(maxDayLength,"")).ToArray()),
						at.TargetTime.HasValue?
						(DateTime.Today+ at.TargetTime.Value).ToString("t"):"Inactive");
					se.Tapped += () => 
					{
						var root = new RootElement("Scheduled Target");
						var d = new ScheduledTargetDialog(root,at,checkpoint,Controller,dialog);
						dialog.NavigationController.PushViewController(d,true);	
					};
					alts.Add (se);
				}
				sectionsToReturn.Add (alts);
			}
				
			if (checkpoint.Occurrences.Any ()) 
			{
			
				var earliest = new ToggledStringElement ("Earliest");
				earliest.PrimaryGenerator = () => checkpoint.Earliest.Time.ToAMPMString ();
				earliest.SecondaryGenerator = () => checkpoint.Earliest.TimeStamp.ToString ("G");

				var latest = new ToggledStringElement ("Latest");
				latest.PrimaryGenerator = () => checkpoint.Latest.Time.ToAMPMString ();
				latest.SecondaryGenerator = () => checkpoint.Latest.TimeStamp.ToString ("G");

				var mostRecent = new ToggledStringElement ("Since Most Recent");
				mostRecent.PrimaryGenerator = () => checkpoint.SinceLastOccurrence.Humanize (1)+" ago";
				mostRecent.SecondaryGenerator = () => checkpoint.MostRecentOccurrenceTimeStamp().ToString("G");

				foreach (var el in new[]{earliest,latest,mostRecent})
				{
					el.Tapped += reloader;
					el.Toggle ();
					Stats.Add (el);
				}
					
				var detailsSection = new Section("Occurrence History:");

				detailsSection.AddAll (
					checkpoint
					.Occurrences
					.OrderByDescending(o=>o.TimeStamp)
					.Select (o => new StringElement (o.Date.ToString("d"), o.TimeStamp.ToString ("t"))));

				sectionsToReturn.Add (detailsSection);
			}

			return sectionsToReturn.ToArray ();
		}

		public override IUIPreviewActionItem[] PreviewActionItems 
		{
			get {
				return this.actions.ToArray();
			}
		}
	}
}

