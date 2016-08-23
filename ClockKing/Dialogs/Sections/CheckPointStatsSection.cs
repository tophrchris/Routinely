using System;
using MonoTouch.Dialog;
using ClockKing.Core;
using Humanizer;
using ClockKing.Extensions;
using System.Linq;

namespace ClockKing
{
	public class CheckPointStatsSection:Section
	{
		public CheckPointStatsSection (CheckPoint checkpoint,Action reloader)
		{
			this.Caption="Stats:";
			var eval = new CheckPointEvaluator (checkpoint);

			this.Add(new MultilineElement(eval.Evaluation));

			this.Add (new StringElement ("Enabled?", checkpoint.IsEnabled?"Yes":"No"));

			var createdElement = new ToggledStringElement ("Created");
			createdElement.PrimaryValueGenerator = () => checkpoint.CreatedOn.Humanize (false);
			createdElement.SecondaryValueGenerator = () => checkpoint.CreatedOn.ToString ("G");
			createdElement.PrimaryCaptionGenerator = () => "Created";
			createdElement.SecondaryCaptionGenerator = () => "Created on:";
			createdElement.Tapped += reloader;
			createdElement.Toggle ();
			this.Add (createdElement);

			var nextElement = new ToggledStringElement ("Next");
			nextElement.PrimaryValueGenerator = () => checkpoint.UntilNextTargetTime.Humanize (2);

			var nextCompletionDay = DateTime.Today.AddDays(checkpoint.CompletedToday ? 0 : 1); 

			nextElement.SecondaryValueGenerator = () => 
				(nextCompletionDay + checkpoint.TargetTimeForDay(nextCompletionDay.DayOfWeek)).ToString("G");
			nextElement.PrimaryCaptionGenerator = () => "Next in";
			nextElement.SecondaryCaptionGenerator = () => "Next at";
			nextElement.Tapped += reloader;
			nextElement.Toggle ();
			this.Add (nextElement);

			if (checkpoint.Occurrences.Any ()) 
			{

				var earliest = new ToggledStringElement ("Earliest");
				earliest.PrimaryValueGenerator = () => checkpoint.Earliest.Time.ToAMPMString ();
				earliest.SecondaryValueGenerator = () => checkpoint.Earliest.TimeStamp.ToString ("G");

				var latest = new ToggledStringElement ("Latest");
				latest.PrimaryValueGenerator = () => checkpoint.Latest.Time.ToAMPMString ();
				latest.SecondaryValueGenerator = () => checkpoint.Latest.TimeStamp.ToString ("G");

				var mostRecent = new ToggledStringElement ("Since Most Recent");
				mostRecent.PrimaryValueGenerator = () => checkpoint.SinceLastOccurrence.Humanize (1)+" ago";
				mostRecent.SecondaryValueGenerator = () => checkpoint.MostRecentOccurrenceTimeStamp().ToString("G");

				foreach (var el in new[]{earliest,latest,mostRecent})
				{
					el.Tapped += reloader;
					el.Toggle ();
					this.Add (el);
				}
			}
			if(checkpoint.ScheduledTargets.Any())
			{
				this.Add (new StringElement ("Target", (DateTime.Today + checkpoint.TargetTime).ToString ("t")));
			}
			
		}
	}
}

