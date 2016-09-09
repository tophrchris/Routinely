using System;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;
using System.Diagnostics;


namespace ClockKing
{
	public class GroupCheckPointsByCategory : CheckPointGrouper
	{
		protected override IEnumerable<KeyValuePair<string, IEnumerable<CheckPoint>>> GetSections(IEnumerable<CheckPoint> cps)
		{
			var result = new Dictionary<string, IEnumerable<CheckPoint>>();

			var found = from cp in this.checkpoints
						group cp by (cp.Category == null) ? string.Empty : cp.Category.Trim() into byCat
						orderby byCat.Key
						select byCat;


			foreach (var g in found)
			{
				var key = string.IsNullOrEmpty(g.Key) ? "(no category)" : g.Key;
				result.Add(key, g.OrderBy(cp => cp.TargetTime).ThenBy(cp => cp.Name).ToList().AsEnumerable());
			}
			return result;
		}

	}
}

