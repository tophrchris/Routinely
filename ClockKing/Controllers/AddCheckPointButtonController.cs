using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Model;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Dialog;
using ClockKing.Extensions;

namespace ClockKing.Commands
{
	public class AddCheckPointButtonController
	{
		protected CheckPointController Controller{ get; set; }
		protected UIBarButtonItem BarButton{ get; set; }

		public AddCheckPointButtonController(CheckPointController controller)
		{
			this.Controller = controller;
			this.BarButton = new UIBarButtonItem ("+", UIBarButtonItemStyle.Bordered,(sender, args) => this.ShowAddCheckPointDialog());
		}

		public UIBarButtonItem Button{get{return this.BarButton;}}

		public void ShowAddCheckPointDialog()
		{

			var root = new RootElement ("Add...");
			var mtd = new AddNewCheckpointDialog (this.Controller, root,true);
			this.Controller.NavigationController.PushViewController (mtd,true);
		}
	}
}

