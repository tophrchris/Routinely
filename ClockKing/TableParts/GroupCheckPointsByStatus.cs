using System;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;
using System.Diagnostics;


namespace ClockKing
{
	public class GroupCheckPointsByStatus:CheckPointGrouper
	{

		protected override IEnumerable<KeyValuePair<string, IEnumerable<CheckPoint>>> GetSections(IEnumerable<CheckPoint> cps)
		{
			var sections = new Dictionary<string, IEnumerable<CheckPoint>>();

			var active = cps.Where(c => c.IsActive);

			var enabled = active.Where(cp => cp.IsEnabled);
			var notYetCompleted = enabled.Where(c => !c.CompletedToday & !c.IsSkipped);


			sections.Add("Missed",
			             notYetCompleted.Where(c => c.IsMissed).OrderBy(c => c.TargetTimeToday).ToList());
			sections.Add("Upcoming",
			             notYetCompleted.Where(c => c.TargetTimeToday.TimeOfDay >= DateTime.Now.TimeOfDay).OrderBy(c => c.TargetTimeToday).ToList());
			sections.Add("Completed",
			             enabled.Where(c => c.CompletedToday).OrderByDescending(c => c.MostRecentOccurrenceTimeStamp()).ToList());

			if (ClockKingOptions.ShowInactiveGoals)
			{
				sections.Add("Disabled",
				             cps.Where(c => !c.IsEnabled).OrderBy(c => c.TargetTimeToday).ToList());
				sections.Add("Inactive",
				             cps.Where(c => !c.IsActive).OrderBy(c => c.CreatedOn).ToList());
				sections.Add("Skipped",
				             cps.Where(c => c.IsSkipped).OrderBy(c => c.TargetTimeToday).ToList());
			}

			return sections;
		}

}


}

