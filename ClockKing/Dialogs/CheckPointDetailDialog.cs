using System;
using Foundation;
using UIKit;
using ClockKing.Core;
using MonoTouch.Dialog;
using System.Linq;
using System.Collections.Generic;
using Humanizer;

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
				(s, e) => this.Controller.ResetNavigation()
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

			acs.AddAction(UIAlertAction.Create("add Alternative Target",UIAlertActionStyle.Default,
				(a)=>
				{
					var n = Data.AddAlternativeTarget(null,new List<DayOfWeek>(){DateTime.Today.DayOfWeek});
					var d = GetDialogForAlternativeTarget(n,Data,this.Controller,this);
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

			var createdElement = new StringElement ("Created:");

			var CreatedOnEnumerator = GetNextCreatedOnText (checkpoint).GetEnumerator();

			CreatedOnEnumerator.MoveNext ();

			createdElement.Value = CreatedOnEnumerator.Current;


			createdElement.Tapped+=()=> {
				CreatedOnEnumerator.MoveNext();
				var found = CreatedOnEnumerator.Current;
				createdElement.Value=found;
				dialog.ReloadData();
			};

			Stats.Add (createdElement);
			Stats.Add (new StringElement("next",
				"in " + checkpoint.UntilNextTargetTime.Humanize(2)));
			

			sectionsToReturn.Add (cellHolder);
			sectionsToReturn.Add (Stats);

			if (checkpoint.TargetTimeAlternatives.Any ()) 
			{
				Stats.Add (new StringElement ("Target", (DateTime.Today+ checkpoint.TargetTime).ToString ("t")));
				var alts = new Section ("Alternative Targets");

				foreach (var at in checkpoint.TargetTimeAlternatives) 
				{
					var se = new StringElement( string.Join(", ",
						at.ApplicableDays.Select(t=>t.ToString()).ToArray()),
						at.TargetTime.HasValue?
						(DateTime.Today+ at.TargetTime.Value).ToString("t"):"Inactive");
					se.Tapped += () => 
					{
						var d = GetDialogForAlternativeTarget(at,checkpoint,Controller,dialog);
						dialog.NavigationController.PushViewController(d,true);	
					};
					alts.Add (se);
				}
				sectionsToReturn.Add (alts);
			}
				
			if (checkpoint.Occurrences.Any ()) 
			{
				Stats.Add (new StringElement ("earliest",
					(DateTime.Today+ distinctTimes.OrderBy (o => o.TotalMinutes).First ()).ToString ("t")));
				Stats.Add (new StringElement ("latest",
					(DateTime.Today+ distinctTimes.OrderByDescending (o => o.TotalMinutes).First ()).ToString ("t")));
				Stats.Add (new StringElement ("since most recent",
					checkpoint.SinceLastOccurrence.Humanize(1)+" ago"));

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

		public static DialogViewController GetDialogForAlternativeTarget(ScheduledTargetTime at,CheckPoint checkpoint, CheckPointController Controller,CheckPointDetailDialog dialog)
		{
			var r = new RootElement("Alternative Targets");
			var d = new DialogViewController(r);

			r.Add (new Section ("Goal"){ new CheckPointElement (checkpoint,Controller)});

			var s = new Section("alternate Target");
			var t = new TimeElement("time",DateTime.Today+ (at.TargetTime??DateTime.Now.TimeOfDay));

			var inactiveSwitch = new BooleanElement("Goal is inactive on these days?",!at.TargetTime.HasValue);
			inactiveSwitch.ValueChanged+=(so,ev)=>
			{
				if(inactiveSwitch.Value){
					s.Remove(t);
				}else{
					s.Insert(1,t);
				}
			};

			s.Add(inactiveSwitch);
			if(at.TargetTime.HasValue)
				s.Add(t);

			var days = new Section("days");
			for(var i =0;i<=6;i++)
				days.Add(new CheckboxElement(((DayOfWeek)i).ToString(),
					at.ApplicableDays.Contains(((DayOfWeek)i))));

			r.Add(new Section[]{s,days});

			r.Add (new Section ("delete") {new StringElement ("delete",
				() => {
					checkpoint.RemoveAlternativeTarget (at);
					Controller.ResaveCheckpoints();
					dialog.RespondToChanges();
					d.DeactivateController(true);
				})
			});

			d.NavigationItem.SetRightBarButtonItem(
				new UIBarButtonItem(UIBarButtonSystemItem.Save,
					(so,ev)=>
					{
						if(inactiveSwitch.Value)
							at.TargetTime=null;
						else
							at.TargetTime=t.DateValue.ToLocalTime().TimeOfDay;

						at.ApplicableDays=
							days.Elements
								.Select((el,i)=>new{index=i,element=el})
								.Where(i=>((CheckboxElement)i.element).Value)
								.Select(i=>(DayOfWeek)i.index)
								.ToArray();

						Controller.ResaveCheckpoints();
						dialog.RespondToChanges();
						d.DeactivateController(true);
					}),true);

			d.NavigationItem.SetLeftBarButtonItem(
				new UIBarButtonItem(UIBarButtonSystemItem.Cancel,
					(so,ev)=>d.DeactivateController(true)),true);
			
			return d;
		}
			

		public override IUIPreviewActionItem[] PreviewActionItems 
		{
			get {
				return this.actions.ToArray();
			}
		}

		protected static IEnumerable<bool> ToggleCreatedOnText(StringElement e, CheckPoint checkpoint){
			foreach (var s in GetNextCreatedOnText(checkpoint)) {
				e.Value = s;
				yield return true;
			}

			yield break;
		}

		protected static IEnumerable<string> GetNextCreatedOnText(CheckPoint checkpoint)
		{
			var humanized = false;
			while (true) 
			{
				humanized = !humanized;
				if (humanized)
					yield return checkpoint.CreatedOn.Humanize ();
				else
					yield return checkpoint.CreatedOn.ToString ("G");
			}
			yield break;
		}
	}
}

