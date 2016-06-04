using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Foundation;
using System.IO;

namespace ClockKing.Core
{
	public class JSONDataProvider:CSVDataProvider
	{
		public JSONDataProvider ():base(".json")
		{
			
		}

		public override IEnumerable<CheckPoint> ReadCheckPoints ()
		{
			if (File.Exists (this.CheckpointPath)) {
				var json = File.ReadAllText (this.CheckpointPath);

				var found = JsonConvert.DeserializeObject<List<CheckPoint>> (json);// as IEnumerable<CheckPoint>;
				var cv = found as IEnumerable<CheckPoint>;
				foreach (var cp in cv)
					yield return cp;
			}
			yield break;
		}

		public override bool WriteCheckPoints (IEnumerable<CheckPoint> CheckPoints)
		{
			string json = JsonConvert.SerializeObject (CheckPoints, Formatting.Indented,
				new JsonSerializerSettings{ReferenceLoopHandling=ReferenceLoopHandling.Ignore});

			File.WriteAllText (CheckpointPath, json);
			return true;
		}
	}
}

