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

			this.BarButton = new UIBarButtonItem ("+", UIBarButtonItemStyle.Bordered,
				(sender, args) => this.ShowAddCheckPointDialog());
		}

		public UIBarButtonItem Button{get{return this.BarButton;}}

		protected void ShowAddCheckPointDialog()
		{
			var section = new Section ("New Checkpoint:");

			var nameElement = new EntryElement ("name", "Name your checkpoint", "");
			var instructions = new MultilineElement ("specify the time that you expect to complete this checkpoint, each day:");

			var targetElement = new TimeElement ("target", DateTime.Now);

			Action dialogCancellation = () => this.Controller.NavigationController.PopViewController (true);

			var saveButton = new StringElement ("save", 
				() =>{ 

					((CheckPointController)this.Controller).AddNewCheckPoint(nameElement.Value,targetElement.DateValue.TimeOfDay);

					dialogCancellation();

				}){Alignment=UITextAlignment.Right};

			var cancelButton = new StringElement ("cancel",dialogCancellation ){Alignment=UITextAlignment.Right};

			section.AddAll (new Element[]{ nameElement, instructions, targetElement, saveButton,cancelButton });
			var root = new RootElement("Add...");
			root.Add(section);

			var mtd = new DialogViewController (root,true);
			this.Controller.NavigationController.PushViewController (mtd,true);
		}

	}
}

