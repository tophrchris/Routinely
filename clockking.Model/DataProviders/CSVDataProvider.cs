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
						try{
						var parts = line.Split ('|');
						var name = parts [0];
						var targetTime = TimeSpan.Parse (parts [1]);
						var enabled = bool.Parse (parts [2]);
						var emoji = parts [3];
							created = new CheckPoint (){ Name = name, TargetTime = targetTime, Enabled = enabled,Emoji=emoji };
						}catch(Exception e)
						{
							//Console.WriteLine (e.Message);
						}
						yield return created;
					}
				} 
			}
			yield break;
		}

		public virtual int LoadOccurrences(Dictionary<string,CheckPoint> checkPoints)
		{
            var added = 0;
			if (Paths.Exists (this.OccurrencesPath)) {
				var read = Paths.ReadAllLines (this.OccurrencesPath);
				if (read.Any ()) 
				{
					foreach (var line in read) {

						var parts = line.Split ('|');
						var name = parts [0];
						var timeStamp = DateTime.Parse (parts [1]);
                        if (checkPoints.ContainsKey(name))
                        {
                            checkPoints[name].AddOccurrence(
                                checkPoints[name].CreateOccurrence(timeStamp));
                            added++;
                        }
					}
				}
			}


            return added;
		}

		private void createRandomOccurrences(Dictionary<string,CheckPoint> checkPoints)
		{
			var r = new Random ();

			foreach (var cp in checkPoints.Values)
			{
				var maxDays = 2 + r.Next (8);
				foreach (int i in Enumerable.Range(2,maxDays))
				{
					var mins = r.Next (90);
					var direction = r.NextDouble () > .5d;
					cp.AddOccurrence(
						cp.CreateOccurrence(cp.TargetTimeToday
							.AddDays(i*-1)
							.AddMinutes(mins*((direction)?1:-1))));
				}
			}

			WriteAllOccurrences (checkPoints.SelectMany (cp => cp.Value.Occurrences));
		}


		public virtual bool WriteCheckPoints(IEnumerable<CheckPoint> CheckPoints)
		{
			var toWrite = CheckPoints.Select (cp=>new{
				cp.Name,
				cp.TargetTime,
				cp.Enabled,
				Emoji=(string.IsNullOrEmpty(cp.Emoji))?"☀":cp.Emoji    });

			var lines = toWrite.Select (tw => string.Format ("{0}|{1}|{2}|{3}", tw.Name, tw.TargetTime, tw.Enabled,tw.Emoji)).ToArray();

			Paths.WriteAllLines (this.CheckpointPath, lines);
			return true;
		}

		public virtual bool WriteAllOccurrences(IEnumerable<Occurrence> occurrences)
		{ 
			if(Paths.Exists(this.OccurrencesPath))
				Paths.Delete(this.OccurrencesPath);
			
			var toWrite = occurrences.Select(o=> new {o.checkpointLabel,  o.TimeStamp});
			var lines = toWrite.Select (tw => string.Format ("{0}|{1}", tw.checkpointLabel, tw.TimeStamp)).ToArray ();
			Paths.WriteAllLines (this.OccurrencesPath, lines);
			return true;
		}

		public virtual void WriteOccurrence(Occurrence toSave)
		{
			var lines = new[]{string.Format("{0}|{1}",toSave.checkpointLabel,toSave.TimeStamp) };

			Paths.AppendAllLines(this.OccurrencesPath,lines);
		}
	}
}

