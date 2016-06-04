using System;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;
using Foundation;
using System.IO;

namespace ClockKing
{
	public class GroupCheckPointsByStatus:CheckPointGrouper
	{
		public GroupCheckPointsByStatus(IEnumerable<CheckPoint> toGroup):base(toGroup){}

		public override IEnumerable<KeyValuePair<string,IEnumerable<CheckPoint>>> GroupedCheckPoints
		{
			get{
				var cps = this.checkpoints;

				var sections = new  Dictionary<string, IEnumerable<CheckPoint>>();

				var enabled = cps.Where (cp => cp.Enabled);
				var notYetCompleted = enabled.Where (c => !c.CompletedToday); 

				sections.Add("Completed",
					enabled.Where (c => c.CompletedToday).OrderBy (c => c.MostRecentOccurrenceTimeStamp ()));
				sections.Add("Missed",
					notYetCompleted.Where(c=>c.TargetTime<=DateTime.Now.TimeOfDay).OrderBy (c => c.TargetTime));
				sections.Add("Upcoming",
					notYetCompleted.Where(c=>c.TargetTime>DateTime.Now.TimeOfDay).OrderBy (c => c.TargetTime));
				sections.Add ("Disabled",	
					cps.Where (cp => !cp.Enabled).OrderBy (cp => cp.TargetTime));

				foreach (var section in sections)
					if (section.Value.Any ())
						yield return section;

				yield break;
			}
		}
	}
}

