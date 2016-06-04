using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace ClockKing.Core
{
	public class JSONDataProvider:CSVDataProvider
	{
		public JSONDataProvider (IPathProvider paths):base(paths)
		{
			
		}

		public override IEnumerable<CheckPoint> ReadCheckPoints ()
		{
			if (Paths.Exists (this.CheckpointPath)) {
				var json = Paths.ReadAllText (this.CheckpointPath);

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

			Paths.WriteAllText (CheckpointPath, json);
			return true;
		}
	}
}

