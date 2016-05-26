using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ClockKing.Model
{
	public class Occurrence
	{

		private CheckPoint checkPoint;

		public DateTime timeStamp; 

		public Occurrence(CheckPoint assignedCheckPoint,DateTime observedTime)
		{
			this.checkPoint = assignedCheckPoint;
			this.timeStamp = observedTime;
		}

		public string checkpointLabel { get { return this.CheckPoint.Name; } }

		[JsonIgnore]
		public CheckPoint CheckPoint { get { return this.checkPoint; } }

		[JsonIgnore]
		public TimeSpan Time 
		{
			get
			{
				return this.timeStamp.TimeOfDay;
			}
		}
	}

}

