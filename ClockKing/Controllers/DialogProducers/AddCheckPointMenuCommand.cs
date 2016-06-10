using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Dialog;
using ClockKing.Extensions;

namespace ClockKing.Commands
{
	public class AddCheckPointMenuCommand
	{
		protected CheckPointController Controller{ get; set; }
		protected UIBarButtonItem BarButton{ get; set; }

		public AddCheckPointMenuCommand(CheckPointController controller)
		{
			this.Controller = controller;
			this.BarButton = new UIBarButtonItem ("+", UIBarButtonItemStyle.Bordered,(sender, args) => this.ShowDialog());
		}

		public UIBarButtonItem MenuButton{get{return this.BarButton;}}

		public void ShowDialog()
		{

			var root = new RootElement ("Add...");
			var mtd = new CheckPointEditingDialog (this.Controller, root,true);
			this.Controller.NavigationController.PushViewController (mtd,true);
		}
	}
}

