using System;
using Foundation;
using UIKit;
using ClockKing.Core;
using MonoTouch.Dialog;
using System.Linq;
using System.Collections.Generic;
using Humanizer;
using ClockKing.Extensions;
using System.Diagnostics;


namespace ClockKing
{

	public class CheckPointDetailDialog:DialogViewController,iNavigatableDialog
	{

		private List<UIPreviewAction> actions { get; set;}
		private iCheckpointCommandController CheckPoints{ get; set;}
		private CheckPointController Controller{ get; set;}
		private CheckPoint toDetail { get; set;}
		public DialogViewController moreDialog { get; set;}

		public CheckPointDetailDialog(iCheckpointCommandController checkPoints,CheckPoint toDetail,RootElement root):base(root)
		{
			this.CheckPoints = checkPoints;
			this.Controller = ((AppDelegate)UIApplication.SharedApplication.Delegate).Controller;
			this.toDetail = toDetail;
			this.TableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.OnDrag;
			this.Render ();


		}

		public override void ViewDidLoad ()
		{
			this.ResetNavigation ();
		}

		public void ResetNavigation(bool refreshData=false)
		{
			Debug.WriteLine("cdd rn");
			this.NavigationItem.SetLeftBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Done,
				(s, e) => ((iNavigatableDialog)this.CheckPoints).ResetNavigation(true)
			), true);

			CreateOptions (this, toDetail);
		}


		public void RespondToChanges(bool condition=false)
		{
			Debug.WriteLine("cdd rtc");
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
			this.CheckPoints.PresentCheckPointActionsFor(Data,(iNavigatableDialog)dialog);
		}


		public Section[] GetDetailSections()
		{
			var sectionsToReturn = new List<Section> ();
		
			sectionsToReturn.Add (new CheckPointCellSection (toDetail));
			sectionsToReturn.Add (new CheckPointStatsSection (toDetail,()=>this.ReloadData()));

			if (toDetail.ScheduledTargets.Any ())
				sectionsToReturn.Add (new AlternativeTargetsSection (toDetail,this.CheckPoints, this));
				
			if (toDetail.Occurrences.Any ()) 
				sectionsToReturn.Add (new OccurrencesSection(toDetail,this.CheckPoints,this));

			return sectionsToReturn.ToArray ();
		}


		#region preview actions
		protected void AttachPreviewActions()
		{
				var executor = new Action<Command>((ub)=>
				{
					if(ub.ExecuteFor(this.CheckPoints,toDetail))
					{
						if(ub.ChangesCheckpoint)
							CheckPoints.ResaveCheckpoints();
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