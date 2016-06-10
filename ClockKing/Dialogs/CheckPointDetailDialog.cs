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
		public DialogViewController moreDialog { get; set;}

		public CheckPointDetailDialog(UIViewController Parent,CheckPoint toDetail,RootElement root):base(root)
		{
			var parent = Parent as CheckPointController;
			this.Controller = parent;
			this.toDetail = toDetail;
			this.Render ();

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


		public void RespondToChanges(bool condition=false)
		{
			this.Render();
			this.Controller.ConditionallyRefreshData (condition);
			CreateOptions (this,toDetail);
		}

		public void Render()
		{
			var sections = GetDetailSections ();

			if (!Root.Any ())
				Root.Add (sections);
			else
			{
				var headers = sections.Select (s => s.Header);
				var toRemove = Root.Where (s => headers.Contains (s.Header));
				toRemove.ToList ().ForEach (r => Root.Remove (r,UITableViewRowAnimation.Right));
				Root.Insert (Root.Count (), UITableViewRowAnimation.Left, sections);
			}
			AttachPreviewActions ();
			if (this.moreDialog != null)
				this.moreDialog.ReloadData ();
		
		}

		public void CreateOptions(UIViewController dialog,CheckPoint Data)
		{
			var handler = new Action<Command> ((c) => 
				{
					if(c.ExecuteFor(this.Controller,Data))
					{
						if(c.ChangesCheckpoint)
							Controller.ResaveCheckpoints();
						RespondToChanges(false);
					}
				});
			
			var acs = UIAlertController.Create (string.Format("options for {0}",Data.Name), "stuff to do", UIAlertControllerStyle.ActionSheet);
					
			this.Controller.Commands.GetAlertActionsForCheckpoint(Data,handler,this)
				.Where(a=>a.Title!="Edit Goal")//TODO:i hate this
				.ToList()
				.ForEach(cmd=>acs.AddAction (cmd));

			acs.AddAction(UIAlertAction.Create("Nevermind!",UIAlertActionStyle.Cancel,null));

			dialog.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem ("Actions",UIBarButtonItemStyle.Plain,
				(s, e) => 
				this.Controller.PresentViewController(acs,true,null)
			), true);
		}


		public Section[] GetDetailSections()
		{
			var sectionsToReturn = new List<Section> ();

			sectionsToReturn.Add (new CheckPointCellSection (Controller,toDetail));
			sectionsToReturn.Add (new CheckPointStatsSection (toDetail,()=>this.ReloadData()));

			if (toDetail.ScheduledTargets.Any ())
				sectionsToReturn.Add (new AlternativeTargetsSection (toDetail, Controller, this));
				
			if (toDetail.Occurrences.Any ()) 
				sectionsToReturn.Add (new OccurrencesSection(toDetail,Controller,this));

			return sectionsToReturn.ToArray ();
		}


		#region preview actions
		protected void AttachPreviewActions()
		{
			var executor = new Action<Command>((ub)=>
				{
					if(ub.ExecuteFor(this.Controller,toDetail))
					{
						if(ub.ChangesCheckpoint)
							Controller.ResaveCheckpoints();
						RespondToChanges(false);
					}
				});

			this.actions = this.Controller.Commands.GetPreviewActionsForCheckpoint (toDetail, executor).ToList();

		}

		public override IUIPreviewActionItem[] PreviewActionItems 
		{
			get {
				return this.actions.ToArray();
			}
		}
		#endregion
	}
}

