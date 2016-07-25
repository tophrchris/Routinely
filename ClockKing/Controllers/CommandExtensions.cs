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

			return UIAlertAction.Create(
				cmd.LongNameWithEmoji,
				cmd.IsDestructive?UIAlertActionStyle.Destructive:UIAlertActionStyle.Default,
				(a)=>handler(cmd));
		}

		public static UIPreviewAction AsPreviewAction(this Command cmd, Action<Command> handler,UIPreviewActionStyle style)
		{
			return UIPreviewAction.Create (cmd.LongNameWithEmoji,style,(a,c)=>{handler(cmd);});
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
			button.SetTitle (cmd.NameWithEmoji, UIControlState.Normal);
			button.SetTitle (cmd.Name, UIControlState.Application);
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
					{"LightBlue",UIColor.FromRGB(102,205,170)},
					{"Blue",UIColor.FromRGB(95,158,160)},
					{"Brown",UIColor.Brown},
					{"Cyan",UIColor.Cyan},
					{"DarkGray",UIColor.DarkGray},
					{"Gray",UIColor.Gray},
					{"Green",UIColor.Green},
					{"LightGray",UIColor.LightGray},
					{"Magenta",UIColor.FromRGB(221,160,221)},
					{"Orange",UIColor.Orange},
					{"Purple",UIColor.Purple},
					{"Red",UIColor.FromRGB(205,92,92)},
					{"White",UIColor.White},
					{"Yellow",UIColor.Yellow}
				};
			}
		}
	}
}

