using System;
using MonoTouch.Dialog;
using UIKit;
using Factorymind.Components;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using System.Diagnostics;
using ClockKing.Extensions;


namespace ClockKing
{
	public class MonthView : CheckPointDialog
	{
		private DataModel Data { get { return this.App.CheckPointData;}}
		private Dictionary<DateTime, IEnumerable<Occurrence>> occurrencesByDate { get; set; }
		private Section Occurrences;
		private Section MissedGoals;
		private FMCalendar calendar;

		public MonthView() : base()
		{

			this.occurrencesByDate = PopulateOccurrences();


			this.Root = new RootElement("History");
			this.calendar = CreateCalendar();
			this.Occurrences = new Section("Completions");
			this.MissedGoals = new Section("Missed Goals");

			var c = new UIViewElement("", this.calendar, false);

			Root.Add(new Section("") { c });
			Root.Add(Occurrences);
			Root.Add(MissedGoals);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

		}

		public override void ViewDidAppear(bool animated)
		{
			this.App.LogActivity("History");
			base.ViewDidAppear(animated);
		
			System.Diagnostics.Debug.WriteLine("month vdl");



			this.occurrencesByDate = PopulateOccurrences();



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


			cal.IsDayMarkedDelegate = (date) => occurrencesByDate.ContainsKey(date);
			cal.IsDateAvailable = (date) => occurrencesByDate.ContainsKey(date.Date);
			cal.DateSelected += (date) => ShowOccurrencesforDay(date);


			return cal;
		}

		//TODO: move this to DataModel?
		private  Dictionary<DateTime, IEnumerable<Occurrence>> PopulateOccurrences()
		{
			var obd = new Dictionary<DateTime, IEnumerable<Occurrence>>();

			var f = from cp in Data.checkPoints
					from o in cp.Value.Occurrences
					group o by o.Date.Date into byDate
					select byDate;

			foreach (var g in f)
				obd.Add(g.Key, g.AsEnumerable());
			return obd;
		}

		private void ShowOccurrencesforDay(DateTime selectedDate)
		{
			Occurrences.Clear();
			MissedGoals.Clear();

			string newCaption;
			var dateString = selectedDate.ToString("D");

			if (occurrencesByDate.ContainsKey(selectedDate))
				newCaption = "Goals Completed on {0}".FormatWith(dateString);
			else
				newCaption = string.Empty;

			Occurrences.Caption = newCaption;



			if (occurrencesByDate.ContainsKey(selectedDate))
			{
				Occurrences.AddAll(
					occurrencesByDate[selectedDate]
					.OrderBy(o => o.TimeStamp)
					.Select((Occurrence arg) =>
					        new StringElement("{0} {1}".FormatWith(arg.CheckPoint.Emoji,arg.CheckPoint.Name),
							() => this.Controller.ShowDetailDialogFor(arg.CheckPoint))
							{
								Value= arg.TimeStamp.ToString("t")
							}

					)
				);
			}
			var activeGoals = Data.checkPoints
								  .Select(kv => kv.Value)
								  .Where(cp => cp.ActiveForDay(selectedDate.DayOfWeek))
								  .Where(cp => cp.CreatedOn< selectedDate);

			var missed = activeGoals;

			if (occurrencesByDate.ContainsKey(selectedDate))
			{
				var completed = occurrencesByDate[selectedDate].Select(o => o.CheckPoint).Distinct();
				missed = activeGoals.Except(completed);
			}
			if (selectedDate.Date == DateTime.Today)
				missed = missed.Where(cp => cp.IsMissed);

			MissedGoals.AddAll(
				missed.Select(cp=> new StringElement("{0} {1}".FormatWith(cp.Emoji, cp.Name),
							() => this.Controller.ShowDetailDialogFor(cp))
				{
					Value = cp.TargetTime.ToAMPMString()
				})
			);

			if (missed.Any())
				MissedGoals.Caption = "Missed Goals on {0}".FormatWith(dateString);
			else
				MissedGoals.Caption = string.Empty;


		}

	}
}
