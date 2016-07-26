﻿using System;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;
using System.Diagnostics;


namespace ClockKing
{
	public class GroupCheckPointsByCategory : CheckPointGrouper
	{
		public override IEnumerable<KeyValuePair<string, IEnumerable<CheckPoint>>> GroupedCheckPoints
		{
			get
			{

				var found = from cp in this.checkpoints
					        group cp by cp.Category.Trim() into byCat
							select byCat;

				foreach (var g in found)
				{
					var key = string.IsNullOrEmpty(g.Key) ? "(no category)" : g.Key;
					var emit = new KeyValuePair<string, IEnumerable<CheckPoint>>(key, g.AsEnumerable());
					yield return emit;
				}
				yield break;
			}
		}
	}
}
