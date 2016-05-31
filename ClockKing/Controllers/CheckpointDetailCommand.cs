﻿using System;
using Foundation;
using UIKit;
using ClockKing.Model;
using MonoTouch.Dialog;
using System.Linq;
using System.Collections.Generic;
using Humanizer;

namespace ClockKing
{
	public class CheckpointDetailCommand
	{
		protected CheckPointController Controller{ get; set;}
		protected CheckPoint LastCheckpointDetailed { get; set;}

		public CheckpointDetailCommand (CheckPointController controller)
		{
			this.Controller = controller;
		}

		public UIViewController GetDetailDialog(CheckPoint Data)
		{
			var checkpoint = Data;

			this.LastCheckpointDetailed = checkpoint;

			var root = new RootElement (string.Format ("{0}", checkpoint.Name));
			root.Add(GetDetailSections(Data));

			return new CheckPointDetailViewController (this.Controller,Data, root);
		}

		public Section[] GetDetailSections(CheckPoint checkpoint)
		{
			var distinctTimes = checkpoint.Occurrences.Select (o => o.Time).Distinct();

			var sectionsToReturn = new List<Section> ();

			var timingSection = new Section ("Stats:");
			sectionsToReturn.Add (timingSection);

			timingSection.Add (new StringElement ("count",
				checkpoint.Occurrences.Count().ToString()));
			timingSection.Add (new StringElement("average", 
				(DateTime.Now.Date+ checkpoint.averageObservedTime).ToString("t")));
			timingSection.Add (new StringElement("next",
				"in " + checkpoint.UntilNextTargetTime.Humanize(2)));

			if (checkpoint.Occurrences.Any ()) 
			{
				timingSection.Add (new StringElement ("earliest",
					(DateTime.Today+ distinctTimes.OrderBy (o => o.TotalMinutes).First ()).ToString ("t")));
				timingSection.Add (new StringElement ("latest",
					(DateTime.Today+ distinctTimes.OrderByDescending (o => o.TotalMinutes).First ()).ToString ("t")));
				timingSection.Add (new StringElement ("since most recent",
					checkpoint.SinceLastOccurrence.Humanize(1)+" ago"));

				var detailsSection = new Section("Occurrence History:");

				detailsSection.AddAll (
					checkpoint
					.Occurrences
					.OrderByDescending(o=>o.timeStamp)
					.Select (o => new StringElement (o.timeStamp.ToString ("d"), o.timeStamp.ToString ("t"))));

				sectionsToReturn.Add (detailsSection);
			}

			return sectionsToReturn.ToArray ();
		}

		public void ShowDetailDialog(CheckPoint Data)
		{
			ShowDetailDialog (GetDetailDialog (Data), Data);
		}

		public void ShowDetailDialog(UIViewController dialog,CheckPoint Data=null)
		{
			if (Data == null)
				Data = LastCheckpointDetailed;
			
			this.Controller.NavigationController.PushViewController (dialog,true);

			dialog.NavigationItem.SetLeftBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Done,
				(s, e) => this.Controller.NavigationController.PopViewController (true)
			), true);

			CreateOptions (dialog, Data);
		}
			
		public void CreateOptions(UIViewController dialog,CheckPoint Data)
		{
			var acs = UIAlertController.Create (string.Format("options for {0}",Data.Name), "stuff to do", UIAlertControllerStyle.ActionSheet);

			var handler = new Action<Command> ((c) => 
				{
					if(c.ExecuteFor(this.Controller,Data))
					{
						this.Controller.ConditionallyRefreshData();
						CreateOptions(this.Controller,Data);
					}
				});

			this.Controller.Commands.GetAlertActionsForCheckpoint(Data,handler).ToList().ForEach(cmd=>acs.AddAction (cmd));

			acs.AddAction(UIAlertAction.Create("Nevermind!",UIAlertActionStyle.Cancel,null));
							
			dialog.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Edit,
				(s, e) => this.Controller.PresentViewController(acs,true,null)
			), true);
		}
	}
}