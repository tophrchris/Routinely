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

				var active = cps.Where (c => c.Active);

				var enabled = active.Where (cp => cp.Enabled);
				var notYetCompleted = enabled.Where (c => !c.CompletedToday & !c.IsSkipped); 


				sections.Add("Missed",
					notYetCompleted.Where(c=>c.IsMissed).OrderBy (c => c.TargetTime));
				sections.Add("Upcoming",
					notYetCompleted.Where(c=>c.TargetTime>=DateTime.Now.TimeOfDay).OrderBy (c => c.TargetTime));
				sections.Add("Completed",
					enabled.Where (c => c.CompletedToday).OrderByDescending (c => c.MostRecentOccurrenceTimeStamp ()));

				if (ClockKingOptions.ShowInactiveGoals)
				{
					sections.Add("Disabled",
						cps.Where(cp => !cp.Enabled).OrderBy(cp => cp.TargetTime));
					sections.Add("Inactive",
						cps.Where(c => !c.Active).OrderBy(c => c.CreatedOn));
					sections.Add("Skipped",
						cps.Where(c => c.IsSkipped).OrderBy(c => c.TargetTime));
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

