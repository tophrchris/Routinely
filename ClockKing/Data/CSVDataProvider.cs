using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ClockKing.Core
{

	public class CSVDataProvider:ICheckPointDataProvider
	{
		protected string MyDocumentsPath{ get; private set;}
		private string CheckpointPath{ get; set;}
		private string OccurrencesPath{ get; set; }
		private static string checkpointFileName =  "checkpoints.csv";
		private static string occurrencesFileName = "occurrences.csv";

		public CSVDataProvider()
		{
			this.MyDocumentsPath = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			this.CheckpointPath = Path.Combine (this.MyDocumentsPath, checkpointFileName);
			this.OccurrencesPath = Path.Combine (this.MyDocumentsPath, occurrencesFileName);
		}

		public IEnumerable<CheckPoint> ReadCheckPoints()
		{

			if (File.Exists(this.CheckpointPath))
			{
				var read = File.ReadAllLines (this.CheckpointPath);
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
							Console.WriteLine (e.Message);
						}
						yield return created;

					}
				} 
			}
			yield break;
		}

		public int LoadOccurrences(Dictionary<string,CheckPoint> checkPoints)
		{
			
			if (File.Exists (this.OccurrencesPath)) {
				var read = File.ReadAllLines (this.OccurrencesPath);
				if (read.Any ()) 
				{
					foreach (var line in read) {

						var parts = line.Split ('|');
						var name = parts [0];
						var timeStamp = DateTime.Parse (parts [1]);
						if (checkPoints.ContainsKey (name))
							checkPoints [name].AddOccurrence (
								checkPoints [name].CreateOccurrence (timeStamp));
					}
				}
			}

			return 0;//checkPoints.Sum (cp => cp.Value.Occurrences.Count ());
		}

		private void createRandomOccurrences(Dictionary<string,CheckPoint> checkPoints){

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


		public bool WriteCheckPoints(IEnumerable<CheckPoint> CheckPoints)
		{
			if(File.Exists(occurrencesFileName))
				File.Delete(occurrencesFileName);

			var toWrite = CheckPoints.Select (cp=>new{
				cp.Name,
				cp.TargetTime,
				cp.Enabled,
				Emoji=(string.IsNullOrEmpty(cp.Emoji))?"☀":cp.Emoji    });

			var lines = toWrite.Select (tw => string.Format ("{0}|{1}|{2}|{3}", tw.Name, tw.TargetTime, tw.Enabled,tw.Emoji)).ToArray();

			File.WriteAllLines (this.CheckpointPath, lines);
			return true;
		}

		public bool WriteAllOccurrences(IEnumerable<Occurrence> occurrences)
		{
			
			var toWrite = occurrences.Select(o=> new {o.checkpointLabel,  o.timeStamp});
			var lines = toWrite.Select (tw => string.Format ("{0}|{1}", tw.checkpointLabel, tw.timeStamp)).ToArray ();
			File.WriteAllLines (this.OccurrencesPath, lines);
			return true;
		}

		public void WriteOccurrence(Occurrence toSave)
		{
			var lines = new[]{string.Format("{0}|{1}",toSave.checkpointLabel,toSave.timeStamp) };

			File.AppendAllLines(this.OccurrencesPath,lines);
		}
	}
}

