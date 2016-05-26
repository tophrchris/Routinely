using System;
using System.Collections.Generic;

namespace ClockKing.Model
{
	public interface ICheckPointDataProvider
	{

		 IEnumerable<CheckPoint> ReadCheckPoints();

		 int LoadOccurrences(Dictionary<string,CheckPoint> checkPoints);

		 bool WriteCheckPoints(IEnumerable<CheckPoint> CheckPoints);

		 bool WriteAllOccurrences(IEnumerable<Occurrence> occurrences);

		void WriteOccurrence(Occurrence toSave);
	}
}

