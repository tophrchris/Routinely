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
				var notYetCompleted = enabled.Where (c => !c.CompletedToday); 


				sections.Add("Missed",
					notYetCompleted.Where(c=>c.IsMissed).OrderBy (c => c.TargetTime));
				sections.Add("Upcoming",
					notYetCompleted.Where(c=>c.TargetTime>=DateTime.Now.TimeOfDay).OrderBy (c => c.TargetTime));
				sections.Add("Completed",
					enabled.Where (c => c.CompletedToday).OrderByDescending (c => c.MostRecentOccurrenceTimeStamp ()));
				sections.Add ("Disabled",	
					cps.Where (cp => !cp.Enabled).OrderBy (cp => cp.TargetTime));
				sections.Add ("Inactive", 
					cps.Where (c => !c.Active).OrderBy (c => c.CreatedOn));

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

