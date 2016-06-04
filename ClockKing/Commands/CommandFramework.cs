using System;
using Foundation;
using UIKit;
using SWTableViewCells;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;

namespace ClockKing
{
	public  abstract class Command
	{
		public string Name;
		public bool IsDestructive{ get; set; }
		public string Category{ get; set; }
		public string LongName{ get; set; }
		public UIColor Color{ get; set; }

		public Command(UIColor Color,string Label)
		{
			this.Name = Label;
			this.IsDestructive = false;
			this.Category = string.Empty;
			this.Color = Color;
		}

		public virtual bool ShouldDecorate(CheckPoint toDecorate)
		{
			return true;
		}
			
		public virtual bool ExecuteFor(CheckPointController controller, CheckPoint checkPoint)
		{
			Alert (string.Format ("Checkpoint:{0}", checkPoint.Name), this.Name);
			return false;
		}

		protected void Alert(string title,string message)
		{
			new UIAlertView (title, message,null, "OK", null).Show ();
		}	
	}

	public abstract class EnabledCheckpointCommand:Command
	{
		public EnabledCheckpointCommand(UIColor color,string title):base(color,title){}
		public override bool ShouldDecorate (CheckPoint toDecorate)
		{
			return toDecorate.Enabled;
		}
	}
	public abstract class DisabledCheckpointCommand:EnabledCheckpointCommand
	{
		public DisabledCheckpointCommand(UIColor color,string title):base(color,title){}
		public override bool ShouldDecorate (CheckPoint toDecorate)
		{
			return !base.ShouldDecorate(toDecorate);
		}
	}
}

