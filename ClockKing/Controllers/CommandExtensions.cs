using System;
using Foundation;
using UIKit;
using SWTableViewCells;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;
using MonoTouch.Dialog;

namespace ClockKing
{
	public static class CommandExtensions
	{
		public static UIAlertAction AsAlertAction(this Command cmd, Action<Command> handler)
		{

			var title = string.IsNullOrEmpty(cmd.LongName) ? cmd.Name : cmd.LongName;

			return UIAlertAction.Create(
				title,
				cmd.IsDestructive?UIAlertActionStyle.Destructive:UIAlertActionStyle.Default,
				(a)=>handler(cmd));
		}

		public static UIPreviewAction AsPreviewAction(this Command cmd, Action<Command> handler,UIPreviewActionStyle style)
		{
			var title = string.IsNullOrEmpty(cmd.LongName) ? cmd.Name : cmd.LongName;

			return UIPreviewAction.Create (title,style,(a,c)=>{handler(cmd);});
		}

		public static UIPreviewAction AsPreviewAction(this Command cmd,Action<Command> handler )
		{
			return cmd.AsPreviewAction (handler, 
				(cmd.IsDestructive)?
				UIPreviewActionStyle.Destructive:
				UIPreviewActionStyle.Default);	
		}
		public static UIButton AsButton(this Command cmd)
		{

			var button = new UIButton (UIButtonType.Custom);
			button.BackgroundColor =  ColorLookup[cmd.ColorName];
			button.SetTitle (cmd.Name, UIControlState.Normal);
			button.SetTitleColor (UIColor.White, UIControlState.Normal);
			button.TitleLabel.AdjustsFontSizeToFitWidth = true;
			return button;

		}
		public static Dictionary<string,UIColor> ColorLookup
		{

			get{
				return new Dictionary<string,UIColor> () 
				{
					{"Black",UIColor.Black},
					{"Blue",UIColor.Blue},
					{"Brown",UIColor.Brown},
					{"Cyan",UIColor.Cyan},
					{"DarkGray",UIColor.DarkGray},
					{"Gray",UIColor.Gray},
					{"Green",UIColor.Green},
					{"LightGray",UIColor.LightGray},
					{"Magenta",UIColor.Magenta},
					{"Orange",UIColor.Orange},
					{"Purple",UIColor.Purple},
					{"Red",UIColor.Red},
					{"White",UIColor.White},
					{"Yellow",UIColor.Yellow}
				};
			}
		}
	}
}

