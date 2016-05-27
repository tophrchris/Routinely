using System;
using Foundation;
using UIKit;
using SWTableViewCells;
using System.Collections.Generic;
using ClockKing.Model;
using System.Linq;

namespace ClockKing
{
	public class Command:UIButton
	{
		public string Name;
		public bool IsDestructive{ get; set; }
		public string Category{ get; set; }
		public string LongName{ get; set; }

		public Command(UIColor Color,string Label):base(UIButtonType.Custom)
		{
			this.Name = Label;
			this.IsDestructive = false;
			this.Category = string.Empty;
			this.BackgroundColor = Color;
			this.SetTitle (Label, UIControlState.Normal);
			this.SetTitleColor (UIColor.White, UIControlState.Normal);
			this.TitleLabel.AdjustsFontSizeToFitWidth = true;
		}

		public virtual bool ShouldDecorate(CheckPoint toDecorate)
		{
			return true;
		}
			
		public virtual bool ExecuteFor(CheckPointController controller, CheckPoint checkPoint)
		{
			MsgBox (string.Format ("Checkpoint:{0}", checkPoint.Name), this.Name);
			return false;
		}

		protected void MsgBox(string title,string message)
		{
			new UIAlertView (title, message,null, "OK", null).Show ();
		}

		public virtual UIPreviewAction AsPreviewAction(Action<Command> handler,UIPreviewActionStyle style)
		{
			var title = string.IsNullOrEmpty(this.LongName) ? this.Name : this.LongName;
			return UIPreviewAction.Create (title,style,
				(a,c)=>
				{
					handler(this);	
				}
			);
		}

		public virtual UIPreviewAction AsPreviewAction(Action<Command> handler )
		{
			return this.AsPreviewAction (handler, 
				(this.IsDestructive)?
				UIPreviewActionStyle.Destructive:
				UIPreviewActionStyle.Default);	
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

