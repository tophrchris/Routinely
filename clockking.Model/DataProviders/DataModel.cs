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
            Debug.WriteLine ("refreshing****************");
            this.checkPoints = LoadCheckPoints();

            if(loadOccurrences)
                LoadOccurrences ();

            return true;
        }

		public CheckPoint NextCheckpoint
		{
			get{
				var o= this.checkPoints.Values
					.Where(cp=>cp.IsActive && cp.IsEnabled &&  !cp.CompletedToday)
					.OrderBy (cp => cp.TargetTimeToday);
				return o.FirstOrDefault(cp => cp.TargetTimeToday.TimeOfDay > DateTime.Now.ToLocalTime ().TimeOfDay);
			}
		}

		public CheckPoint LastCheckpoint
		{
			get{
				var o= this.checkPoints.Values
					.Where(cp=>cp.IsEnabled && cp.IsActive && !cp.CompletedToday && !cp.IsSkipped)
					.OrderByDescending (cp => cp.TargetTimeToday);
				return o.FirstOrDefault(cp => cp.TargetTimeToday.TimeOfDay < DateTime.Now.ToLocalTime ().TimeOfDay);
			}
		}

		public CheckPoint AddNewCheckPoint(string title,TimeSpan TargetTime,string emoji,string category)
		{
            var newcp = new CheckPoint ()
            {
                Name = title, 
                TargetTime = TargetTime,
                Emoji = emoji,
                CreatedOn=DateTime.Now,
                Category=category
            };
            newcp.UniqueIdentifier = Guid.NewGuid ();
			
            return this.AddNewCheckPoint(newcp);
		}
        /// <summary>
        /// TODO: add more guard clauses here.
        /// </summary>
        /// <returns>The new check point.</returns>
        /// <param name="toAdd">To add.</param>
        public CheckPoint AddNewCheckPoint (CheckPoint toAdd)
        {
            if(toAdd.UniqueIdentifier==Guid.Empty)
                throw new ArgumentException ("unique identifier required!");
            this.checkPoints.Add (toAdd.Name, toAdd);
            SaveCheckPoints ();
            return toAdd;
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
            var toWrite = this.checkPoints.SelectMany (cp => cp.Value.AllOccurrences).OrderBy(o=>o.TimeStamp);
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
            var byGuid = new Dictionary<Guid, CheckPoint> ();

			foreach (var found in dataProvider.ReadCheckPoints())
				checkpoints.Add (found.Name, found);

            byGuid= checkpoints.Values.ToDictionary (k => k.UniqueIdentifier, v => v);

            foreach (var withRelated in checkpoints.Values.Where (c => c.RelativeTarget != null))
                withRelated.RelativeTarget.RelatedCheckPoint = byGuid [withRelated.RelativeTarget.RelatedCheckPointGuid];
			
			return checkpoints;
		}

		public void LoadOccurrences()
		{
			dataProvider.LoadOccurrences (this.checkPoints);
		}
	}
}

