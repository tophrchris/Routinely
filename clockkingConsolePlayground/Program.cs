using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ClockKing.Core;


namespace ClockKing.ConsolePlayground
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var provider = new JSONDataProvider (new PathProvider (".json"));

			var checkpoints = provider.ReadCheckPoints ().ToDictionary (kv => kv.Name, kv => kv);

			var cp = checkpoints ["Bedtime"];
			cp.AddOccurrence (cp.CreateOccurrence ());

			provider.LoadOccurrences (checkpoints);

			foreach(var ce in checkpoints.Values.Select(c=>new CheckPointEvaluator(c)))
				Console.WriteLine (ce.Evaluation);
		
			Console.ReadLine ();
				
		}
	}
}