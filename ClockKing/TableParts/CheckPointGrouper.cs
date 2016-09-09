using System;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;
using Foundation;
using System.IO;
using System.Diagnostics;
using UIKit;

namespace ClockKing
{
	public class CheckPointGrouper
	{
		protected int RefreshIntervalInSeconds { get; set; } = 2;

		protected IEnumerable<CheckPoint> checkpoints
		{
			get
			{
				return ((AppDelegate)UIApplication.SharedApplication.Delegate).CheckPointData.checkPoints.Values;
			}
		}

		private Stopwatch SinceLastRefresh { get; set; } = Stopwatch.StartNew();

		public IEnumerable<KeyValuePair<string, IEnumerable<CheckPoint>>> GroupedCheckPoints
		{
			get
			{
				foreach (var section in this.Sections)
					if (section.Value.Any())
					{
						yield return section;
					}

				yield break;
			}
		}

		private IEnumerable<KeyValuePair<string, IEnumerable<CheckPoint>>> Cached { get; set; }

		protected IEnumerable<KeyValuePair<string, IEnumerable<CheckPoint>>> Sections
		{
			get
			{
				IEnumerable<KeyValuePair<string, IEnumerable<CheckPoint>>> found = null;
				var cps = this.checkpoints;
				if (this.Cached == null | SinceLastRefresh.Elapsed.TotalSeconds > RefreshIntervalInSeconds)
				{
					found = this.GetSections(cps);
					Cached = found;
					SinceLastRefresh.Restart();
				}
				else
					found = Cached;
				return found;

			}
		}

		protected virtual IEnumerable<KeyValuePair<string, IEnumerable<CheckPoint>>> GetSections(IEnumerable<CheckPoint> cps)
		{
			var enabled = this.checkpoints.Where(cp => cp.IsEnabled);
			var disabled = this.checkpoints.Where(cp => !cp.IsEnabled);

			if (enabled.Any())
				yield return new KeyValuePair<string, IEnumerable<CheckPoint>>
					(disabled.Any() ? "Enabled" : "", enabled);

			if (disabled.Any() & ClockKingOptions.ShowInactiveGoals)
				yield return new KeyValuePair<string, IEnumerable<CheckPoint>>
					("Disabled", disabled);

			yield break;
		}
	}
}