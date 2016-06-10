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


			this.Add (new StringElement ("Enabled?", checkpoint.Enabled?"Yes":"No"));

			var createdElement = new ToggledStringElement ("Created");
			createdElement.PrimaryGenerator = () => checkpoint.CreatedOn.Humanize (false);
			createdElement.SecondaryGenerator = () => checkpoint.CreatedOn.ToString ("G");
			createdElement.Tapped += reloader;
			createdElement.Toggle ();
			this.Add (createdElement);

			var nextElement = new ToggledStringElement ("Next");
			nextElement.PrimaryGenerator = () => checkpoint.UntilNextTargetTime.Humanize (2);
			nextElement.SecondaryGenerator = () => checkpoint.UntilNextTargetTime.ToAMPMString();
			nextElement.Tapped += reloader;
			nextElement.Toggle ();
			this.Add (nextElement);

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
					this.Add (el);
				}
			}
			if(checkpoint.ScheduledTargets.Any())
				this.Add (new StringElement ("Target", (DateTime.Today+ checkpoint.TargetTime).ToString ("t")));
			
		}
	}
}

