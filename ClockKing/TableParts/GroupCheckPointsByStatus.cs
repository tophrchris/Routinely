using System;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;
using System.Diagnostics;


namespace ClockKing
{
	public class GroupCheckPointsByStatus:CheckPointGrouper
	{

		public override IEnumerable<KeyValuePair<string,IEnumerable<CheckPoint>>> GroupedCheckPoints
		{
			get{
				var cps = this.checkpoints;

				var sections = new  Dictionary<string, IEnumerable<CheckPoint>>();

				var active = cps.Where (c => c.IsActive);

				var enabled = active.Where (cp => cp.IsEnabled);
				var notYetCompleted = enabled.Where (c => !c.CompletedToday & !c.IsSkipped); 


				sections.Add("Missed",
					notYetCompleted.Where(c=>c.IsMissed).OrderBy (c => c.TargetTimeToday));
				sections.Add("Upcoming",
					notYetCompleted.Where(c=>c.TargetTimeToday.TimeOfDay>=DateTime.Now.TimeOfDay).OrderBy (c => c.TargetTimeToday));
				sections.Add("Completed",
					enabled.Where (c => c.CompletedToday).OrderByDescending (c => c.MostRecentOccurrenceTimeStamp ()));

				if (ClockKingOptions.ShowInactiveGoals)
				{
					sections.Add("Disabled",
						cps.Where(cp => !cp.IsEnabled).OrderBy(cp => cp.TargetTimeToday));
					sections.Add("Inactive",
						cps.Where(c => !c.IsActive).OrderBy(c => c.CreatedOn));
					sections.Add("Skipped",
						cps.Where(c => c.IsSkipped).OrderBy(c => c.TargetTimeToday));
				}

				foreach (var section in sections)
					if (section.Value.Any())
					{
						yield return section;
					}

				yield break;
			}
		}
	}


}

