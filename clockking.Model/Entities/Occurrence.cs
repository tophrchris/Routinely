using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ClockKing.Core
{
	public class Occurrence
	{

        private CheckPoint checkPoint{ get; set;}

        public DateTime TimeStamp{ get; set;}

		public Occurrence(CheckPoint assignedCheckPoint,DateTime observedTime)
		{
			this.checkPoint = assignedCheckPoint;
			this.TimeStamp = observedTime;
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
					if(this.TimeStamp.TimeOfDay.Hours<2)
						return this.TimeStamp.TimeOfDay.Add(TimeSpan.FromHours(24));
				
				return this.TimeStamp.TimeOfDay;
			}
		}
		public DateTime Date
		{
			get{
				return this.TimeStamp.Date.Add (TimeSpan.FromDays (this.Time.TotalHours > 24.0D ? -1 : 0));
			}
		}
	}

}

