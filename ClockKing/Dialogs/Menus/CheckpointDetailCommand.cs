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
		protected iCheckpointCommandController Controller{ get; set;}
		protected CheckPoint LastCheckpointDetailed { get; set;}

		public CheckpointDetailCommand (iCheckpointCommandController controller)
		{
			this.Controller = controller;
		}

		public CheckPointDetailDialog ShowDetailDialog(CheckPoint Data)
		{
			return ShowDetailDialog (GetDetailDialog (Data), Data);
		}

		public CheckPointDetailDialog GetDetailDialog(CheckPoint Data)
		{
			this.LastCheckpointDetailed = Data;

			return new CheckPointDetailDialog (this.Controller,Data, new RootElement(Data.Name));
		}

		public CheckPointDetailDialog ShowDetailDialog(CheckPointDetailDialog dialog,CheckPoint Data=null)
		{
			if (Data == null)
				Data = LastCheckpointDetailed;
			this.Controller.NavigateToDialog (dialog);
			return dialog;

		}
	}
}