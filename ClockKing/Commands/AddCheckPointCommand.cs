using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Model;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Dialog;

namespace ClockKing.Commands
{
	public class AddCheckPointCommand
	{
		protected UIViewController Controller{ get; set; }
		protected UIBarButtonItem BarButton{ get; set; }

		public AddCheckPointCommand(UIViewController controller)
		{
			this.Controller = controller;
			this.BarButton = new UIBarButtonItem ("+", UIBarButtonItemStyle.Bordered,(sender, args) => this.ShowAddCheckPointDialog());
		}

		public UIBarButtonItem Button{get{return this.BarButton;}}

		protected void ShowAddCheckPointDialog()
		{
			var root = new RootElement("Add...");
			var section = new Section ("New Checkpoint:");
			var nameElement = new EntryElement ("name", "Name your checkpoint", "");
			var instructions = new MultilineElement ("specify the time that you expect to complete this checkpoint, each day:");
			var emojiElement = new EntryElement ("Emoji", "specify some emoji :P","");
			var targetElement = new TimeElement ("target", DateTime.Now);
			var nowElement = new BooleanElement ("Add Occurrence now?", false);

			Action dialogCancellation = () => this.Controller.NavigationController.PopViewController (true);

			Action saveAction = () => { 
				var checkPointController = this.Controller as CheckPointController;

				var newcp = checkPointController
					.AddNewCheckPoint (
					            nameElement.Value,
					            targetElement.DateValue.ToLocalTime ().TimeOfDay,
					            emojiElement.Value);

				if (nowElement.Value)
					newcp.AddOccurrence (newcp.CreateOccurrence ());

				dialogCancellation ();
			};

			section.AddAll (new Element[]{ 	nameElement,
											instructions,
											emojiElement,
											targetElement,
											nowElement });
			root.Add(section);

			var mtd = new DialogViewController (root,true);
			this.Controller.NavigationController.PushViewController (mtd,true);
			mtd.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Save,(s,e)=>saveAction()),true);
		}
	}
}

