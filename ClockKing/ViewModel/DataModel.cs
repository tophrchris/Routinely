using System;
using System.Collections.Generic;
using ClockKing.Model;
using System.Linq;
using Foundation;

namespace ClockKing
{
	public class DataModel
	{
		private Dictionary<string,CheckPoint> checkPoints;

		public DataModel ()
		{
			this.checkPoints = new Dictionary<string,CheckPoint> () {
				{"wakeup",new CheckPoint(){Name="wakeup",TargetTime=TimeSpan.FromHours(6)}},
				{"breakfast",new CheckPoint (){ Name = "Breakfast",TargetTime= TimeSpan.FromHours(9) }},
				{"lunch",new CheckPoint () { Name = "Lunch", TargetTime=TimeSpan.FromHours(12)}},
				{"dinner",new CheckPoint(){Name="dinner",TargetTime=TimeSpan.FromHours(19)}},
				{"bedtime",new CheckPoint(){Name="bedtime",TargetTime=TimeSpan.FromHours(23)}}

			};

			var b = checkPoints ["breakfast"];
			var l = checkPoints ["lunch"];

			b.AddOccurrence (b.CreateOccurrence (DateTime.Parse("5/1/16 9:05am")));
			b.AddOccurrence (b.CreateOccurrence (DateTime.Parse("5/2/16 9:15am")));
			b.AddOccurrence (b.CreateOccurrence (DateTime.Parse("5/3/16 9:32am")));
			l.AddOccurrence (l.CreateOccurrence (DateTime.Parse("5/3/16 11:32am")));
			l.AddOccurrence (l.CreateOccurrence (DateTime.Parse("5/3/16 12:32pm")));
			l.AddOccurrence (l.CreateOccurrence (DateTime.Parse("5/3/16 1:32pm")));


		}
		public IEnumerable<CheckPointPair> CheckPointPairs { 
			get {
				return CreateCheckPointPairs(this.checkPoints);
			}

		}

		public void AddNewCheckPoint(string title,TimeSpan TargetTime)
		{
			this.checkPoints.Add (title, new CheckPoint (){ Name = title, TargetTime = TargetTime });
		}
			

		protected IEnumerable<CheckPointPair> CreateCheckPointPairs(Dictionary<string,CheckPoint> checkPoints)
		{

			var ordered = checkPoints
				.Select (kv => kv.Value)
				.OrderBy (cp => cp.averageObservedTime)
				.Select ((c, i) => new{index = i,checkpoint = c});

			var first = ordered.First();

			var paired = 
				from e1 in ordered
				join e2 in ordered on e1.index equals e2.index - 1 into gj
				from oe in gj.DefaultIfEmpty (first)
				select new CheckPointPair (e1.checkpoint, oe.checkpoint);

			return paired;
		}
	}
}

