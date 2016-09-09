using System;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;
using Foundation;
using System.IO;
using UIKit;

namespace ClockKing
{
	public class GroupCheckPointsByTimeOfDay:CheckPointGrouper
	{

		protected override IEnumerable<KeyValuePair<string, IEnumerable<CheckPoint>>> GetSections(IEnumerable<CheckPoint> cps)
		{
			var sunrise = TimeSpan.FromHours(6);
			var noon = TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(-1));
			var sunset = TimeSpan.FromHours(19);

			var boundaries = new Dictionary<string, Tuple<TimeSpan, TimeSpan>>() {
					{ "Wee Hours",Tuple.Create (TimeSpan.FromHours (0), sunrise) },
					{ "Morning",Tuple.Create (sunrise, noon) },
					{ "Afternoon",Tuple.Create (noon,sunset) },
					{ "Evening", Tuple.Create(sunset,TimeSpan.FromHours(24))}
				};

			var sections = checkpoints
					.Where(cp => cp.IsActive)
					.Where(cp => cp.IsEnabled)
					.OrderBy(cp => cp.TargetTime)
					.Select(cp => new
					{
						CheckPoint = cp,
						Section = boundaries.First(b => cp.TargetTime >= b.Value.Item1 && cp.TargetTime < b.Value.Item2)
					})
					.GroupBy(g => g.Section);


			foreach (var section in sections)
				if (section.Any())
					yield return
						new KeyValuePair<string, IEnumerable<CheckPoint>>
						(section.Key.Key, section.Select(q => q.CheckPoint).AsEnumerable());

			if (checkpoints.Any(cp => !cp.IsEnabled) & ClockKingOptions.ShowInactiveGoals)
				yield return new KeyValuePair<string, IEnumerable<CheckPoint>>
					("Disabled", checkpoints.Where(cp => !cp.IsEnabled).OrderBy(cp => cp.TargetTime));
			if (checkpoints.Any(cp => !cp.IsActive) & ClockKingOptions.ShowInactiveGoals)
				yield return new KeyValuePair<string, IEnumerable<CheckPoint>>
					("Inactive", checkpoints.Where(cp => !cp.IsActive).OrderBy(cp => cp.CreatedOn));

			yield break;
		}


	}
}

