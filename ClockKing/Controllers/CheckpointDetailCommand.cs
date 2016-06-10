using System;
using Foundation;
using UIKit;
using ClockKing.Core;
using MonoTouch.Dialog;
using System.Linq;
using System.Collections.Generic;
using Humanizer;

namespace ClockKing
{
	public class CheckpointDetailCommand
	{
		protected CheckPointController Controller{ get; set;}
		protected CheckPoint LastCheckpointDetailed { get; set;}

		public CheckpointDetailCommand (CheckPointController controller)
		{
			this.Controller = controller;
		}

		public CheckPointDetailDialog GetDetailDialog(CheckPoint Data)
		{
			var checkpoint = Data;

			this.LastCheckpointDetailed = checkpoint;

			return new CheckPointDetailDialog (this.Controller,Data, new RootElement("Goal"));
		}

		public CheckPointDetailDialog ShowDetailDialog(CheckPoint Data)
		{
			return ShowDetailDialog (GetDetailDialog (Data), Data);
		}

		public CheckPointDetailDialog ShowDetailDialog(CheckPointDetailDialog dialog,CheckPoint Data=null)
		{
			if (Data == null)
				Data = LastCheckpointDetailed;
			
			this.Controller.NavigationController.PushViewController (dialog,true);
			return dialog;

		}
	}
}