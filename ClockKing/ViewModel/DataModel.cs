using System;
using System.Collections.Generic;
using ClockKing.Model;
using System.Linq;
using Foundation;
using System.IO;

namespace ClockKing
{
	public class DataModel
	{
		private Dictionary<string,LinkedList<CheckPoint>> grouped { get; set; }
		private Dictionary<string,CheckPoint> checkPoints { get; set;}
		private ICheckPointDataProvider dataProvider { get; set; }

		public DataModel (bool loadOccurrences = true)
		{
			this.dataProvider = new CSVDataProvider ();
			this.checkPoints = LoadCheckPoints();

			LoadGroups ();

			if(loadOccurrences)
				LoadOccurrences ();
		}

		public void LoadGroups()
		{
			var cps = this.dataProvider.ReadCheckPoints ().ToList();

			this.grouped = new Dictionary<string, LinkedList<CheckPoint>> ();
			var enabled = cps.Where (cp => cp.Enabled);
			var disabled = cps.Where (cp => !cp.Enabled).OrderBy (cp => cp.TargetTime);
			var completed = enabled.Where (c => c.CompletedToday).OrderBy (c => c.MostRecentOccurrenceTimeStamp ());
			var upcoming = enabled.Where(c=>!c.CompletedToday).OrderBy(c=>c.TargetTime);

			if(completed.Any())
				this.grouped.Add ("Completed", new LinkedList<CheckPoint>(completed));

			if(upcoming.Any())
				this.grouped.Add ("upcoming", new LinkedList<CheckPoint> (upcoming));

			if (disabled.Any ())
				this.grouped.Add ("Disabled", new LinkedList<CheckPoint> (disabled));
		}

		public Dictionary<string,LinkedList<CheckPoint>> GroupedCheckPoints{
			get{
				return this.grouped;
				}
		}


		public IEnumerable<CheckPointPair> CheckPointPairs { 
			get 
			{
				return CreateCheckPointPairs(this.checkPoints);
			}

		}
		public IEnumerable<CheckPoint> DisabledCheckPoints
		{
			get
			{
				return this.checkPoints.Where (cp => !cp.Value.Enabled).Select(kv=>kv.Value);
			}
		}




		public CheckPoint AddNewCheckPoint(string title,TimeSpan TargetTime,string emoji)
		{
			var newcp = new CheckPoint (){ Name = title, TargetTime = TargetTime, Emoji = emoji };
			this.checkPoints.Add (title, newcp);
			SaveCheckPoints ();
			return newcp;
		}
			
		public bool RemoveCheckPoint(CheckPoint toDelete)
		{
			if (this.checkPoints.ContainsKey (toDelete.Name)) 
			{
				var removed= this.checkPoints.Remove (toDelete.Name);
				if (removed)
					SaveCheckPoints ();
				return removed;
			}
			return false;
		}

		public bool SaveCheckPoints()
		{
			var toWrite = this.checkPoints.Select (cp => cp.Value);
			return dataProvider.WriteCheckPoints (toWrite);
		}

		public bool SaveOccurrences()
		{
			var toWrite = this.checkPoints.SelectMany (cp => cp.Value.Occurrences);
			return this.dataProvider.WriteAllOccurrences (toWrite);
		}

		public void SaveOccurrence(Occurrence toSave)
		{
			this.dataProvider.WriteOccurrence (toSave);
		}

		public Dictionary<string,CheckPoint>  LoadCheckPoints()
		{
			var checkpoints = new Dictionary<string,CheckPoint> ();

			foreach (var found in dataProvider.ReadCheckPoints())
				checkpoints.Add (found.Name, found);

			if(!checkpoints.Any())
				checkpoints = CreateDefaultCheckPoints ();
			
			return checkpoints;
		}

		public void LoadOccurrences()
		{
			dataProvider.LoadOccurrences (this.checkPoints);
		}

		protected IEnumerable<CheckPointPair> CreateCheckPointPairs(Dictionary<string,CheckPoint> checkPoints)
		{

			var ordered = checkPoints
				.Where(cp=>cp.Value.Enabled)
				.Select (kv => kv.Value)
				.OrderBy (cp => cp.TargetTime)
				.ThenBy(cp=>cp.averageObservedTime)
				.Select ((c, i) => new{index = i,checkpoint = c});
			if (ordered.Any ()) {
				var first = ordered.First ();

				var paired = 
					from e1 in ordered
					join e2 in ordered on e1.index equals e2.index - 1 into gj
					from oe in gj.DefaultIfEmpty (first)
					select new CheckPointPair (e1.checkpoint, oe.checkpoint);
				
				return paired;
			} else
				return new List<CheckPointPair> ();

		}




		public Dictionary<string,CheckPoint> CreateDefaultCheckPoints()
		{
			var defaults= new Dictionary<string,CheckPoint> () {
				{"wakeup",new CheckPoint(){Name="wakeup",TargetTime=TimeSpan.FromHours(6)}},
				{"Breakfast",new CheckPoint (){ Name = "Breakfast",TargetTime= TimeSpan.FromHours(9) }},
				{"Lunch",new CheckPoint () { Name = "Lunch", TargetTime=TimeSpan.FromHours(12)}},
				{"dinner",new CheckPoint(){Name="dinner",TargetTime=TimeSpan.FromHours(19)}},
				{"bedtime",new CheckPoint(){Name="bedtime",TargetTime=TimeSpan.FromHours(23)}}
			};
			var b = defaults ["Breakfast"];
			var l = defaults ["Lunch"];

			b.AddOccurrence (b.CreateOccurrence (DateTime.Parse("5/1/16 9:05am")));
			b.AddOccurrence (b.CreateOccurrence (DateTime.Parse("5/2/16 9:15am")));
			b.AddOccurrence (b.CreateOccurrence (DateTime.Parse("5/3/16 9:32am")));
			l.AddOccurrence (l.CreateOccurrence (DateTime.Parse("5/3/16 11:32am")));
			l.AddOccurrence (l.CreateOccurrence (DateTime.Parse("5/3/16 12:32pm")));
			l.AddOccurrence (l.CreateOccurrence (DateTime.Parse("5/3/16 1:32pm")));
			return defaults;
		}
	}
}

