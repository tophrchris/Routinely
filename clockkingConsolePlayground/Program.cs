using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using clockking.Model;


namespace clockkingConsolePlayground
{
	class MainClass
	{
		public static void Main (string[] args)
		{



			var checkPoints = new Dictionary<string,CheckPoint> () {
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

			foreach(var p in paired)
				Console.WriteLine (p);

			//var allOccurances = checkPoints.SelectMany (kv => kv.Value.Occurrences);

			string json = JsonConvert.SerializeObject (checkPoints, Formatting.Indented,
				new JsonSerializerSettings{ReferenceLoopHandling=ReferenceLoopHandling.Ignore});
			Console.WriteLine (json);
		
			Console.ReadLine ();
				
		}
	}
}