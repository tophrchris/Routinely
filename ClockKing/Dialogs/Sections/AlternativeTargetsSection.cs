﻿using System;
using MonoTouch.Dialog;
using ClockKing.Core;
using Humanizer;
using ClockKing.Extensions;
using System.Linq;
namespace ClockKing
{
	public class AlternativeTargetsSection:Section
	{
		public AlternativeTargetsSection (CheckPoint checkpoint,iCheckpointCommandController Controller,CheckPointDetailDialog dialog)
		{

			this.Caption = "Alternative Targets";

			foreach (var at in checkpoint.ScheduledTargets) 
			{
				var maxDayLength = (at.ApplicableDays.Count () > 3) ? 3 : 10;	
				var se = new StringElement( string.Join(", ",
					at.ApplicableDays.Select(t=>t.ToString().Truncate(maxDayLength,"")).ToArray()),
					at.TargetTime.HasValue?
					(DateTime.Today+ at.TargetTime.Value).ToString("t"):"Inactive");
				se.Tapped += () => 
				{
					var root = new RootElement("Scheduled Target");
					var d = new ScheduledTargetDialog(root,at,checkpoint,Controller,dialog);
					dialog.NavigationController.PushViewController(d,true);	
				};
				this.Add (se);
			}
		}
	}
}
