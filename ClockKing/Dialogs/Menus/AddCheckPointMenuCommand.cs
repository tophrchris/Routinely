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
		protected CheckPointManager CommandController{ get; set; }
		protected UIBarButtonItem BarButton{ get; set; }

		public AddCheckPointMenuCommand(CheckPointManager controller)
		{
			this.CommandController = controller;
			this.BarButton = new UIBarButtonItem ("+", UIBarButtonItemStyle.Bordered,(sender, args) => this.ShowDialog());
		}

		public UIBarButtonItem MenuButton{get{return this.BarButton;}}

		public void ShowDialog()
		{

			iNavigatableDialog mtd;

			if (ClockKingOptions.ShowExampleBrowser)
				mtd = new SampleGoalBrowsingDialog();
			else
				mtd = new CheckPointEditingDialog(this.CommandController, new RootElement("Add..."), true);
			
			this.CommandController.NavigateToDialog (mtd);
		}
	}
}

