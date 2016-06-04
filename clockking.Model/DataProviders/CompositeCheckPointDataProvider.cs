using System;
using System.Collections.Generic;
using System.Linq;

namespace ClockKing.Core
{
	public class CompositeCheckPointDataProvider:ICheckPointDataProvider	
	{
		private List<ICheckPointDataProvider> Providers {get;set;}

		public CompositeCheckPointDataProvider ()
		{
			this.Providers = new List<ICheckPointDataProvider> ();
		}

		public void AddProvider(ICheckPointDataProvider provider)
		{
			this.Providers.Add(provider);
		}

		public IEnumerable<CheckPoint> ReadCheckPoints()
		{
			var found = new List<CheckPoint> ();

			foreach (var provider in Providers) 
			{
				found.AddRange (provider.ReadCheckPoints ());
				if (found.Any ())
					return found;
			}
			return found;
		}
		public bool WriteCheckPoints(IEnumerable<CheckPoint> CheckPoints)
		{
			var anyWritten = false;
			foreach (var provider in Providers)
				if (provider.WriteCheckPoints (CheckPoints))
					anyWritten= true;
			
			return anyWritten;
		}


		public int LoadOccurrences(Dictionary<string,CheckPoint> checkPoints)
		{
			foreach (var provider in Providers) 
			{
				var loaded = provider.LoadOccurrences (checkPoints);
				if (loaded > 0)
					return loaded;
			}
			return 0;
		}
			
		public bool WriteAllOccurrences(IEnumerable<Occurrence> occurrences)
		{
			var anyWritten = false;
			foreach(var provider in Providers)
				if(provider.WriteAllOccurrences(occurrences))
					anyWritten=true;

			return anyWritten;

		}

		public void WriteOccurrence(Occurrence toSave)
		{
			foreach (var provider in Providers)
				provider.WriteOccurrence (toSave);
		}


	}
}

