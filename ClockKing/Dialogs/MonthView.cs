using System;
using MonoTouch.Dialog;
using UIKit;
using Factorymind.Components;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using System.Diagnostics;


namespace ClockKing
{
	public class MonthView : DialogViewController
	{
		private CheckPointController controller;
		private DataModel Data;
		private Dictionary<DateTime, IEnumerable<Occurrence>> occurrencesByDate { get; set; }
		private Section occurrences;
		private FMCalendar calendar;

		public MonthView() : base(UITableViewStyle.Grouped, null)
		{

			var a = UIApplication.SharedApplication.Delegate as AppDelegate;
			controller = a.Controller;
			occurrencesByDate = new Dictionary<DateTime, IEnumerable<Occurrence>>();

			Data = a.CheckPointData;

			PopulateOccurrences();

			this.calendar = CreateCalendar();
			this.occurrences = new Section("occurrences");

			var c = new UIViewElement("",this.calendar,false);

			Root = new RootElement("History") {
				new Section ("") {c},occurrences
			};

			this.NavigationItem.SetLeftBarButtonItem(
				new UIBarButtonItem(UIBarButtonSystemItem.Done,
				                    (s, e) =>this.NavigationController.PopViewController(true)), true);	

		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			System.Diagnostics.Debug.WriteLine("month vdl");
			PopulateOccurrences();
			if (this.calendar != null)
				ShowOccurrencesforDay(this.calendar.CurrentSelectedDate);
			this.ReloadData();
		}


		private FMCalendar CreateCalendar()
		{
			var cal = new FMCalendar(
				new CoreGraphics.CGRect(View.Bounds.Location,
				                        new CoreGraphics.CGSize(View.Bounds.Width,
				                                                View.Bounds.Height*.4f)
				                       ));

			View.BackgroundColor = UIColor.White;
			cal.SelectionColor = UIColor.Blue;
			cal.TodayCircleColor = UIColor.Red;

			cal.MonthFormatString = "MMMM yyyy";
			cal.SundayFirst = true;

			cal.IsDayMarkedDelegate = (date) =>occurrencesByDate.ContainsKey(date);
			cal.IsDateAvailable = (date) =>occurrencesByDate.ContainsKey(date.Date);
			cal.DateSelected += (date) =>ShowOccurrencesforDay(date);

			return cal;
		}

		//TODO: move this to DataModel?
		private void PopulateOccurrences()
		{
			occurrencesByDate.Clear();
			var f = from cp in Data.checkPoints
					from o in cp.Value.Occurrences
					group o by o.Date.Date into byDate
					select byDate;

			foreach (var g in f)
				occurrencesByDate.Add(g.Key, g.AsEnumerable());
		}

		private void ShowOccurrencesforDay(DateTime date)
		{
			occurrences.Clear();

			occurrences.Caption = "Goals Completed on {0}".FormatWith(date.ToString("D"));

			if (occurrencesByDate.ContainsKey(date))
			{
				occurrences.AddAll(
					occurrencesByDate[date]
					.OrderBy(o => o.TimeStamp)
					.Select((Occurrence arg) =>
					        new StringElement("{0} {1}".FormatWith(arg.CheckPoint.Emoji,arg.CheckPoint.Name),
							() => controller.ShowDetailDialogFor(arg.CheckPoint))
							{
								Value= arg.TimeStamp.ToString("t")
							}

					)
				);

			}
		}

	}
}
