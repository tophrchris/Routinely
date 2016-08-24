using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ClockKing.Core
{
	public class CSVDataProvider:ICheckPointDataProvider
	{
		protected IPathProvider Paths { get; set; }
		protected string CheckpointPath{ get { return Paths.GetCheckpointFileName (); }}
		protected string OccurrencesPath{ get {return Paths.GetOccurrencesFileName (); } }

		public CSVDataProvider(IPathProvider paths)
		{
			this.Paths = paths;
		}
			

		public virtual IEnumerable<CheckPoint> ReadCheckPoints()
		{
			if (Paths.Exists(this.CheckpointPath))
			{
				var read = Paths.ReadAllLines (this.CheckpointPath);
				if(read.Count()>10)
					yield break;
				if (read.Any ()) 
				{
					CheckPoint created = new CheckPoint();
					foreach (var line in read) {
						
						var parts = line.Split ('|');
						var name = parts [0];
						var targetTime = TimeSpan.Parse (parts [1]);
						var enabled = bool.Parse (parts [2]);
						var emoji = parts [3];
							created = new CheckPoint (){ Name = name, TargetTime = targetTime, IsEnabled = enabled,Emoji=emoji };
						
						yield return created;
					}
				} 
			}
			yield break;
		}

        public virtual int LoadOccurrences (Dictionary<string, CheckPoint> checkPoints)
        {
            var added = 0;
            if (Paths.Exists (this.OccurrencesPath)) {
                var read = Paths.ReadAllLines (this.OccurrencesPath);
                if (read.Any ()) {
                    foreach (var line in read) {

                        bool Skipped = false;
                        var parts = line.Split ('|');
                        var name = parts [0];
                        var timeStamp = DateTime.Parse (parts [1]);
                        if (parts.Length == 3)
                            bool.TryParse (parts [2], out Skipped);
                        if (checkPoints.ContainsKey (name)) {
                            var newOccurrence = checkPoints [name].CreateOccurrence (timeStamp);
                            newOccurrence.IsSkipped = Skipped;
                            checkPoints [name].AddOccurrence (newOccurrence);
                            added++;
                        }
					}
				}
			}

           return added;
        }

		public virtual bool WriteCheckPoints(IEnumerable<CheckPoint> CheckPoints)
		{
			var toWrite = CheckPoints.Select (cp=>new{
				cp.Name,
				cp.TargetTime,
				cp.IsEnabled,
				Emoji=(string.IsNullOrEmpty(cp.Emoji))?"☀":cp.Emoji    });

			var lines = toWrite.Select (tw => string.Format ("{0}|{1}|{2}|{3}", tw.Name, tw.TargetTime, tw.IsEnabled,tw.Emoji)).ToArray();

			Paths.WriteAllLines (this.CheckpointPath, lines);
			return true;
		}

		public virtual bool WriteAllOccurrences(IEnumerable<Occurrence> occurrences)
		{ 
			if(Paths.Exists(this.OccurrencesPath))
				Paths.Delete(this.OccurrencesPath);
			
            var lines = occurrences.Select(o=> AsWriteable(o));
            Paths.WriteAllLines (this.OccurrencesPath, lines.ToArray());
			return true;
		}

		public virtual void WriteOccurrence(Occurrence toSave)
		{
            var lines = new[]{AsWriteable(toSave) };

			Paths.AppendAllLines(this.OccurrencesPath,lines);
		}

        private string AsWriteable (Occurrence toSave)
        {
            return string.Format ("{0}|{1}|{2}", toSave.checkpointLabel, toSave.TimeStamp, toSave.IsSkipped);
        }
	}
}

