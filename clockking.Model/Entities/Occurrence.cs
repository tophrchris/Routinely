using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ClockKing.Core
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

				if(checkPoint.TargetTime>TimeSpan.FromHours(22))
					if(this.timeStamp.TimeOfDay.Hours<2)
						return this.timeStamp.TimeOfDay.Add(TimeSpan.FromHours(24));
				
				return this.timeStamp.TimeOfDay;
			}
		}
		public DateTime Date
		{
			get{
				return this.timeStamp.Date.Add (TimeSpan.FromDays (this.Time.TotalHours > 24.0D ? -1 : 0));
			}
		}
	}

}

