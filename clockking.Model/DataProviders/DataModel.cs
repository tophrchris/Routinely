using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ClockKing.Core
{
	public class DataModel
	{

		public Dictionary<string,CheckPoint> checkPoints { get; set;}
		private ICheckPointDataProvider dataProvider { get; set; }

		public DataModel (ICheckPointDataProvider provider, bool loadOccurrences = true)
		{
			this.dataProvider = provider;
            RefreshData(loadOccurrences);
			
		}

        public bool RefreshData(bool loadOccurrences=true)
        {
            this.checkPoints = LoadCheckPoints();

            if(loadOccurrences)
                LoadOccurrences ();

            return true;
        }

		public CheckPoint NextCheckpoint
		{
			get{
				var o= this.checkPoints.Values
					.Where(cp=>!cp.CompletedToday)
					.OrderBy (cp => cp.TargetTime);
				return o.FirstOrDefault(cp => cp.TargetTime > DateTime.Now.ToLocalTime ().TimeOfDay);
			}
		}

		public CheckPoint LastCheckpoint
		{
			get{
				var o= this.checkPoints.Values
					.Where(cp=>!cp.CompletedToday)
					.OrderByDescending (cp => cp.TargetTime);
				return o.FirstOrDefault(cp => cp.TargetTime < DateTime.Now.ToLocalTime ().TimeOfDay);
			}
		}

		public CheckPoint AddNewCheckPoint(string title,TimeSpan TargetTime,string emoji)
		{
            var newcp = new CheckPoint (){ Name = title, TargetTime = TargetTime, Emoji = emoji,CreatedOn=DateTime.Now };
            newcp.UniqueIdentifier = Guid.NewGuid ();
			this.checkPoints.Add (title, newcp);
			SaveCheckPoints ();
			return newcp;
		}
			
		public bool RemoveCheckPoint(CheckPoint toDelete)
		{
			if (this.checkPoints.ContainsKey (toDelete.Name)) 
			{
				//TODO: delete observations
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
            var toWrite = this.checkPoints.SelectMany (cp => cp.Value.Occurrences).OrderBy(o=>o.TimeStamp);
			return this.dataProvider.WriteAllOccurrences (toWrite);
		}

		public void SaveOccurrence(Occurrence toSave)
		{
            Debug.WriteLine ("data model save occurrence");
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

