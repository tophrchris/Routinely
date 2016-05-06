using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using MonoTouch.Dialog;

namespace ClockKing
{
	partial class AddNewCheckPointController : DialogViewController
	{
		public CheckPointTableViewController Parent { get; set; }

		public AddNewCheckPointController (IntPtr handle) : base (handle)
		{

		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var section = new Section ("New Checkpoint:");
				
			var nameElement = new EntryElement ("name", "Name your checkpoint", "");
			var instructions = new MultilineElement ("specify the time that you expect to complete this checkpoint, each day:");

			var embedded = new RootElement ("target time");
			var targetElement = new TimeElement ("target", DateTime.Now);
			embedded.Add (new Section (){targetElement});

			var saveButton = new StringElement ("save", 
				() =>{ 
					this.Parent.AddNewCheckPoint(nameElement.Value,targetElement.DateValue.TimeOfDay);
					this.DismissModalViewController(true);
				}){Alignment=UITextAlignment.Right};

			var cancelButton = new StringElement ("cancel", 
				() =>{ 
					this.DismissModalViewController(true);
				}){Alignment=UITextAlignment.Right};


			section.AddAll (new Element[]{ nameElement, instructions, embedded, saveButton,cancelButton });
			

			this.Root.Add (section);



			this.NavigationItem.SetLeftBarButtonItem (
				new UIBarButtonItem ("cancel", UIBarButtonItemStyle.Done, 
					(so,e)=>this.PerformSegue("Return",this)), true);
			
				
			this.NavigationItem.SetRightBarButtonItem (
				new UIBarButtonItem ("save",UIBarButtonItemStyle.Plain,null), true);
		}
	}
}



/*Next steps

Setup/getting started
Familiar places
The importance of patterns
Gotchas*/