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
		private Dictionary<string,CheckPoint> checkPoints;

		public DataModel (bool loadOccurrences = true)
		{
			this.checkPoints = LoadCheckPoints();
			if(loadOccurrences)
				LoadOccurrences ();

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
			var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			var fileName = Path.Combine (documents, "checkpoints.csv");

			var toWrite = this.checkPoints.Select (cp=>new{
				cp.Value.Name,
				cp.Value.TargetTime,
				cp.Value.Enabled,
				Emoji=(string.IsNullOrEmpty(cp.Value.Emoji))?"☀":cp.Value.Emoji    });

			var lines = toWrite.Select (tw => string.Format ("{0}|{1}|{2}|{3}", tw.Name, tw.TargetTime, tw.Enabled,tw.Emoji)).ToArray();


			File.WriteAllLines (fileName, lines);
			return true;

		}

		public bool SaveOccurrences()
		{
			var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			var fileName = Path.Combine (documents, "occurrences.csv");

			var toWrite = this.checkPoints.SelectMany(cp=>cp.Value.Occurrences.Select(o=>
				new {o.checkpointLabel,  o.timeStamp}));
			var lines = toWrite.Select (tw => string.Format ("{0}|{1}", tw.checkpointLabel, tw.timeStamp)).ToArray ();

			File.WriteAllLines (fileName, lines);
			return true;
		}

		public void SaveOccurrence(Occurrence toSave)
		{
			var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			var fileName = Path.Combine (documents, "occurrences.csv");
			var lines = new[]{string.Format("{0}|{1}",toSave.checkpointLabel,toSave.timeStamp) };

			File.AppendAllLines(fileName,lines);
		}

		public Dictionary<string,CheckPoint>  LoadCheckPoints()
		{
			var checkpoints = new Dictionary<string,CheckPoint> ();

			var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			var fileName = Path.Combine (documents, "checkpoints.csv");

			if (File.Exists(fileName))
			{
				var read = File.ReadAllLines (fileName);
				if (read.Any ()) 
				{
				
					foreach (var line in read) {
						var parts = line.Split ('|');
						var name = parts [0];
						var targetTime = TimeSpan.Parse (parts [1]);
						var enabled = bool.Parse (parts [2]);
						var emoji = parts [3];
						checkpoints.Add (name, new CheckPoint (){ Name = name, TargetTime = targetTime, Enabled = enabled,Emoji=emoji });
					}
				} 
			}
			if(!checkpoints.Any())
				checkpoints = CreateDefaultCheckPoints ();
			
			return checkpoints;
		}

		public void LoadOccurrences()
		{
			var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			var fileName = Path.Combine (documents, "occurrences.csv");

			if (File.Exists (fileName)) {
				var read = File.ReadAllLines (fileName);
				if (read.Any ()) {
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
	}
}

